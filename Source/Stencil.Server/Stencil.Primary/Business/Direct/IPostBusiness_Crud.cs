using System;
using System.Collections.Generic;
using System.Text;
using Stencil.Domain;

namespace Stencil.Primary.Business.Direct
{
    // WARNING: THIS FILE IS GENERATED
    public partial interface IPostBusiness
    {
        Post GetById(Guid post_id);
        List<Post> Find(int skip, int take, string keyword = "", string order_by = "", bool descending = false);
        
        List<Post> GetByAccount(Guid account_id);
        Post Insert(Post insertPost);
        Post Update(Post updatePost);
        
        void Delete(Guid post_id);
        void SynchronizationUpdate(Guid post_id, bool success, DateTime sync_date_utc, string sync_log);
        List<Guid?> SynchronizationGetInvalid(int retryPriorityThreshold, string sync_agent);
        void SynchronizationHydrateUpdate(Guid post_id, bool success, DateTime sync_date_utc, string sync_log);
        List<Guid?> SynchronizationHydrateGetInvalid(int retryPriorityThreshold, string sync_agent);
        void Invalidate(Guid post_id, string reason);
        
    }
}

