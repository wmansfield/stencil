using Codeable.Foundation.Common;
using Codeable.Foundation.UI.Web.Core;
using Codeable.Foundation.UI.Web.Core.MVC;
using Codeable.Foundation.Web.Core.MVC.Attributes;
using Stencil.Primary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace Stencil.Plugins.SystemMonitor.Controllers
{
    public class ErrorsController : CoreController
    {
        public ErrorsController(IFoundation foundation)
            : base(foundation)
        {
        }



        [AllowAnonymous]
        [NoClientCache(), OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Index(bool delete = false)
        {
            return base.ExecuteFunction<ActionResult>("Index", delegate ()
            {
                string fileName = Path.Combine(Path.GetTempPath(), @"StencilErrorTracking\Error.Log");
                if (System.IO.File.Exists(fileName))
                {
                    if (delete)
                    {
                        System.IO.File.WriteAllText(fileName, "cleared");
                        return Json(new { success = true, message = "deleted" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return File(fileName, "text");
                    }
                }

                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            });
        }

        [AllowAnonymous]
        [NoClientCache(), OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Test(bool delete = false)
        {
            return base.ExecuteFunction<ActionResult>("Test", delegate ()
            {
                string fileName = Path.Combine(Path.GetTempPath(), @"StencilErrorTracking\Error.Log");
                if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                }

                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            });
        }


        [AllowAnonymous]
        [NoClientCache(), OutputCache(NoStore = true, Duration = 0)]
        public ActionResult RestartIf(string name = "")
        {
            bool restarted = false;
            if (name == Environment.MachineName)
            {
                if (WebCoreUtility.GetTrustLevel() > AspNetHostingPermissionLevel.Medium)
                {
                    HttpRuntime.UnloadAppDomain();
                    restarted = true;
                }
                else
                {
                    bool wroteToWebConfig = false;
                    try
                    {
                        System.IO.File.SetLastWriteTimeUtc(HostingEnvironment.MapPath("~/web.config"), DateTime.UtcNow);
                        wroteToWebConfig = true;
                    }
                    catch { }

                    restarted = wroteToWebConfig;
                }
            }
            return Json(new { machine = Environment.MachineName, restarted = restarted }, JsonRequestBehavior.AllowGet);
        }
    }
}