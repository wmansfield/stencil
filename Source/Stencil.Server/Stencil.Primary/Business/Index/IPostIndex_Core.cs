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
    public partial interface IPostIndex : IIndexer<Post>
    {
        Post GetById(Guid id);
        TCustomModel GetById<TCustomModel>(Guid id)
            where TCustomModel : class;
        Post GetById(Guid id, Guid? for_account_id);
        ListResult<Post> GetByAccount(Guid account_id, int skip, int take, string order_by = "", bool descending = false, Guid? for_account_id = null);
        
        ListResult<Post> Find(Guid? for_account_id, int skip, int take, string keyword = "", string order_by = "", bool descending = false, Guid? account_id = null);
       
    }
}
