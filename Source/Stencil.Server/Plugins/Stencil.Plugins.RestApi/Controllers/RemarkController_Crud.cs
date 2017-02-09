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
    [RoutePrefix("api/remarks")]
    public partial class RemarkController : HealthRestApiController
    {
        public RemarkController(IFoundation foundation)
            : base(foundation, "Remark")
        {
        }

        [HttpGet]
        [Route("{remark_id}")]
        public object GetById(Guid remark_id)
        {
            return base.ExecuteFunction<object>("GetById", delegate()
            {
                sdk.Remark result = this.API.Index.Remarks.GetById(remark_id);
                if (result == null)
                {
                    return Http404("Remark");
                }

                

                return base.Http200(new ItemResult<sdk.Remark>()
                {
                    success = true, 
                    item = result
                });
            });
        }
        
        
        [HttpGet]
        [Route("")]
        public object Find(int skip = 0, int take = 10, string order_by = "", bool descending = false, string keyword = "", Guid? post_id = null, Guid? account_id = null)
        {
            return base.ExecuteFunction<object>("Find", delegate()
            {
                
                ListResult<sdk.Remark> result = this.API.Index.Remarks.Find(skip, take, keyword, order_by, descending, post_id, account_id);
                result.success = true;
                return base.Http200(result);
            });
        }
        [HttpGet]
        [Route("by_post/{post_id}")]
        public object GetByPost(Guid post_id, int skip = 0, int take = 10, string order_by = "", bool descending = false)
        {
            return base.ExecuteFunction<object>("GetByPost", delegate ()
            {
                
                
                ListResult<sdk.Remark> result = this.API.Index.Remarks.GetByPost(post_id, skip, take, order_by, descending);
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
                
                
                ListResult<sdk.Remark> result = this.API.Index.Remarks.GetByAccount(account_id, skip, take, order_by, descending);
                result.success = true;
                return base.Http200(result);
            });
        }
        
        
        
       

        [HttpPost]
        [Route("")]
        public object Create(sdk.Remark remark)
        {
            return base.ExecuteFunction<object>("Create", delegate()
            {
                this.ValidateNotNull(remark, "Remark");

                dm.Remark insert = remark.ToDomainModel();

                
                insert = this.API.Direct.Remarks.Insert(insert);
                

                
                sdk.Remark result = this.API.Index.Remarks.GetById(insert.remark_id);

                return base.Http201(new ItemResult<sdk.Remark>()
                {
                    success = true,
                    item = result
                }
                , string.Format("api/remark/{0}", remark.remark_id));

            });

        }


        [HttpPut]
        [Route("{remark_id}")]
        public object Update(Guid remark_id, sdk.Remark remark)
        {
            return base.ExecuteFunction<object>("Update", delegate()
            {
                this.ValidateNotNull(remark, "Remark");
                this.ValidateRouteMatch(remark_id, remark.remark_id, "Remark");

                remark.remark_id = remark_id;
                dm.Remark update = remark.ToDomainModel();


                update = this.API.Direct.Remarks.Update(update);
                
                
                sdk.Remark existing = this.API.Index.Remarks.GetById(update.remark_id);
                
                
                return base.Http200(new ItemResult<sdk.Remark>()
                {
                    success = true,
                    item = existing
                });

            });

        }

        

        [HttpDelete]
        [Route("{remark_id}")]
        public object Delete(Guid remark_id)
        {
            return base.ExecuteFunction("Delete", delegate()
            {
                dm.Remark delete = this.API.Direct.Remarks.GetById(remark_id);
                
                
                this.API.Direct.Remarks.Delete(remark_id);

                return Http200(new ActionResult()
                {
                    success = true,
                    message = remark_id.ToString()
                });
            });
        }

    }
}

