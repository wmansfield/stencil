using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stencil.Domain;
using Codeable.Foundation.Common.Aspect;
using Codeable.Foundation.Common;

namespace Stencil.Primary.Business.Integration
{
    public partial class DependencyCoordinator_Core : ChokeableClass, IDependencyCoordinator
    {
        public DependencyCoordinator_Core(IFoundation iFoundation)
            : base(iFoundation)
        {
            this.API = new StencilAPI(iFoundation);
        }
        public virtual StencilAPI API { get; set; }
        
        public virtual void GlobalSettingInvalidated(Dependency affectedDependencies, Guid global_setting_id)
        {
            base.ExecuteMethod("GlobalSettingInvalidated", delegate ()
            {
                DependencyWorker<GlobalSetting>.EnqueueRequest(this.IFoundation, affectedDependencies, global_setting_id, this.ProcessGlobalSettingInvalidation);
            });
        }
        protected virtual void ProcessGlobalSettingInvalidation(Dependency dependencies, Guid global_setting_id)
        {
            base.ExecuteMethod("ProcessGlobalSettingInvalidation", delegate ()
            {
                
            });
        }
        public virtual void AccountInvalidated(Dependency affectedDependencies, Guid account_id)
        {
            base.ExecuteMethod("AccountInvalidated", delegate ()
            {
                DependencyWorker<Account>.EnqueueRequest(this.IFoundation, affectedDependencies, account_id, this.ProcessAccountInvalidation);
            });
        }
        protected virtual void ProcessAccountInvalidation(Dependency dependencies, Guid account_id)
        {
            base.ExecuteMethod("ProcessAccountInvalidation", delegate ()
            {
                
            });
        }
        public virtual void AssetInvalidated(Dependency affectedDependencies, Guid asset_id)
        {
            base.ExecuteMethod("AssetInvalidated", delegate ()
            {
                DependencyWorker<Asset>.EnqueueRequest(this.IFoundation, affectedDependencies, asset_id, this.ProcessAssetInvalidation);
            });
        }
        protected virtual void ProcessAssetInvalidation(Dependency dependencies, Guid asset_id)
        {
            base.ExecuteMethod("ProcessAssetInvalidation", delegate ()
            {
                
            });
        }
        
    }
}


