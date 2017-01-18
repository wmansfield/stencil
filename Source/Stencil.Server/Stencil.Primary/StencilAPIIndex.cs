using Codeable.Foundation.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stencil.Primary.Business.Index;

namespace Stencil.Primary
{
    public class StencilAPIIndex : BaseClass
    {
        public StencilAPIIndex(IFoundation ifoundation)
            : base(ifoundation)
        {
        }

        public IAccountIndex Accounts
        {
            get { return this.IFoundation.Resolve<IAccountIndex>(); }
        }
        
    }
}


