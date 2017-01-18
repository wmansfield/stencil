using Stencil.SDK.Endpoints;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stencil.SDK
{
    public partial class StencilSDK
    {
        // members for web ease
        public GlobalSettingEndpoint GlobalSetting;
        public AccountEndpoint Account;
        public AssetEndpoint Asset;
        

        protected virtual void ConstructCoreEndpoints()
        {
            this.GlobalSetting = new GlobalSettingEndpoint(this);
            this.Account = new AccountEndpoint(this);
            this.Asset = new AssetEndpoint(this);
            
        }   
    }
}

