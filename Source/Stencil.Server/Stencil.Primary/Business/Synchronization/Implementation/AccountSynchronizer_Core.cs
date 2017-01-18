using Codeable.Foundation.Common;
using Stencil.Common;
using Stencil.Domain;
using Stencil.Primary.Business.Index;
using Stencil.Primary.Health;
using sdk = Stencil.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Codeable.Foundation.Core;

namespace Stencil.Primary.Synchronization.Implementation
{
    public partial class AccountSynchronizer : SynchronizerBase<Guid>, IAccountSynchronizer
    {
        public AccountSynchronizer(IFoundation foundation)
            : base(foundation, "AccountSynchronizer")
        {

        }

        public override int Priority
        {
            get
            {
                return 10;
            }
        }

        public override void PerformSynchronizationForItem(Guid primaryKey)
        {
            base.ExecuteMethod("PerformSynchronizationForItem", delegate ()
            {
                Account domainModel = this.API.Direct.Accounts.GetById(primaryKey);
                if (domainModel != null)
                {
                    Action<Guid, bool, DateTime, string> synchronizationUpdateMethod = this.API.Direct.Accounts.SynchronizationUpdate;
                    if(this.API.Integration.SettingsResolver.IsHydrate())
                    {
                        synchronizationUpdateMethod = this.API.Direct.Accounts.SynchronizationHydrateUpdate;
                    }
                    DateTime syncDate = DateTime.UtcNow;
                    if (domainModel.sync_invalid_utc.HasValue)
                    {
                        syncDate = domainModel.sync_invalid_utc.Value;
                    }
                    try
                    {
                        sdk.Account sdkModel = domainModel.ToSDKModel();
                        
                        this.HydrateSDKModelComputed(domainModel, sdkModel);
                        this.HydrateSDKModel(domainModel, sdkModel);

                        if (domainModel.deleted_utc.HasValue)
                        {
                            this.API.Index.Accounts.DeleteDocument(sdkModel);
                            synchronizationUpdateMethod(domainModel.account_id, true, syncDate, null);
                        }
                        else
                        {
                            IndexResult result = this.API.Index.Accounts.UpdateDocument(sdkModel);
                            if (result.success)
                            {
                                synchronizationUpdateMethod(domainModel.account_id, true, syncDate, result.ToString());
                            }
                            else
                            {
                                throw new Exception(result.ToString());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        this.IFoundation.LogError(ex, "PerformSynchronizationForItem");
                        HealthReporter.Current.UpdateMetric(HealthTrackType.Each, string.Format(HealthReporter.INDEXER_ERROR_SYNC, this.EntityName), 0, 1);
                        synchronizationUpdateMethod(primaryKey, false, syncDate, CoreUtility.FormatException(ex));
                    }
                }
            });
        }

        public override int PerformSynchronization(string requestedAgentName)
        {
            return base.ExecuteFunction("PerformSynchronization", delegate ()
            {
                string agentName = requestedAgentName;
                if(string.IsNullOrEmpty(agentName))
                {
                    agentName = this.AgentName;
                }
                List<Guid?> invalidItems = new List<Guid?>();
                if(this.API.Integration.SettingsResolver.IsHydrate())
                {
                    invalidItems = this.API.Direct.Accounts.SynchronizationHydrateGetInvalid(CommonAssumptions.INDEX_RETRY_THRESHOLD_SECONDS, agentName);
                }
                else
                {
                    invalidItems = this.API.Direct.Accounts.SynchronizationGetInvalid(CommonAssumptions.INDEX_RETRY_THRESHOLD_SECONDS, agentName);
                }
                foreach (Guid? item in invalidItems)
                {
                    this.PerformSynchronizationForItem(item.GetValueOrDefault());
                }
                return invalidItems.Count;
            });
        }
        
        /// <summary>
        /// Computed and Calculated Aggs, Typically Generated
        /// </summary>
        protected void HydrateSDKModelComputed(Account domainModel, sdk.Account sdkModel)
        {
            
        }
        partial void HydrateSDKModel(Account domainModel, sdk.Account sdkModel);
    }
}

