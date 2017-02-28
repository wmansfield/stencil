using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stencil.SDK.Models;

namespace Stencil.Primary.Synchronization.Implementation
{
    public partial class PostSynchronizer
    {
        partial void HydrateSDKModel(Domain.Post domainModel, SDK.Models.Post sdkModel)
        {
            Account account = this.API.Index.Accounts.GetById(domainModel.account_id);
            if(account != null)
            {
                sdkModel.account_name = account.first_name;
            }
        }
    }
}
