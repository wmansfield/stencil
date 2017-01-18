using Codeable.Foundation.Common;
using Codeable.Foundation.Common.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stencil.Primary.Exceptions;

namespace Stencil.Primary.Health.Exceptions
{
    public class HealthFriendlyExceptionHandler : FriendlyExceptionHandler
    {
        public HealthFriendlyExceptionHandler(IFoundation iFoundation, ILogger iLogger)
            : this(iFoundation, iLogger, string.Empty)
        {

        }
        public HealthFriendlyExceptionHandler(IFoundation iFoundation, ILogger iLogger, string policyName)
            : base(iFoundation, iLogger, policyName)
        {
        }
        protected override void LogException(Exception ex)
        {
            HealthReporter.LogException(ex);
            base.LogException(ex);
        }
    }
}
