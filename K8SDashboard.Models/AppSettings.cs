using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K8SDashboard.Models
{
    public class AppSettings
    {
        public string DefaultNamespace { get; set; }
        public int KubeApiTimeout { get; set; }
        public string K8sLabelApp { get; set; }
        public string K8sLabelInternalIp { get; set; }
        public string K8sLabelAksZone { get; set; }
    }
}
