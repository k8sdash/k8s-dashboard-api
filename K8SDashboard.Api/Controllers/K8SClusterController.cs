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
                if(lightRoutes!= null)
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
    }
}
