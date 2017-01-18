using Codeable.Foundation.Common;
using Codeable.Foundation.Common.Aspect;
using Codeable.Foundation.Core.Caching;
using Codeable.Foundation.Core.Unity;
using Stencil.Domain;
using Stencil.Primary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Web.Security
{
    public class StencilHashedTimeSignatureAuthorizer : ChokeableClass
    {
        public StencilHashedTimeSignatureAuthorizer(IFoundation iFoundation)
            : base(iFoundation)
        {
            this.TimedCache = new AspectCache("StencilHashedTimeSignatureAuthorizer.TimedCache", iFoundation, new ExpireStaticLifetimeManager("StencilHashedTimeSignatureAuthorizer.TimedCache.Static", TimeSpan.FromMinutes(5), false));
            this.API = iFoundation.Resolve<StencilAPI>();
        }

        protected virtual StencilAPI API { get; set; }


        public virtual AspectCache TimedCache { get; set; }

        public virtual Account Authorize(string api_key, string signature)
        {
            return base.ExecuteFunction("Authorize", delegate ()
            {
                Account result = null;
                if (!string.IsNullOrEmpty(api_key))
                {
                    try
                    {
                        bool fromCache = true;
                        Account account = TimedCache.PerLifetime(api_key, delegate ()
                        {
                            fromCache = false;
                            return this.API.Direct.Accounts.GetByApiKey(api_key);
                        });

                        if (fromCache && (account != null) && (account.disabled))
                        {
                            // try to get non-cached for enabled toggle
                            account = this.API.Direct.Accounts.GetByApiKey(api_key);
                        }

                        if ((account != null) && !account.disabled)
                        {
                            if (new HashedTimeSignatureVerifier().ValidateSignature(account.api_key, account.api_secret, signature))
                            {
                                result = account;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        this.IFoundation.LogError(ex, "Authorize");
                    }
                }
                return result;

            });
        }
    }
}
