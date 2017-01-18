using Codeable.Foundation.Common;
using Codeable.Foundation.Common.System;
using Codeable.Foundation.Core.System;
using Stencil.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Stencil.Primary.Exceptions
{
    public class FriendlyExceptionHandler : StandardThrowExceptionHandler
    {
        public FriendlyExceptionHandler(IFoundation iFoundation, ILogger iLogger)
            : this(iFoundation, iLogger, string.Empty)
        {

        }
        public FriendlyExceptionHandler(IFoundation iFoundation, ILogger iLogger, string policyName)
            : base(iLogger)
        {
            this.PolicyName = policyName;
            this.Foundation = iFoundation;
        }

        public IFoundation Foundation { get; set; }

        #region IHandleException Members

        public override bool HandleException(Exception ex, out bool rethrowCurrent, out Exception replacedException)
        {
            this.LogException(ex);

            replacedException = null;
            rethrowCurrent = true;

            if (ex is ThreadAbortException)
            {
                rethrowCurrent = false;// don't rethrow, aborting anyway
                return true;
            }
            if ((ex is HttpResponseException)
                || (ex is ServerException)
                || (ex is UIException))
            {
                return true; // pass through
            }

            if (ex is DbException)
            {
                replacedException = new ServerException("A server data error has occurred.");
                return true;
            }
            if (ex is ArgumentNullException)
            {
                replacedException = new ServerException("A server reference error was detected.");
                return true;
            }
            if (ex is NullReferenceException)
            {
                replacedException = new ServerException("A server reference error has occurred.");
                return true;
            }

            replacedException = new ServerException("A server error has occurred.");
            return true;
        }

        #endregion
    }
}
