using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Plugins.Amazon.Models.Amazon
{
    public class AmazonInputInfo
    {
        public string key { get; set; }
        public string frameRate { get; set; }
        public string resolution { get; set; }
        public string aspectRatio { get; set; }
        public string interlaced { get; set; }
        public string container { get; set; }
    }
}
