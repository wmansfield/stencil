using Codeable.Foundation.Common;
using Codeable.Foundation.Common.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Exceptions
{
    public class FriendlyExceptionHandlerProvider : IHandleExceptionProvider
    {
        public FriendlyExceptionHandlerProvider(IFoundation foundation, ILogger iLogger)
        {
            this.ILogger = iLogger;
            this.Foundation = foundation;
        }
        protected virtual ILogger ILogger { get; set; }
        protected virtual IFoundation Foundation { get; set; }

        #region IHandleExceptionProvider Members

        public virtual string PolicyName { get; set; }

        public virtual IHandleException CreateHandler()
        {
            return new FriendlyExceptionHandler(this.Foundation, this.ILogger, this.PolicyName);
        }

        #endregion
    }
}
