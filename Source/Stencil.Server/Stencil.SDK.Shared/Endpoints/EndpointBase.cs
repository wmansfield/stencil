using System;
using System.Collections.Generic;
using System.Text;

namespace Stencil.SDK.Endpoints
{
    public abstract class EndpointBase
    {
        public EndpointBase(StencilSDK sdk)
        {
            this.Sdk = sdk;
        }

        protected virtual StencilSDK Sdk { get; set; }

    }
}
