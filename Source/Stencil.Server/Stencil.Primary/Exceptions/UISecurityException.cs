using Stencil.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Exceptions
{
    [Serializable]
    public class UISecurityException : UIException
    {
        public UISecurityException() { }
        public UISecurityException(string message) : base(message) { }
        public UISecurityException(string message, Exception inner) : base(message, inner) { }
        protected UISecurityException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }
}
