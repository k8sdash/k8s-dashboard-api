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
                var nodes = await Get<V1Node>(retriesLeft, GetNodes);
                var pods = await Get<V1Pod>(retriesLeft, GetPods);
                var services = await Get<V1Service>(retriesLeft, GetServices);
                var ingresses = await Get<V1Ingress>(retriesLeft, GetIngresses);
                var rules = ingresses.SelectMany(p => p.Spec.Rules);
                logger.LogDebug("Extracted {CountRules} rules from {CountIngresses} ingresses", rules.Count(), ingresses.Count);

                var nodePodServices =
                    from n in nodes
                    join p in pods
                    on n.Metadata.Name equals p.Spec.NodeName
                    join s in services
                    on p.Metadata.Labels.ContainsKey(appSettings.K8sLabelApp) ? p.Metadata.Labels[appSettings.K8sLabelApp] : string.Empty equals s.Spec.Selector != null && s.Spec.Selector.ContainsKey(appSettings.K8sLabelApp) ? s.Spec.Selector?[appSettings.K8sLabelApp] : string.Empty into joined
                    from j in joined.DefaultIfEmpty()
                    select new { p, n, s=j };

                var nodePodServiceRules =
                    from nps in nodePodServices
                    join r in rules
                    on nps.s.Metadata.Name equals r.Http.Paths.First().Backend.Service.Name into joined 
                    from j in joined.DefaultIfEmpty()
                    select new { nps.p, nps.n, nps.s, r=j};

                var lightRoutes = nodePodServiceRules.Select(x =>  new LightRoute() {
                        Node = x.p.Spec.NodeName,
                        NodeIp = string.Join(",", x.n.Status.Addresses?.Where(p => p.Type == appSettings.K8sLabelInternalIp).Select(p => p.Address)),
                        PodPort = string.Join(",", x.s.Spec.Ports?.Select(p => p.Port)),
                        NodeAz = x.n.Metadata.Labels.ContainsKey(appSettings.K8sLabelAksZone) ? x.n.Metadata.Labels[appSettings.K8sLabelAksZone] : string.Empty,
                        Pod = x.p.Metadata.Name,
                        PodIp = x.p.Status.PodIP,
                        Image = string.Join(",", x.p.Spec.Containers.Select(p => p.Image)),
                        PodPhase = x.p.Status.Phase,
                        Ingress = string.Join(",", x.r?.Host),
                        NameSpace = x.p.Metadata.NamespaceProperty,
                        Service = x.s.Metadata.Name,
                        App = x.p.Metadata.Labels.ContainsKey(appSettings.K8sLabelApp) ? x.p.Metadata.Labels[appSettings.K8sLabelApp] : string.Empty
                    }).ToList();
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

        private async Task<IList<V1Pod>> GetPods() => (await client.ListPodForAllNamespacesAsync())?.Items;
        private async Task<IList<V1Service>> GetServices() => (await client.ListServiceForAllNamespacesAsync())?.Items;
        private async Task<IList<V1Node>> GetNodes() => (await client.ListNodeAsync())?.Items;
        private async Task<IList<V1Ingress>> GetIngresses() => (await client.ListIngressForAllNamespacesAsync())?.Items;

        private async Task<IList<T>> Get<T> (int retriesLeft, Func<Task<IList<T>>> func)
        {
            logger.LogDebug(Message, typeof(T), appSettings.KubeApiTimeout, retriesLeft);
            var list = await func();
            if (list == null || !list.Any())
            {
                logger.LogWarning("Unable to collect {Type} from kubeAPI.", typeof(T));
                return new List<T>();
            }
            logger.LogDebug("Found {Count} {Type} in namespace {Namespace}", list.Count, typeof(T));
            return list;
        }
    }
}