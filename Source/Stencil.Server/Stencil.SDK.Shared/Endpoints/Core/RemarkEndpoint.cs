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
    public partial class RemarkEndpoint : EndpointBase
    {
        public RemarkEndpoint(StencilSDK api)
            : base(api)
        {

        }
        
        public Task<ItemResult<Remark>> GetRemarkAsync(Guid remark_id)
        {
            var request = new RestRequest(Method.GET);
            request.Resource = "remarks/{remark_id}";
            request.AddUrlSegment("remark_id", remark_id.ToString());
            
            return this.Sdk.ExecuteAsync<ItemResult<Remark>>(request);
        }
        
        public Task<ListResult<Remark>> Find(int skip = 0, int take = 10, string keyword = "", string order_by = "", bool descending = false, Guid? post_id = null, Guid? account_id = null)
        {
            var request = new RestRequest(Method.GET);
            request.Resource = "remarks";
            request.AddParameter("skip", skip);
            request.AddParameter("take", take);
            request.AddParameter("order_by", order_by);
            request.AddParameter("descending", descending);
            request.AddParameter("keyword", keyword);
            request.AddParameter("post_id", post_id);
            request.AddParameter("account_id", account_id);
            
            
            return this.Sdk.ExecuteAsync<ListResult<Remark>>(request);
        }
        public Task<ListResult<Remark>> GetRemarkByPostAsync(Guid post_id, int skip = 0, int take = 10, string order_by = "", bool descending = false)
        {
            var request = new RestRequest(Method.GET);
            request.Resource = "remarks/by_post/{post_id}";
            request.AddUrlSegment("post_id", post_id.ToString());
            request.AddParameter("skip", skip);
            request.AddParameter("take", take);
            request.AddParameter("order_by", order_by);
            request.AddParameter("descending", descending);
            
            return this.Sdk.ExecuteAsync<ListResult<Remark>>(request);
        }
        
        public Task<ListResult<Remark>> GetRemarkByAccountAsync(Guid account_id, int skip = 0, int take = 10, string order_by = "", bool descending = false)
        {
            var request = new RestRequest(Method.GET);
            request.Resource = "remarks/by_account/{account_id}";
            request.AddUrlSegment("account_id", account_id.ToString());
            request.AddParameter("skip", skip);
            request.AddParameter("take", take);
            request.AddParameter("order_by", order_by);
            request.AddParameter("descending", descending);
            
            return this.Sdk.ExecuteAsync<ListResult<Remark>>(request);
        }
        

        public Task<ItemResult<Remark>> CreateRemarkAsync(Remark remark)
        {
            var request = new RestRequest(Method.POST);
            request.Resource = "remarks";
            request.AddJsonBody(remark);
            return this.Sdk.ExecuteAsync<ItemResult<Remark>>(request);
        }

        public Task<ItemResult<Remark>> UpdateRemarkAsync(Guid remark_id, Remark remark)
        {
            var request = new RestRequest(Method.PUT);
            request.Resource = "remarks/{remark_id}";
            request.AddUrlSegment("remark_id", remark_id.ToString());
            request.AddJsonBody(remark);
            return this.Sdk.ExecuteAsync<ItemResult<Remark>>(request);
        }

        

        public Task<ActionResult> DeleteRemarkAsync(Guid remark_id)
        {
            var request = new RestRequest(Method.DELETE);
            request.Resource = "remarks/{remark_id}";
            request.AddUrlSegment("remark_id", remark_id.ToString());
            return this.Sdk.ExecuteAsync<ActionResult>(request);
        }
    }
}
