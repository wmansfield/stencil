using Codeable.Foundation.Common;
using Codeable.Foundation.UI.Web.Core.MVC;
using Codeable.Foundation.Web.Core.MVC.Attributes;
using Stencil.Common.Configuration;
using Stencil.Plugins.SystemMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Stencil.Plugins.SystemMonitor.Controllers
{
    public class VersionController : CoreController
    {
        public VersionController(IFoundation foundation)
            : base(foundation)
        {
        }

        [NoClientCache(), OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Index()
        {
            return base.ExecuteFunction("Index", delegate ()
            {
                ISettingsResolver resolver = this.IFoundation.Resolve<ISettingsResolver>();

                VersionInfo info = new VersionInfo();
                info.UIVersion = resolver.GetSetting("Stencil-SDK-Version");
                info.BuildDate = System.IO.File.ReadAllText(Server.MapPath("~/_build.txt")); ;
                return View(info);
            });
        }
    }
}