using K8SDashboard.Models;
using Microsoft.Extensions.Logging;
using k8s;
using k8s.Models;

namespace K8SDashboard.Services
{
    public class K8SClientService : IK8SClientService
    {
        private const string Message = "Getting {ObjectType} with TimeOut {TimeOut} sec... [{Retries} Retries left]...";
        private readonly ILogger<K8SClientService> logger;
        private readonly AppSettings appSettings;
        private readonly Kubernetes client;

        public event EventHandler K8sPodChanged;

        public K8SClientService(ILogger<K8SClientService> logger, AppSettings appSettings)
        {
            this.logger = logger;
            this.appSettings = appSettings;
            KubernetesClientConfiguration config;
            if (KubernetesClientConfiguration.IsInCluster())
            {
                config = KubernetesClientConfiguration.InClusterConfig();
                logger.LogDebug("detected running in kubernetes cluster");
            }
            else
                try
                {
                    logger.LogDebug("NOT running IN kubernetes cluster...");
                    config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
                }
                catch (k8s.Exceptions.KubeConfigException ex)
                {

                    logger.LogCritical(ex, "Unable to proceed! Not running in kubernetes, and unable to find a default KubeConfig");
                    return;
                } 
            client = new Kubernetes(config);
            WatchPodsUsingCallback();
        }

        public bool Valid()
        {
            return client != null;
        }

        private async Task WatchPodsUsingCallback()
        {
            var pods = await client.CoreV1.ListPodForAllNamespacesWithHttpMessagesAsync(watch: true);
            using (pods.Watch((Action<WatchEventType, V1Pod>)((type, item) => { K8SPodEvent(type, item); })))
            {
                var manualResetEventSlim = new ManualResetEventSlim(false);
                manualResetEventSlim.Wait();
            }
        }

        private void K8SPodEvent(WatchEventType type, V1Pod item)
        {
            OnK8sPodChanged(new K8SPodEventArgs() { EventType = type.ToString(), PodName = item.Metadata.Name });
            logger.LogInformation("Received Event '{@Type}' on Pod '{Pod}' from kubeAPI", type, item.Metadata.Name);
        }

        private void OnK8sPodChanged(K8SPodEventArgs k8SPodEventArgs)
        {
            EventHandler handler = K8sPodChanged;
            handler?.Invoke(this, k8SPodEventArgs);
        }

        public async Task<List<LightRoute>> ListLightRoutesWithTimeOut(int retriesLeft)
        {
            try
            {
                var nodes = await Get(retriesLeft, GetNodes);
                var pods = (await Get(retriesLeft, GetPods)).Where(p => p.Metadata.Labels.ContainsKey(appSettings.K8sLabelApp1) || p.Metadata.Labels.ContainsKey(appSettings.K8sLabelApp2)).ToList() ;
                var services = (await Get(retriesLeft, GetServices)).Where(s=> s.Spec.Selector != null && (s.Spec.Selector.ContainsKey(appSettings.K8sLabelApp1) || s.Spec.Selector.ContainsKey(appSettings.K8sLabelApp2))).ToList();

                var nodePodServices =
                    from n in nodes
                    join p in pods
                    on n.Metadata.Name equals p.Spec.NodeName
                    join s in services
                    on new
                    {
                        Label = p.Metadata.Labels.ContainsKey(appSettings.K8sLabelApp1) ? p.Metadata.Labels[appSettings.K8sLabelApp1] : p.Metadata.Labels[appSettings.K8sLabelApp2], 
                        Namespace = p.Namespace()
                    } equals new
                    {
                        Label = s.Spec.Selector.ContainsKey(appSettings.K8sLabelApp1) ? s.Spec.Selector[appSettings.K8sLabelApp1] : s.Spec.Selector[appSettings.K8sLabelApp2],
                        Namespace = s.Namespace()
                    } into joined
                    from j in joined.DefaultIfEmpty()
                    select new { p, n, s = j ?? new V1Service() };
                logger.LogDebug("Joined nods, pods and services. Counting {Count}", nodePodServices?.Count());

                var ingresses = await Get(retriesLeft, GetIngresses);
                var rules = ingresses.Select(p => p.Spec.Rules.Select(q => new { NameSpace = p.Namespace(), Rules = q })).SelectMany(p =>p);
                var paths = rules.Select(p => p.Rules.Http.Paths.Select(q => new { NameSpace = p.NameSpace, Paths = q, Rules = p.Rules })).SelectMany(p => p);
                
                var nodePodServicePaths =
                    from nps in nodePodServices
                    join pa in paths
                    on new { Name = nps.s.Metadata?.Name, NameSpace = nps.s.Namespace() } equals new { Name = pa.Paths.Backend?.Service?.Name, NameSpace = pa.NameSpace } into joined
                    from j in joined.DefaultIfEmpty()
                    select new { nps.p, nps.n, nps.s, host = j == null ? String.Empty : j.Rules.Host};
                logger.LogDebug("Joined nods, pods, services and hosts. Counting {Count}", nodePodServicePaths.Count());

                var lightRoutes = nodePodServicePaths.Select(x =>
                {
                    try
                    {
                        var lightRoute = new LightRoute()
                        {
                            Id = Guid.NewGuid(),
                            Node = x.p.Spec.NodeName,
                            NodeIp = string.Join(appSettings.DisplaySeparator, x.n.Status?.Addresses?.Where(p => p?.Type == appSettings.K8sLabelInternalIp).Select(p => p?.Address)),
                            PodPort = x.s.Spec != null && x.s.Spec.Ports != null && x.s.Spec.Ports.Any() ? string.Join(appSettings.DisplaySeparator, x.s.Spec.Ports.Select(p => p.Port).Distinct()) : string.Empty, // TODO probably want to show the protocol instead of hiding it
                            NodeAz = x.n.Metadata.Labels.ContainsKey(appSettings.K8sLabelAksZone) ? x.n.Metadata.Labels[appSettings.K8sLabelAksZone] : string.Empty,
                            Pod = x.p.Metadata.Name,
                            PodIp = x.p.Status.PodIP,
                            Image = string.Join(appSettings.DisplaySeparator, x.p.Spec?.Containers?.Select(p => p?.Image ?? string.Empty)),
                            PodPhase = x.p.Status.Phase,
                            Ingress = string.Join(appSettings.DisplaySeparator, x.host ),
                            NameSpace = x.p.Metadata.NamespaceProperty,
                            Service = x.s.Metadata?.Name
                        };
                        return lightRoute;
                    }
                    catch (Exception e)
                    {
                        logger.LogWarning(e, "Unable to build lightRoute");
                        return new LightRoute()
                        {
                            Id = Guid.NewGuid(),
                            Node = x.p.Spec.NodeName,
                            Pod = x.p.Metadata.Name,
                        };
                    }
                }).ToList();
                logger.LogDebug("Made {Count} LightRoutes joining data from kubeAPI", lightRoutes.Count);
                return lightRoutes;
            }
            catch (Exception ex)
            {
                if (retriesLeft > 0)
                    return await ListLightRoutesWithTimeOut(retriesLeft - 1);
                else
                    logger.LogWarning(ex, "Impossible to load.");
                return null;
            }
        }

        private async Task<IList<V1Pod>> GetPods() => (await client.ListPodForAllNamespacesAsync())?.Items;
        private async Task<IList<V1Service>> GetServices() => (await client.ListServiceForAllNamespacesAsync())?.Items;
        private async Task<IList<V1Node>> GetNodes() => (await client.ListNodeAsync())?.Items;
        private async Task<IList<V1Ingress>> GetIngresses() => (await client.ListIngressForAllNamespacesAsync())?.Items;

        private async Task<IList<T>> Get<T>(int retriesLeft, Func<Task<IList<T>>> func)
        {
            logger.LogDebug(Message, typeof(T), appSettings.KubeApiTimeout, retriesLeft);
            IList<T> list = new List<T>();
            try
            {
                list = await func();
                if (list == null || !list.Any())
                {
                    logger.LogWarning("Unable to collect {Type} from kubeAPI.", typeof(T));
                    return new List<T>();
                }
            logger.LogDebug("Found {Count} {Type}", list?.Count, typeof(T));
            }
            catch (k8s.Autorest.HttpOperationException ex)
            {
                if (ex.Response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    logger.LogError(ex, "Impossible to load {Type}. The current service account is likely missing permissions.", typeof(T));
            }
            return list;
        }
    }
}