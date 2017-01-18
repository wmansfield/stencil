using DuoCode.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stencil;

namespace System.Security.Cryptography
{
    public class MD5
    {
        public static MD5 Create()
        {
            return new MD5();
        }
        public string GenerateHash(string content)
        {
            dynamic sdkBridge = Js.referenceAs<dynamic>(WebAssumptions.JS_BRIDGE_NAME);
            return sdkBridge.generateMD5Hash(content);
        }
    }
}
