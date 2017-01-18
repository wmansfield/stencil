using Codeable.Foundation.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stencil.Primary.Business.Direct;

namespace Stencil.Primary
{
    public class StencilAPIDirect : BaseClass
    {
        public StencilAPIDirect(IFoundation ifoundation)
            : base(ifoundation)
        {
        }
        public IGlobalSettingBusiness GlobalSettings
        {
            get { return this.IFoundation.Resolve<IGlobalSettingBusiness>(); }
        }
        public IAccountBusiness Accounts
        {
            get { return this.IFoundation.Resolve<IAccountBusiness>(); }
        }
        public IAssetBusiness Assets
        {
            get { return this.IFoundation.Resolve<IAssetBusiness>(); }
        }
        
    }
}


