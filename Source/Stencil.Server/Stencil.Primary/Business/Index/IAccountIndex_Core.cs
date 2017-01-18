using Codeable.Foundation.Common;
using Stencil.SDK.Models;
using Stencil.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Business.Index
{
    public partial interface IAccountIndex : IIndexer<Account>
    {
        Account GetById(Guid id);
        TCustomModel GetById<TCustomModel>(Guid id)
            where TCustomModel : class;
        
        ListResult<Account> Find(int skip, int take, string keyword = "", string order_by = "", bool descending = false);
       
    }
}
