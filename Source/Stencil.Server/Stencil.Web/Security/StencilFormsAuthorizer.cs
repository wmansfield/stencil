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
     public class StencilFormsAuthorizer : ChokeableClass
    {
        public StencilFormsAuthorizer(IFoundation iFoundation)
            : base(iFoundation)
        {
            this.TimedCache = new AspectCache("StencilFormsAuthorizer.TimedCache", iFoundation, new ExpireStaticLifetimeManager("StencilFormsAuthorizer.TimedCache.Static", TimeSpan.FromMinutes(5), false));
            this.API = iFoundation.Resolve<StencilAPI>();
        }

        protected virtual StencilAPI API { get; set; }


        public virtual AspectCache TimedCache { get; set; }

        public virtual Account Authorize(string account_id)
        {
            return base.ExecuteFunction("Authorize", delegate ()
            {
                Account result = null;
                Guid accountID = Guid.Empty;
                if (Guid.TryParse(account_id, out accountID) && accountID != Guid.Empty)
                {
                    try
                    {
                        bool fromCache = true;
                        Account account = TimedCache.PerLifetime(account_id, delegate ()
                        {
                            fromCache = false;
                            return this.API.Direct.Accounts.GetById(accountID);
                        });

                        if (fromCache && (account != null) && account.disabled)
                        {
                            // try to get non-cached for enabled toggle
                            account = this.API.Direct.Accounts.GetById(accountID);
                        }

                        if (account != null && !account.disabled)
                        {
                            result = account;
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
