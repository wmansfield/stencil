using Stencil.SDK.Endpoints;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stencil.SDK
{
    public partial class StencilSDK
    {
        //don't use properties for js mapping ease
        public AuthEndpoint Auth;
        public AccountsEndpoint Accounts;
        public ServerEndpoint Server;

        protected virtual void ConstructManualEndpoints()
        {
            this.Auth = new AuthEndpoint(this);
            this.Accounts = new AccountsEndpoint(this);
            this.Server = new ServerEndpoint(this);
        }
    }
}
