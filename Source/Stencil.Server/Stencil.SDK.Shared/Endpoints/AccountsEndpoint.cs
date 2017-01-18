#if WINDOWS_PHONE_APP
using RestSharp.Portable;
#else
using RestSharp;
#endif
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Stencil.SDK.Models;
using Stencil.SDK.Models.Requests;
using Stencil.SDK.Models.Responses;

namespace Stencil.SDK.Endpoints
{
    public class AccountsEndpoint : EndpointBase
    {
        public AccountsEndpoint(StencilSDK api)
            : base(api)
        {

        }

        public Task<ItemResult<AccountInfo>> GetSelfAsync()
        {
            var request = new RestRequest(Method.GET);
            request.Resource = "accounts/self";
            return this.Sdk.ExecuteAsync<ItemResult<AccountInfo>>(request);
        }

        public Task<ActionResult> RegisterPushTokenAsync(PushTokenInput input)
        {
            var request = new RestRequest(Method.POST);
            request.Resource = "accounts/self/push_register";
            request.AddJsonBody(input);
            return this.Sdk.ExecuteAsync<ActionResult>(request);
        }
    }
}
