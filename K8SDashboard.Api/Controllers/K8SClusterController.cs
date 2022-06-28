using K8SDashboard.Models;
using K8SDashboard.Services;
using Microsoft.AspNetCore.Mvc;

namespace K8SDashboard.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]", Order = 1)]
    [Route("api/v{version:apiVersion}/[controller]", Order = 2)]
    [ApiVersion("1.0")]
    public class K8SClusterController : ControllerBase
    {
        private readonly IK8SClientService k8SClientService;
        private readonly ILogger<K8SClusterController> logger;
        private readonly AppSettings appSettings;

        public K8SClusterController(ILogger<K8SClusterController> logger, AppSettings appSettings, IK8SClientService k8SClientService)
        {
            this.logger = logger;
            this.appSettings = appSettings;
            this.k8SClientService = k8SClientService;
        }

        [HttpGet]
        [Route("lightRoutes")]
        public async Task<ActionResult<List<LightRoute>>> GetLightRoutes()
        {
            try
            {
                logger.LogDebug("controller will get lightRoutes from service...");
                var lightRoutes = await k8SClientService.ListLightRoutesWithTimeOut(2);
                if (lightRoutes != null)
                {
                    logger.LogTrace("Got {Count} Light Routes", lightRoutes.Count);
                    return Ok(lightRoutes);
                }
                return BadRequest("Unable to get routes. Please check the logs");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "There was a problem getting Light Routes");
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("lightRoutesGrouped")]
        public async Task<ActionResult<List<LightRoute>>> GetLightRoutesGrouped()
        {
            // aggrid-enterprise offers grouping - given that the k8s-dashboard-client is using aggrid-community this is a grouping proxy
            try
            {
                logger.LogDebug("controller will get grouped lightRoutes from service...");
                var lightRoutes = await k8SClientService.ListLightRoutesWithTimeOut(2);
                if (lightRoutes != null)
                {
                    logger.LogTrace("Got {Count} Grouped Light Routes", lightRoutes.Count);
                    var groups = lightRoutes.GroupBy(p => (p.App, p.NodeIp, p.PodIp, p.PodPhase, p.Image, p.Ingress, p.NameSpace, p.Node, p.NodeAz, p.Pod));


                    var groupedLightRoutes = groups.Select(g => new LightRoute {
                        Id = Guid.NewGuid(),
                        App = g.Key.App,
                        NodeIp = g.Key.NodeIp,
                        PodIp = g.Key.PodIp,
                        Ingress = g.Key.Ingress,
                        Image = g.Key.Image,
                        NameSpace = g.Key.NameSpace,
                        Node = g.Key.Node, 
                        NodeAz = g.Key.NodeAz,
                        Pod = g.Key.Pod,
                        PodPhase = g.Key.PodPhase,
                        Service = Display(g.Select(p=>p.Service).ToArray()),
                        PodPort = Display(g.Select(p => p.PodPort).ToArray())
                    });

                    return Ok(groupedLightRoutes);
                }
                return BadRequest("Unable to get grouped routes. Please check the logs");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "There was a problem getting Grouped Light Routes");
                return BadRequest(ex);
            }
        }

        private string Display(IEnumerable<string> inputs)
        {
            if (inputs == null || !inputs.Any())
                return String.Empty;
            string[] a = inputs.SelectMany(x => x.Contains(appSettings.DisplaySeparator)? x.Split(appSettings.DisplaySeparator) : new string[] { x }).Distinct().OrderBy(p=>p).ToArray();
            return string.Join(appSettings.DisplaySeparator, a);
        }
    }
}
