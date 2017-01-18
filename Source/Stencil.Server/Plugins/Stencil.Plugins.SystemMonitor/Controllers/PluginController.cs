using Codeable.Foundation.Common;
using Codeable.Foundation.Common.Plugins;
using Codeable.Foundation.UI.Web.Core.MVC;
using Codeable.Foundation.Web.Core.MVC.Attributes;
using Stencil.Plugins.SystemMonitor.Models;
using System.Linq;
using System.Web.Mvc;

namespace Stencil.Plugins.SystemMonitor.Controllers
{
    public class PluginController : CoreController
    {
        public PluginController(IFoundation iFoundation)
            : base(iFoundation)
        {
        }

        [NoClientCache(), OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Index()
        {
            return base.ExecuteFunction("Index", delegate ()
            {
                IPluginManager pluginManager = this.IFoundation.GetPluginManager();
                IWebPluginLoader webPluginLoader = this.IFoundation.Resolve<IWebPluginLoader>();

                PluginInfo result = new PluginInfo();
                result.FoundationPlugins = pluginManager.FoundationPlugins.ToList();
                result.WebPlugins = webPluginLoader.GetRegisteredPlugins().ToList();

                return View(result);
            });
        }
    }
}