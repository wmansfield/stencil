using Codeable.Foundation.Common;
using Codeable.Foundation.Core;
using System;
using System.Web.Http;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using sdk = Stencil.SDK.Models;
using dm = Stencil.Domain;
using Stencil.Primary;
using Stencil.SDK;
using Stencil.Web.Controllers;
using Stencil.Web.Security;

namespace Stencil.Plugins.RestAPI.Controllers
{
    [ApiKeyHttpAuthorize]
    [RoutePrefix("api/posts")]
    public partial class PostController : HealthRestApiController
    {
        public PostController(IFoundation foundation)
            : base(foundation, "Post")
        {
        }

        [HttpGet]
        [Route("{post_id}")]
        public object GetById(Guid post_id)
        {
            return base.ExecuteFunction<object>("GetById", delegate()
            {
                dm.Account currentAccount = this.GetCurrentAccount();
                sdk.Post result = this.API.Index.Posts.GetById(post_id, currentAccount.account_id);
                if (result == null)
                {
                    return Http404("Post");
                }

                

                return base.Http200(new ItemResult<sdk.Post>()
                {
                    success = true, 
                    item = result
                });
            });
        }
        
        
        [HttpGet]
        [Route("")]
        public object Find(int skip = 0, int take = 10, string order_by = "", bool descending = false, string keyword = "", Guid? account_id = null)
        {
            return base.ExecuteFunction<object>("Find", delegate()
            {
                dm.Account currentAccount = this.GetCurrentAccount();
                ListResult<sdk.Post> result = this.API.Index.Posts.Find(currentAccount.account_id, skip, take, keyword, order_by, descending, account_id);
                result.success = true;
                return base.Http200(result);
            });
        }
        [HttpGet]
        [Route("by_account/{account_id}")]
        public object GetByAccount(Guid account_id, int skip = 0, int take = 10, string order_by = "", bool descending = false)
        {
            return base.ExecuteFunction<object>("GetByAccount", delegate ()
            {
                
                dm.Account currentAccount = this.GetCurrentAccount();
                ListResult<sdk.Post> result = this.API.Index.Posts.GetByAccount(account_id, skip, take, order_by, descending, currentAccount.account_id);
                result.success = true;
                return base.Http200(result);
            });
        }
        
        
        
       

        [HttpPost]
        [Route("")]
        public object Create(sdk.Post post)
        {
            return base.ExecuteFunction<object>("Create", delegate()
            {
                this.ValidateNotNull(post, "Post");

                dm.Post insert = post.ToDomainModel();

                
                insert = this.API.Direct.Posts.Insert(insert);
                

                
                sdk.Post result = this.API.Index.Posts.GetById(insert.post_id);

                return base.Http201(new ItemResult<sdk.Post>()
                {
                    success = true,
                    item = result
                }
                , string.Format("api/post/{0}", post.post_id));

            });

        }


        [HttpPut]
        [Route("{post_id}")]
        public object Update(Guid post_id, sdk.Post post)
        {
            return base.ExecuteFunction<object>("Update", delegate()
            {
                this.ValidateNotNull(post, "Post");
                this.ValidateRouteMatch(post_id, post.post_id, "Post");

                post.post_id = post_id;
                dm.Post update = post.ToDomainModel();


                update = this.API.Direct.Posts.Update(update);
                
                
                sdk.Post existing = this.API.Index.Posts.GetById(update.post_id);
                
                
                return base.Http200(new ItemResult<sdk.Post>()
                {
                    success = true,
                    item = existing
                });

            });

        }

        

        [HttpDelete]
        [Route("{post_id}")]
        public object Delete(Guid post_id)
        {
            return base.ExecuteFunction("Delete", delegate()
            {
                dm.Post delete = this.API.Direct.Posts.GetById(post_id);
                
                
                this.API.Direct.Posts.Delete(post_id);

                return Http200(new ActionResult()
                {
                    success = true,
                    message = post_id.ToString()
                });
            });
        }

    }
}

