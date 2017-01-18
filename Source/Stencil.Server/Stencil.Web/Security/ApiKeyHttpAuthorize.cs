using Codeable.Foundation.Common;
using Codeable.Foundation.Core;
using Stencil.Domain;
using Stencil.Primary.Workers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Stencil.Web.Security
{
    public class ApiKeyHttpAuthorize : AuthorizeAttribute
    {
        public const string API_PARAM_KEY = "api_key";
        public const string API_PARAM_SIG = "api_signature";
        public const string PARAM_PLATFORM = "X-DevicePlatform";
        public const string PARAM_VERSION = "X-DeviceVersion";
        public const string FACTION_ID = "X-Faction";

        public const string CURRENT_ACCOUNT_HTTP_CONTEXT_KEY = "__current_account";

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            return AuthorizedRequest(actionContext);
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            actionContext.Response = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized)
            {
                ReasonPhrase = "Unauthorized",
                Content = new StringContent("Access denied")
            };
        }

        public bool AuthorizedRequest(HttpActionContext actionContext)
        {
            IFoundation iFoundation = CoreFoundation.Current; //weak usage of CoreFoundation.Current
            Account account = null;
            bool isPreAuthorized = base.IsAuthorized(actionContext);

            // already verified [same request?]
            if (actionContext.Request.Properties.ContainsKey(CURRENT_ACCOUNT_HTTP_CONTEXT_KEY))
            {
                return true;
            }


            if (isPreAuthorized)
            {
                StencilFormsAuthorizer authorizer = iFoundation.Resolve<StencilFormsAuthorizer>();
                account = authorizer.Authorize(actionContext.RequestContext.Principal.Identity.Name);
            }

            if (account == null)
            {
                NameValueCollection query = HttpUtility.ParseQueryString(actionContext.Request.RequestUri.ToString());

                // from query string
                string key = query[API_PARAM_KEY];
                string signature = query[API_PARAM_SIG];

                // from headers
                if (actionContext.Request.Headers.Contains(API_PARAM_KEY))
                {
                    string value = actionContext.Request.Headers.GetValues(API_PARAM_KEY).FirstOrDefault();
                    if (!string.IsNullOrEmpty(value))
                    {
                        key = value;
                    }
                }
                if (actionContext.Request.Headers.Contains(API_PARAM_SIG))
                {
                    string value = actionContext.Request.Headers.GetValues(API_PARAM_SIG).FirstOrDefault();
                    if (!string.IsNullOrEmpty(value))
                    {
                        signature = value;
                    }
                }
                StencilHashedTimeSignatureAuthorizer authorizer = iFoundation.Resolve<StencilHashedTimeSignatureAuthorizer>();
                account = authorizer.Authorize(key, signature);
            }

            if (account != null)
            {
                actionContext.Request.Properties[CURRENT_ACCOUNT_HTTP_CONTEXT_KEY] = account;
                try
                {
                    ApiIdentity apiIdentity = new ApiIdentity(account.account_id, string.Format("{0} {1}", account.first_name, account.last_name));
                    var context = HttpContext.Current;
                    if (context != null)
                    {
                        context.User = new GenericPrincipal(apiIdentity, new string[0]);
                    }
                }
                catch (Exception ex)
                {
                    iFoundation.LogError(ex, "HttpContext.Current.User");
                }
                string platform = string.Empty;
                try
                {
                    if (actionContext.Request.Headers.Contains(PARAM_PLATFORM))
                    {
                        string value = actionContext.Request.Headers.GetValues(PARAM_PLATFORM).FirstOrDefault();
                        if (!string.IsNullOrEmpty(value))
                        {
                            platform += value;
                        }
                    }
                    if (actionContext.Request.Headers.Contains(PARAM_VERSION))
                    {
                        string value = actionContext.Request.Headers.GetValues(PARAM_VERSION).FirstOrDefault();
                        if (!string.IsNullOrEmpty(value))
                        {
                            platform += " - v" + value;
                        }
                    }
                }
                catch (Exception ex)
                {
                    iFoundation.LogError(ex, "HttpContext.Current.User");
                }

                AccountLoggedInWorker.EnqueueRequest(iFoundation, new LoggedInRequest()
                {
                    account_id = account.account_id,
                    platform = platform,
                    login_utc = DateTime.UtcNow
                });
                return true;
            }

            return false;
        }

    }
}
