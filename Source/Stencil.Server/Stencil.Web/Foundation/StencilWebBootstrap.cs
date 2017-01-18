using Codeable.Foundation.UI.Web.Core.Foundation;
using Stencil.Primary.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Web.Foundation
{
    public class StencilWebBootstrap : MVCWebBootStrap
    {
        public StencilWebBootstrap()
        {
            this.BootStrapChain.Add(new StencilBootStrap());
        }
    }
}
