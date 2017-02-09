using System;
using System.Collections.Generic;
using System.Text;
using Stencil.Domain;

namespace Stencil.Primary.Business.Direct
{
    // WARNING: THIS FILE IS GENERATED
    public partial interface IRemarkBusiness
    {
        Remark GetById(Guid remark_id);
        List<Remark> Find(int skip, int take, string keyword = "", string order_by = "", bool descending = false);
        
        List<Remark> GetByPost(Guid post_id);
        
        List<Remark> GetByAccount(Guid account_id);
        Remark Insert(Remark insertRemark);
        Remark Update(Remark updateRemark);
        
        void Delete(Guid remark_id);
        void SynchronizationUpdate(Guid remark_id, bool success, DateTime sync_date_utc, string sync_log);
        List<Guid?> SynchronizationGetInvalid(int retryPriorityThreshold, string sync_agent);
        void SynchronizationHydrateUpdate(Guid remark_id, bool success, DateTime sync_date_utc, string sync_log);
        List<Guid?> SynchronizationHydrateGetInvalid(int retryPriorityThreshold, string sync_agent);
        void Invalidate(Guid remark_id, string reason);
        
    }
}

