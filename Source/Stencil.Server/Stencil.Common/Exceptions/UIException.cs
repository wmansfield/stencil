using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Common.Exceptions
{
    [Serializable]
    public class UIException : Exception
    {
        public UIException() { }
        public UIException(string message) : base(message) { }
        public UIException(string message, Exception inner) : base(message, inner) { }
        protected UIException(
          global::System.Runtime.Serialization.SerializationInfo info,
          global::System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
