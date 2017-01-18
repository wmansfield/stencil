#if !WEB
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Stencil.SDK.Exceptions
{

#if !WINDOWS_PHONE_APP
    [Serializable]
#endif
    public class EndpointException : Exception
    {
        public EndpointException() { }
        public EndpointException(HttpStatusCode statusCode)
            : base(string.Format("HttpStatusCode: {0}", statusCode))
        {
            this.StatusCode = statusCode;
        }
        public EndpointException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            this.StatusCode = statusCode;
        }
        public EndpointException(HttpStatusCode statusCode, string message, Exception inner)
            : base(message, inner)
        {
            this.StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; set; }
#if !WINDOWS_PHONE_APP
        protected EndpointException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
#endif
    }
}
#endif