using System;
using Foundation;

namespace Stencil.Native.Core
{
    public partial class BaseClass
    {
        public Action CreateAction(string name, Action method, Action<Exception> onError = null)
        {
            return delegate()
            {
                this.ExecuteMethod(name, method, null);
            };
        }
    }
}

