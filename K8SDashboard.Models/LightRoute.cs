namespace K8SDashboard.Models
{
    public class LightRoute
    {
        public string? Ingress { get; set; }
        public string? NameSpace {  get; set; }
        public string? Service { get; set; }
        public string? App { get; set; }
        public string? Node { get; set; }
        public string? NodeIp {  get; set; }
        public string? PodPort { get; set;}
        public string? NodeAz { get; set; }
        public string? Pod {  get; set; }
        public string? PodIp { get; set;  }
        public string? PodPhase {  get; set; }
        public string? Image {  get; set; }
}
}