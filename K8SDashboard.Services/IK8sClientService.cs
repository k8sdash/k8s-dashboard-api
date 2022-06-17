using K8SDashboard.Models;

namespace K8SDashboard.Services
{
    public interface IK8SClientService
    {
        event EventHandler K8sPodChanged;
        Task<List<LightRoute>> ListLightRoutesWithTimeOut(int retriesLeft);
        bool Valid();


    }
}