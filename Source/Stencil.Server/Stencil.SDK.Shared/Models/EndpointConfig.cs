using System;
using System.Collections.Generic;
using System.Text;

namespace Stencil.SDK.Models
{
    public class EndpointConfig
    {
        public EndpointConfig()
        {

        }
        public int StaleSeconds { get; set; }
        public int PageSize { get; set; }
        public int ScrollThresholdSize { get; set; }
        public int ScrollThresholdCount { get; set; }
    }
}
