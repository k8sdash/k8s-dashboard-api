using K8SDashboard.Models;
using Microsoft.Extensions.Logging;
using k8s;
using k8s.Models;

namespace K8SDashboard.Services
{
    public class K8SClientService : IK8SClientService
    {
        private const string Message = "Getting {ObjectType} in namespace {Namespace} with TimeOut {TimeOut} sec... [{Retries} Retries left]...";
        private readonly ILogger<K8SClientService> logger;
        private readonly AppSettings appSettings;
        private readonly Kubernetes client;

        public K8SClientService(ILogger<K8SClientService> logger, AppSettings appSettings)
        {
            this.logger = logger;
            this.appSettings = appSettings;
            var config = KubernetesClientConfiguration.IsInCluster()? KubernetesClientConfiguration.InClusterConfig() : KubernetesClientConfiguration.BuildConfigFromConfigFile(); 
            client = new Kubernetes(config);
        }

        public async Task<List<LightRoute>> ListLightRoutesWithTimeOut(string ns, int retriesLeft)
        {
            try
            {
                var nodes = await Get<V1Node>(ns, retriesLeft, GetNodes);
                var pods = await Get<V1Pod>(ns, retriesLeft, GetPods);
                var services = await Get<V1Service>(ns, retriesLeft, GetServices);
                var ingresses = await Get<V1Ingress>(ns, retriesLeft, GetIngresses);
                var rules = ingresses.SelectMany(p => p.Spec.Rules);
                logger.LogDebug("Extracted {CountRules} rules from {CountIngresses} ingresses", rules.Count(), ingresses.Count);

                var joinedData =
                    from n in nodes
                    join p in pods
                    on n.Metadata.Name equals p.Spec.NodeName
                    //join s in services
                    //on r.Http.Paths.First().Backend.Service.Name equals s.Metadata.Name
                    //join r in rules
                    //on s.Spec.Selector.ContainsKey(appSettings.K8sLabelApp) ? s.Spec.Selector[appSettings.K8sLabelApp] : string.Empty equals p.Metadata.Labels.ContainsKey(appSettings.K8sLabelApp) ? p.Metadata.Labels[appSettings.K8sLabelApp] : string.Empty
                    select new LightRoute() {
                        Node = p.Spec.NodeName,
                        NodeIp = string.Join(",", n.Status.Addresses?.Where(p => p.Type == appSettings.K8sLabelInternalIp).Select(p => p.Address)),
                        //PodPort = string.Join(",", s.Spec.Ports?.Select(p => p.Port)),
                        NodeAz = n.Metadata.Labels.ContainsKey(appSettings.K8sLabelAksZone) ? n.Metadata.Labels[appSettings.K8sLabelAksZone] : string.Empty,
                        Pod = p.Metadata.Name,
                        PodIp = p.Status.PodIP,
                        Image = string.Join(",", p.Spec.Containers.Select(p => p.Image)),
                        PodPhase = p.Status.Phase,
                        //Name = string.Join(",", r.Host),
                        //NameSpace = s.Metadata.NamespaceProperty,
                        //Service = s.Metadata.Name,
                        App = p.Metadata.Labels.ContainsKey(appSettings.K8sLabelApp) ? p.Metadata.Labels[appSettings.K8sLabelApp] : string.Empty
                    };
                var lightRoutes = joinedData.ToList();
                logger.LogDebug("Made {Count} LightRoutes joining data from kubeAPI", lightRoutes.Count);
                return lightRoutes;
            }
            catch (k8s.Autorest.HttpOperationException ex)
            {
                if (ex.Response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    logger.LogWarning(ex, "Impossible to load. The current service account might be missing permissions.");
                return null;
            }
            catch (Exception ex)
            {
                if (retriesLeft > 0)
                    return await ListLightRoutesWithTimeOut(ns, retriesLeft - 1);
                else
                    logger.LogWarning(ex, "Impossible to load.");
                return null;
            }
        }

        private async Task<IList<V1Pod>> GetPods(string ns) => (await client.ListNamespacedPodAsync(ns))?.Items;
        private async Task<IList<V1Service>> GetServices(string ns) => (await client.ListNamespacedServiceAsync(ns))?.Items;
        private async Task<IList<V1Node>> GetNodes(string ns) => (await client.ListNodeAsync())?.Items;
        private async Task<IList<V1Ingress>> GetIngresses(string ns) => (await client.ListNamespacedIngressAsync(ns))?.Items;

        private async Task<IList<T>> Get<T> (string ns, int retriesLeft, Func<string, Task<IList<T>>> func)
        {
            logger.LogDebug(Message, typeof(T), ns, appSettings.KubeApiTimeout, retriesLeft);
            var list = await func(ns);
            if (list == null || !list.Any())
            {
                logger.LogWarning("Unable to collect {Type} from kubeAPI.", typeof(T));
                return new List<T>();
            }
            logger.LogDebug("Found {Count} {Type} in namespace {Namespace}", list.Count, typeof(T), ns);
            return list;
        }
    }
}