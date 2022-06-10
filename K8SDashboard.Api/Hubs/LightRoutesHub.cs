using Microsoft.AspNetCore.SignalR;


namespace K8SDashboard.Api
{
    public class LightRoutesHub : Hub
    {
        public async Task SendMessage(string pod, string eventType)
        {
            await Clients.All.SendAsync("ReceiveMessage", pod, eventType);
        }
    }
}
