using System;
using System.Collections.Generic;
using System.Text;
using Stencil.Domain;

namespace Stencil.Primary.Business.Direct
{
    // WARNING: THIS FILE IS GENERATED
    public partial interface IAccountBusiness
    {
        Account GetById(Guid account_id);
        List<Account> Find(int skip, int take, string keyword = "", string order_by = "", bool descending = false);
        Account Insert(Account insertAccount);
        Account Update(Account updateAccount);
        
        void Delete(Guid account_id);
        void SynchronizationUpdate(Guid account_id, bool success, DateTime sync_date_utc, string sync_log);
        List<Guid?> SynchronizationGetInvalid(int retryPriorityThreshold, string sync_agent);
        void SynchronizationHydrateUpdate(Guid account_id, bool success, DateTime sync_date_utc, string sync_log);
        List<Guid?> SynchronizationHydrateGetInvalid(int retryPriorityThreshold, string sync_agent);
        void Invalidate(Guid account_id, string reason);
        
    }
}

