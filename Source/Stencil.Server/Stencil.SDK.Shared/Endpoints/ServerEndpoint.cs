using RestSharp;
using Stencil.SDK.Models;
using Stencil.SDK.Models.Responses;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.SDK.Endpoints
{
    public class ServerEndpoint : EndpointBase
    {
        public ServerEndpoint(StencilSDK api)
            : base(api)
        {

        }
        public Task<ItemResult<AppConfig>> GetAppConfigAsync(string platform)
        {
            var request = new RestRequest(Method.GET);
            request.Resource = "server/app_config";
            request.AddParameter("platform", platform);
            return this.Sdk.ExecuteAsync<ItemResult<AppConfig>>(request);
        }

        public Task<ItemResult<UpdateRequiredInfo>> GetIsUpdateRequiredAsync(string platform, string version)
        {
            var request = new RestRequest(Method.GET);
            request.Resource = "server/update_required";
            request.AddParameter("platform", platform);
            request.AddParameter("version", version);
            return this.Sdk.ExecuteAsync<ItemResult<UpdateRequiredInfo>>(request);
        }

    }
}
