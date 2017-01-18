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
    public class BootstrapController : CoreController
    {
        public BootstrapController(IFoundation foundation)
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
                    Guid newAccountID = new Guid("{0AB9F424-DF92-4678-8AB7-68F186E1C497}");// hard code for super safety (fkey)
                    Account newAccount = new Account()
                    {
                        account_id = newAccountID,
                        first_name = "William",
                        last_name = "Mansfield",
                        email = "wmansfield@socialhaven.com",
                        password = "stencildemo",
                        entitlements = "super_admin",
                    };
                    Account initialAccount = API.Direct.Accounts.CreateInitialAccount(newAccount);
                    if(initialAccount != null)
                    {
                        result = "Created new user";
                    }
                    else
                    {
                        result = "Pong";
                    }
                }
                catch (Exception ex)
                {
                    result = ex.Message;
                }
               
                ViewBag.Result = result;
                return View();
            });
        }
    }
}
