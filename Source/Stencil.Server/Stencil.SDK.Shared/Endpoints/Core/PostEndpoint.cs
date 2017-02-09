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
    public partial class PostEndpoint : EndpointBase
    {
        public PostEndpoint(StencilSDK api)
            : base(api)
        {

        }
        
        public Task<ItemResult<Post>> GetPostAsync(Guid post_id)
        {
            var request = new RestRequest(Method.GET);
            request.Resource = "posts/{post_id}";
            request.AddUrlSegment("post_id", post_id.ToString());
            
            return this.Sdk.ExecuteAsync<ItemResult<Post>>(request);
        }
        
        public Task<ListResult<Post>> Find(int skip = 0, int take = 10, string keyword = "", string order_by = "", bool descending = false, Guid? account_id = null)
        {
            var request = new RestRequest(Method.GET);
            request.Resource = "posts";
            request.AddParameter("skip", skip);
            request.AddParameter("take", take);
            request.AddParameter("order_by", order_by);
            request.AddParameter("descending", descending);
            request.AddParameter("keyword", keyword);
            request.AddParameter("account_id", account_id);
            
            
            return this.Sdk.ExecuteAsync<ListResult<Post>>(request);
        }
        public Task<ListResult<Post>> GetPostByAccountAsync(Guid account_id, int skip = 0, int take = 10, string order_by = "", bool descending = false)
        {
            var request = new RestRequest(Method.GET);
            request.Resource = "posts/by_account/{account_id}";
            request.AddUrlSegment("account_id", account_id.ToString());
            request.AddParameter("skip", skip);
            request.AddParameter("take", take);
            request.AddParameter("order_by", order_by);
            request.AddParameter("descending", descending);
            
            return this.Sdk.ExecuteAsync<ListResult<Post>>(request);
        }
        

        public Task<ItemResult<Post>> CreatePostAsync(Post post)
        {
            var request = new RestRequest(Method.POST);
            request.Resource = "posts";
            request.AddJsonBody(post);
            return this.Sdk.ExecuteAsync<ItemResult<Post>>(request);
        }

        public Task<ItemResult<Post>> UpdatePostAsync(Guid post_id, Post post)
        {
            var request = new RestRequest(Method.PUT);
            request.Resource = "posts/{post_id}";
            request.AddUrlSegment("post_id", post_id.ToString());
            request.AddJsonBody(post);
            return this.Sdk.ExecuteAsync<ItemResult<Post>>(request);
        }

        

        public Task<ActionResult> DeletePostAsync(Guid post_id)
        {
            var request = new RestRequest(Method.DELETE);
            request.Resource = "posts/{post_id}";
            request.AddUrlSegment("post_id", post_id.ToString());
            return this.Sdk.ExecuteAsync<ActionResult>(request);
        }
    }
}
