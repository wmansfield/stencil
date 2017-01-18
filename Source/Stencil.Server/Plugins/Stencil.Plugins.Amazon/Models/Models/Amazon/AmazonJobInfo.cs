using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Plugins.Amazon.Models.Amazon
{
    public class AmazonJobInfo
    {
        public string state { get; set; }
        public string version { get; set; }
        public string jobId { get; set; }
        public string pipelineId { get; set; }
        public string outputKeyPrefix { get; set; }
        public AmazonInputInfo input { get; set; }
        public AmazonOutputInfo[] outputs { get; set; }

        public string messageDetails { get; set; }
    }
}
