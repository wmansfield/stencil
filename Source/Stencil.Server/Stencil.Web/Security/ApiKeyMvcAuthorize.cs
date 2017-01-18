using Codeable.Foundation.Common;
using Codeable.Foundation.Core;
using Stencil.Domain;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Stencil.Web.Security
{
    public class ApiKeyMvcAuthorize : AuthorizeAttribute
    {
        public const string API_PARAM_KEY = "api_key";
        public const string API_PARAM_SIG = "api_signature";
        public const string CURRENT_ACCOUNT_HTTP_CONTEXT_KEY = "__current_account";

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            IFoundation iFoundation = CoreFoundation.Current;
            Account account = null;
            bool isPreAuthorized = base.AuthorizeCore(httpContext);

            // already verified
            if (httpContext.Items.Contains(CURRENT_ACCOUNT_HTTP_CONTEXT_KEY))
            {
                return true;
            }

            if (isPreAuthorized)
            {
                StencilFormsAuthorizer authorizer = iFoundation.Resolve<StencilFormsAuthorizer>();
                account = authorizer.Authorize(httpContext.User.Identity.Name);
            }
            if (account == null)
            {
                // try with headers or QS
                NameValueCollection query = httpContext.Request.QueryString;
                string key = query[API_PARAM_KEY];
                string signature = query[API_PARAM_SIG];

                // from headers
                string value = httpContext.Request.Headers[API_PARAM_KEY];
                if (!string.IsNullOrEmpty(value))
                {
                    key = value;
                }
                value = httpContext.Request.Headers[API_PARAM_SIG];
                if (!string.IsNullOrEmpty(value))
                {
                    signature = value;
                }
                StencilHashedTimeSignatureAuthorizer authorizer = iFoundation.Resolve<StencilHashedTimeSignatureAuthorizer>();
                account = authorizer.Authorize(key, signature);
            }

            if (account != null)
            {
                httpContext.Items[CURRENT_ACCOUNT_HTTP_CONTEXT_KEY] = account;
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
                    iFoundation.LogError(ex, "HttpContext.Current.Account");
                }
                return true;
            }

            return false;
        }

    }
}
