using Codeable.Foundation.Common;
using Codeable.Foundation.Common.System;
using Codeable.Foundation.Core;
using Codeable.Foundation.UI.Web.Core.MVC;
using Stencil.Primary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Web.Controllers
{
    public abstract class MvcBaseController : CoreController
    {
        #region Constructors

        public MvcBaseController()
            : base()
        {
            this.API = CoreFoundation.Current.Resolve<StencilAPI>();
        }
        public MvcBaseController(IFoundation iFoundation)
            : base(iFoundation)
        {
            this.API = iFoundation.Resolve<StencilAPI>();
        }
        public MvcBaseController(IFoundation iFoundation, IHandleExceptionProvider iHandleExceptionProvider)
            : base(iFoundation, iHandleExceptionProvider)
        {
            this.API = iFoundation.Resolve<StencilAPI>();
        }

        #endregion

        public virtual StencilAPI API { get; set; }
    }
}
