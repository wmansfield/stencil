using Codeable.Foundation.Common.Plugins;
using Codeable.Foundation.UI.Web.Common.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stencil.Plugins.SystemMonitor.Models
{
    public class PluginInfo
    {
        public List<IFoundationPlugin> FoundationPlugins { get; set; }

        public List<IWebPlugin> WebPlugins { get; set; }
    }
}