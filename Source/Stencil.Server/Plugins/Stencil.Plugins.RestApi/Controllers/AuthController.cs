using Codeable.Foundation.Common;
using Stencil.Common;
using Stencil.Domain;
using Stencil.SDK;
using sdk = Stencil.SDK.Models;
using dm = Stencil.Domain;
using Stencil.SDK.Models.Requests;
using Stencil.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Security;
using Stencil.Primary;
using System.Threading.Tasks;
using Stencil.Primary.Mapping;

namespace Stencil.Plugins.RestAPI.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("api/auth")]
    public partial class AuthController : RestApiBaseController
    {
        public AuthController(IFoundation foundation)
            : base(foundation)
        {
        }

        [HttpPost]
        [Route("login")]
        public object Login(AuthLoginInput input)
        {
            return base.ExecuteFunction<object>("Login", delegate ()
            {
                Account account = this.API.Direct.Accounts.GetForValidPassword(input.user, input.password);
                if (account == null)
                {
                    return Http400("Invalid password/user combination");
                }


                sdk.Responses.AccountInfo data = account.ToInfoModel();

                FormsAuthentication.SetAuthCookie(account.account_id.ToString(), input.persist);

                ItemResult<sdk.Responses.AccountInfo> result = new ItemResult<sdk.Responses.AccountInfo>()
                {
                    item = data,
                    success = true
                };
                return base.Http200(result);
            });
        }

        [HttpPost]
        [Route("register")]
        public object Register(RegisterInput input)
        {
            return base.ExecuteFunction<object>("Register", delegate ()
            {
                // create/retrieve account
                Account account = this.API.Direct.Accounts.GetByEmail(input.email);
                if (account == null)
                {
                    account = new Account()
                    {
                        account_id = Guid.NewGuid(),
                        disabled = false,
                        first_name = input.first_name,
                        last_name = input.last_name,
                        email = input.email,
                        password = input.password,
                    };
                    account = this.API.Direct.Accounts.Insert(account);
                }
                if (account == null)
                {
                    return Http500("Unable to create accounts at this time. Please try again soon.");
                }

                // Standard Login
                sdk.Responses.AccountInfo data = account.ToInfoModel();

                FormsAuthentication.SetAuthCookie(account.account_id.ToString(), false);
                ItemResult<sdk.Responses.AccountInfo> result = new ItemResult<sdk.Responses.AccountInfo>()
                {
                    item = data,
                    success = true
                };
                return base.Http200(result);
            });
        }

        [HttpPost]
        [Route("logout")]
        public object Logout()
        {
            return base.ExecuteFunction<object>("Logout", delegate ()
            {
                FormsAuthentication.SignOut();
                return base.Http200(new ActionResult() { success = true });
            });
        }


        [HttpPost]
        [Route("password_reset/start")]
        public object PasswordResetStart(PasswordResetInput input)
        {
            return base.ExecuteFunction<object>("PasswordResetStart", delegate ()
            {
                string test = this.Request.Content.ReadAsAsync<string>().Result;
                Account account = this.API.Direct.Accounts.GetByEmail(input.email);
                if (account != null)
                {
                    this.API.Direct.Accounts.PasswordResetStart(account.account_id);
                }

                return base.Http200(new ActionResult() { success = true });
            });
        }

        [HttpPost]
        [Route("password_reset/complete")]
        public object PasswordResetComplete(PasswordResetInput input)
        {
            return base.ExecuteFunction<object>("PasswordResetComplete", delegate ()
            {
                bool success = false;
                Account account = this.API.Direct.Accounts.GetByEmail(input.email);
                if (account != null)
                {
                    success = this.API.Direct.Accounts.PasswordResetComplete(account.account_id, input.token, input.password);
                }

                return base.Http200(new ActionResult() { success = success });
            });
        }

    }
}
