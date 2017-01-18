using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Domain
{
    public static class _DomainExtensions
    {
        public static bool IsAdmin(this Account account)
        {
            return account.HasEntitlement(WellKnownEntitlements.super_admin.ToString());
        }
        public static bool HasEntitlement(this Account account, string entitlement)
        {
            if (account != null && !string.IsNullOrWhiteSpace(account.entitlements) && !string.IsNullOrWhiteSpace(entitlement))
            {
                return account.entitlements.Split(',').Contains(entitlement);
            }
            return false;
        }
    }
}
