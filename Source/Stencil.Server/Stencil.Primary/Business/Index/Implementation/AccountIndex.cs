using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Business.Index.Implementation
{
    public partial class AccountIndex
    {
        public void UpdateLastLogin(Guid account_id, DateTime? last_login_utc, string platform)
        {
            base.ExecuteMethod("UpdateLastLogin", delegate ()
            {
                this.UpdateDocumentPartial(account_id.ToString(), new
                {
                    last_login_utc = last_login_utc,
                    last_login_platform = platform
                });
            });
        }
    }
}
