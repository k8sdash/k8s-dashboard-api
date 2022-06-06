using K8SDashboard.Models;

namespace K8SDashboard.Services
{
    public class K8sClientService : IK8sClientService
    {
        public async Task<List<LightRoute>> ListLightRoutesWithTimeOut(object defaultNamespace, int retriesLeft)
        {
            return new List<LightRoute>();
        }
    }
}