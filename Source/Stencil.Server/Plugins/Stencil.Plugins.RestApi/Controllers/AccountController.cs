using Stencil.Domain;
using Stencil.Primary.Mapping;
using Stencil.SDK;
using Stencil.SDK.Models.Requests;
using Stencil.SDK.Models.Responses;
using Stencil.Web.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Stencil.Plugins.RestAPI.Controllers
{
    public partial class AccountController
    {
        [HttpGet]
        [Route("self")]
        public object Self()
        {
            return base.ExecuteFunction("Self", delegate ()
            {
                Account currentAccount = this.GetCurrentAccount();

                Account account = null;

                AccountInfo data = account.ToInfoModel();


                ItemResult<AccountInfo> result = new ItemResult<AccountInfo>()
                {
                    success = true,
                    item = data
                };

                return Http200(result);
            });
        }

        [HttpPost]
        [Route("self/push_register")]
        public async Task<object> RegisterPushTokenAsync(PushTokenInput request)
        {
            return await base.ExecuteFunctionAsync<object>("RegisterPushTokenAsync", async delegate ()
            {
                Account currentAccount = this.GetCurrentAccount();

                switch (request.platform)
                {
                    case "ios":
                        await this.API.Integration.Push.RegisterApple(currentAccount.account_id, request.token);
                        break;
                    case "android":
                        await this.API.Integration.Push.RegisterGoogle(currentAccount.account_id, request.token);
                        break;
                    default:
                        return base.Http404("platform");
                }

                return base.Http200(new ActionResult() { success = true });
            });
        }
    }
}