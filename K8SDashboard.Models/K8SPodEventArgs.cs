namespace K8SDashboard.Models
{
    public class K8SPodEventArgs : EventArgs
    {
        public string? PodName { get; set; }
        public string? EventType { get; set;  }
    }
}
