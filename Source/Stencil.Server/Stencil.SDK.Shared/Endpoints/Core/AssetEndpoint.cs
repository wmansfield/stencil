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

namespace Stencil.SDK.Endpoints
{
    public partial class AssetEndpoint : EndpointBase
    {
        public AssetEndpoint(StencilSDK api)
            : base(api)
        {

        }
        
        public Task<ItemResult<Asset>> GetAssetAsync(Guid asset_id)
        {
            var request = new RestRequest(Method.GET);
            request.Resource = "assets/{asset_id}";
            request.AddUrlSegment("asset_id", asset_id.ToString());
            
            return this.Sdk.ExecuteAsync<ItemResult<Asset>>(request);
        }
        
        

        public Task<ItemResult<Asset>> CreateAssetAsync(Asset asset)
        {
            var request = new RestRequest(Method.POST);
            request.Resource = "assets";
            request.AddJsonBody(asset);
            return this.Sdk.ExecuteAsync<ItemResult<Asset>>(request);
        }

        public Task<ItemResult<Asset>> UpdateAssetAsync(Guid asset_id, Asset asset)
        {
            var request = new RestRequest(Method.PUT);
            request.Resource = "assets/{asset_id}";
            request.AddUrlSegment("asset_id", asset_id.ToString());
            request.AddJsonBody(asset);
            return this.Sdk.ExecuteAsync<ItemResult<Asset>>(request);
        }

        

        public Task<ActionResult> DeleteAssetAsync(Guid asset_id)
        {
            var request = new RestRequest(Method.DELETE);
            request.Resource = "assets/{asset_id}";
            request.AddUrlSegment("asset_id", asset_id.ToString());
            return this.Sdk.ExecuteAsync<ActionResult>(request);
        }
    }
}
