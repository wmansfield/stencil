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
    public partial interface IRemarkIndex : IIndexer<Remark>
    {
        Remark GetById(Guid id);
        TCustomModel GetById<TCustomModel>(Guid id)
            where TCustomModel : class;
        ListResult<Remark> GetByPost(Guid post_id, int skip, int take, string order_by = "", bool descending = false);
        ListResult<Remark> GetByAccount(Guid account_id, int skip, int take, string order_by = "", bool descending = false);
        
        ListResult<Remark> Find(int skip, int take, string keyword = "", string order_by = "", bool descending = false, Guid? post_id = null, Guid? account_id = null);
       int GetCountRemark(Guid post_id);
        
    }
}
