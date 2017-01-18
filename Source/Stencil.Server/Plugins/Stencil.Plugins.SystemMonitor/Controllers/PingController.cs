using Codeable.Foundation.Common;
using Codeable.Foundation.UI.Web.Core.MVC;
using Codeable.Foundation.Web.Core.MVC.Attributes;
using Stencil.Domain;
using Stencil.Primary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Stencil.Plugins.SystemMonitor.Controllers
{
    public class PingController : CoreController
    {
        public PingController(IFoundation foundation)
            : base(foundation)
        {
        }

        [NoClientCache(), OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Index()
        {
            return base.ExecuteFunction("Index", delegate ()
            {
                StencilAPI API = this.IFoundation.Resolve<StencilAPI>();

                string result = string.Empty;
                try
                {
                    var directResult = API.Direct.GlobalSettings.GetByName("anything");
                }
                catch (Exception)
                {
                    result += "DIRECT-FAIL ";
                }
                try
                {
                    var cacheResult = API.Index.Accounts.GetById(Guid.Empty);
                }
                catch (Exception)
                {
                    result += "INDEX-FAIL ";
                }
                if (string.IsNullOrEmpty(result))
                {
                    result = "PONG";
                }
                ViewBag.Result = result;
                return View();
            });
        }
    }
}
