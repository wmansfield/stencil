using Codeable.Foundation.Common;
using Codeable.Foundation.Common.System;
using Codeable.Foundation.Core.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Health.Exceptions
{
    public class HealthSwallowExceptionHandlerProvider : SwallowExceptionHandlerProvider
    {
        public HealthSwallowExceptionHandlerProvider(IFoundation foundation, ILogger iLogger)
            : base(iLogger)
        {
        }

        public override IHandleException CreateHandler()
        {
            return new HealthSwallowExceptionHandler(this.ILogger);
        }
    }
}
