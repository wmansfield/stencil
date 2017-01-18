using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stencil.Domain;

namespace Stencil.Primary.Business.Integration
{
    public interface IDependencyCoordinator
    {
        void GlobalSettingInvalidated(Dependency affectedDependencies, Guid global_setting_id);
        void AccountInvalidated(Dependency affectedDependencies, Guid account_id);
        void AssetInvalidated(Dependency affectedDependencies, Guid asset_id);
        
    }
}


