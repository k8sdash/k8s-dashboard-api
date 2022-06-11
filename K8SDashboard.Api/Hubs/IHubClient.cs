namespace K8SDashboard.Api
{
    public interface IHubClient
    {
        Task Propagate(string? pod, string? eventType);
    }
}