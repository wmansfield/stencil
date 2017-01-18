using Codeable.Foundation.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary
{
    public class StencilAPI : BaseClass
    {
        public StencilAPI(IFoundation iFoundation)
            : base(iFoundation)
        {
            this.Direct = new StencilAPIDirect(iFoundation);
            Index = new StencilAPIIndex(iFoundation);
            this.Integration = new StencilAPIIntegration(iFoundation);
        }
        public StencilAPIDirect Direct { get; set; }
        public StencilAPIIndex Index { get; set; }
        public StencilAPIIntegration Integration { get; set; }

    }
}
