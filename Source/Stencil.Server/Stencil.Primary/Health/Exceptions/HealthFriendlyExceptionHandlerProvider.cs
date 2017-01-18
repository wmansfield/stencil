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
    public class HealthFriendlyExceptionHandlerProvider : FriendlyExceptionHandlerProvider
    {
        public HealthFriendlyExceptionHandlerProvider(IFoundation foundation, ILogger iLogger)
            : base(foundation, iLogger)
        {
        }

        #region IHandleExceptionProvider Members

        public override IHandleException CreateHandler()
        {
            return new HealthFriendlyExceptionHandler(this.Foundation, this.ILogger, this.PolicyName);
        }

        #endregion
    }
}
