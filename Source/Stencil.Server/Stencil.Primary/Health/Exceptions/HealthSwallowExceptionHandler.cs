using Codeable.Foundation.Common.System;
using Codeable.Foundation.Core.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Health.Exceptions
{
    public class HealthSwallowExceptionHandler : SwallowExceptionHandler
    {
        public HealthSwallowExceptionHandler(ILogger iLogger)
            : base(iLogger)
        {

        }

        protected override void LogException(Exception ex)
        {
            HealthReporter.LogException(ex);
            base.LogException(ex);
        }
    }
}
