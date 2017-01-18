#if !WEB
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Stencil.SDK.Exceptions
{
    public class EndpointTimeoutException : EndpointException
    {
        public EndpointTimeoutException() : base() { }
        public EndpointTimeoutException(HttpStatusCode statusCode) : base(statusCode) { }
        public EndpointTimeoutException(HttpStatusCode statusCode, string message) : base(statusCode, message) { }
        public EndpointTimeoutException(HttpStatusCode statusCode, string message, Exception inner) : base(statusCode, message, inner) { }

    }
}
#endif