using Codeable.Foundation.Common;
using Codeable.Foundation.Common.Daemons;
using Codeable.Foundation.UI.Web.Core.MVC;
using Codeable.Foundation.Web.Core.MVC.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Stencil.Plugins.SystemMonitor.Controllers
{
    public class DaemonsController : CoreController
    {
        public DaemonsController(IFoundation iFoundation)
            : base(iFoundation)
        {
        }

        [NoClientCache()]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Index(string name = "", string op = "")
        {
            return base.ExecuteFunction("Index", delegate ()
            {
                string message = string.Empty;
                IDaemonManager daemonManager = this.IFoundation.GetDaemonManager();
                if (op == "removeall")
                {
                    daemonManager.UnRegisterAllDaemons();
                    message = "RemovedAll";
                }
                else if (op == "remove")
                {
                    daemonManager.UnRegisterDaemon(name);
                    message = "Removed: " + name;
                }
                else if (op == "stop")
                {
                    daemonManager.StopDaemon(name);
                    message = "Stopped: " + name;
                }
                else if (op == "start")
                {
                    daemonManager.StartDaemon(name);
                    message = "Started: " + name;
                }

                ViewBag.Message = message;
                return View(daemonManager.GetAllTimerDetails().OrderBy(x => x.Name).ToList());
            });
        }
    }
}
