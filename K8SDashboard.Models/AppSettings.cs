namespace K8SDashboard.Models
{
    public class AppSettings
    {
        public int KubeApiTimeout { get; set; }
        public string? K8sLabelApp1 { get; set; }
        public string? K8sLabelApp2 { get; set; }
        public string? K8sLabelInternalIp { get; set; }
        public string? K8sLabelAksZone { get; set; }

        public string? ApiTitle { get; set; }
        public string? ApiDescription { get; set; }
        public string? ApiContactName { get; set; }
        public string? ApiContactUrl { get; set; }
        public string? ApiLicenseName { get; set; }
        public string? ApiLicenseUrl { get; set; }
        public string? DisplaySeparator { get; set; }
    }
}
