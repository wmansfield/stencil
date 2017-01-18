using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil
{
    public static class Client
    {
        public static Stencil.SDK.StencilSDK Create(string baseUrl)
        {
            return new Stencil.SDK.StencilSDK(baseUrl);
        }
    }
}
