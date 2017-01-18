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
    public class AuthEndpoint : EndpointBase
    {
        public AuthEndpoint(StencilSDK api)
            : base(api)
        {

        }

        public Task<ItemResult<AccountInfo>> LoginAsync(AuthLoginInput input)
        {
            var request = new RestRequest(Method.POST);
            request.Resource = "auth/login";
            request.AddJsonBody(input);
            return this.Sdk.ExecuteAsync<ItemResult<AccountInfo>>(request);
        }
        public Task<ItemResult<AccountInfo>> RegisterAsync(RegisterInput input)
        {
            var request = new RestRequest(Method.POST);
            request.Resource = "auth/register";
            request.AddJsonBody(input);
            return this.Sdk.ExecuteAsync<ItemResult<AccountInfo>>(request);
        }

        public Task<ActionResult> LogoutAsync()
        {
            var request = new RestRequest(Method.POST);
            request.Resource = "auth/logout";
            return this.Sdk.ExecuteAsync<ActionResult>(request);
        }

        public Task<ActionResult> PasswordResetStartAsync(string email)
        {
            PasswordResetInput input = new PasswordResetInput()
            {
                email = email
            };
            var request = new RestRequest(Method.POST);
            request.Resource = "auth/password_reset/start";
            request.AddJsonBody(input);
            return this.Sdk.ExecuteAsync<ActionResult>(request);
        }
        public Task<ActionResult> PasswordResetCompleteAsync(string email, string token, string password)
        {
            PasswordResetInput input = new PasswordResetInput()
            {
                email = email,
                password = password,
                token = token
            };
            var request = new RestRequest(Method.POST);
            request.Resource = "auth/password_reset/complete";
            request.AddJsonBody(input);
            return this.Sdk.ExecuteAsync<ActionResult>(request);
        }
    }
}
