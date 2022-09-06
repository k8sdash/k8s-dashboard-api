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
                var nodes = await Get<V1Node>(retriesLeft, GetNodes);
                var pods = (await Get<V1Pod>(retriesLeft, GetPods)).Where(p => p.Metadata.Labels.ContainsKey(appSettings.K8sLabelApp)) ;
                var services = (await Get<V1Service>(retriesLeft, GetServices)).Where(s=> s.Spec.Selector != null && s.Spec.Selector.ContainsKey(appSettings.K8sLabelApp));

                var nodePodServices =
                    from n in nodes
                    join p in pods
                    on n.Metadata.Name equals p.Spec.NodeName
                    join s in services
                    on p.Metadata.Labels[appSettings.K8sLabelApp] equals s.Spec.Selector?[appSettings.K8sLabelApp] into joined
                    from j in joined.DefaultIfEmpty()
                    select new { p, n, s = j ?? new V1Service() };
                logger.LogDebug("Joined nods, pods and services. Counting {Count}", nodePodServices?.Count());

                var ingresses = await Get<V1Ingress>(retriesLeft, GetIngresses);
                var rules = ingresses.SelectMany(p => p.Spec.Rules).Where(r=>r!=null);
                logger.LogDebug("Extracted {CountRules} rules from {CountIngresses} ingresses", rules.Count(), ingresses.Count);                
                var nodePodServiceRules =
                    from nps in nodePodServices
                    join r in rules
                    on nps.s.Metadata?.Name equals r.Http?.Paths?.First()?.Backend?.Service?.Name into joined
                    from j in joined.DefaultIfEmpty()
                    select new { nps.p, nps.n, nps.s, r = j ?? new V1IngressRule() };

                logger.LogDebug("Joined nods, pods, services and rules. Counting {Count}", nodePodServiceRules.Count());

                var lightRoutes = nodePodServiceRules.Select(x =>
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
                            Ingress = string.Join(appSettings.DisplaySeparator, x.r?.Host ?? string.Empty),
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