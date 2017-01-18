using DuoCode.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.SDK.Security
{
    public partial class HashedTimeSignatureGenerator
    {
        protected virtual long GetUnixTime()
        {
            dynamic sdkBridge = Js.referenceAs<dynamic>(WebAssumptions.JS_BRIDGE_NAME);
            return sdkBridge.getTimeStamp();
        }
    }
}