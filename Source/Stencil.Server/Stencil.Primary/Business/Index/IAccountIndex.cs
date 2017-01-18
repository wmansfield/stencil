using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Business.Index
{
    public partial interface IAccountIndex
    {
        void UpdateLastLogin(Guid account_id, DateTime? last_login_utc, string platform);
    }
}
