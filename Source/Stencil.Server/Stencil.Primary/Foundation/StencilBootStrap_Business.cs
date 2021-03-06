using Codeable.Foundation.Common;
using Codeable.Foundation.UI.Web.Core.Unity;
using Stencil.Primary.Business.Direct;
using Stencil.Primary.Business.Direct.Implementation;
using Stencil.Primary.Business.Index;
using Stencil.Primary.Business.Index.Implementation;
using Stencil.Primary.Synchronization;
using Stencil.Primary.Synchronization.Implementation;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Foundation
{
    public partial class StencilBootStrap
    {
        protected virtual void RegisterDataElements(IFoundation foundation)
        {
            foundation.Container.RegisterType<IGlobalSettingBusiness, GlobalSettingBusiness>(new HttpRequestLifetimeManager());
            foundation.Container.RegisterType<IAccountBusiness, AccountBusiness>(new HttpRequestLifetimeManager());
            foundation.Container.RegisterType<IAssetBusiness, AssetBusiness>(new HttpRequestLifetimeManager());
            
            
            //Indexes
            foundation.Container.RegisterType<IAccountIndex, AccountIndex>(new HttpRequestLifetimeManager());
            
            
            //Synchronizers
            foundation.Container.RegisterType<IAccountSynchronizer, AccountSynchronizer>(new HttpRequestLifetimeManager());
            
        }
    }
}

