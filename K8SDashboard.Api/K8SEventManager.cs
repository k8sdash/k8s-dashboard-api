using K8SDashboard.Models;
using K8SDashboard.Services;
using Microsoft.AspNetCore.SignalR;

namespace K8SDashboard.Api
{
    public class K8SEventManager
    {
        private readonly ILogger<K8SEventManager> logger;
        private readonly IHubContext<LightRoutesHub, IHubClient> hubContext;
        private readonly IK8SClientService k8SClientService;

        public K8SEventManager(ILogger<K8SEventManager> logger, IHubContext<LightRoutesHub, IHubClient> hubContext, IK8SClientService k8SClientService)
        {
            this.logger = logger;
            this.hubContext = hubContext;
            this.k8SClientService = k8SClientService;
        }

        public void Start()
        {
            k8SClientService.K8sPodChanged += K8SClientService_K8sPodChanged;
        }

        private void K8SClientService_K8sPodChanged(object? sender, EventArgs e)
        {
            if(e==null)
            {
                logger.LogWarning("Unexptected Null Event");
                return;
            }
            var k8sEvent = (K8SPodEventArgs)e;
            logger.LogDebug("aSignalR EventHub picked K8S event of Type '{Type}' on Pod '{Pod}'", k8sEvent.EventType, k8sEvent.PodName);
            hubContext.Clients.All.Propagate(k8sEvent.PodName, k8sEvent.EventType);
        }
    }
}
