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
    [RoutePrefix("api/accounts")]
    public partial class AccountController : HealthRestApiController
    {
        public AccountController(IFoundation foundation)
            : base(foundation, "Account")
        {
        }

        [HttpGet]
        [Route("{account_id}")]
        public object GetById(Guid account_id)
        {
            return base.ExecuteFunction<object>("GetById", delegate()
            {
                sdk.Account result = this.API.Index.Accounts.GetById(account_id);
                if (result == null)
                {
                    return Http404("Account");
                }

                

                return base.Http200(new ItemResult<sdk.Account>()
                {
                    success = true, 
                    item = result
                });
            });
        }
        
        
        [HttpGet]
        [Route("")]
        public object Find(int skip = 0, int take = 10, string order_by = "", bool descending = false, string keyword = "")
        {
            return base.ExecuteFunction<object>("Find", delegate()
            {
                
                ListResult<sdk.Account> result = this.API.Index.Accounts.Find(skip, take, keyword, order_by, descending);
                result.success = true;
                return base.Http200(result);
            });
        }
        
        
       

        [HttpPost]
        [Route("")]
        public object Create(sdk.Account account)
        {
            return base.ExecuteFunction<object>("Create", delegate()
            {
                this.ValidateNotNull(account, "Account");

                dm.Account insert = account.ToDomainModel();

                
                insert = this.API.Direct.Accounts.Insert(insert);
                

                
                sdk.Account result = this.API.Index.Accounts.GetById(insert.account_id);

                return base.Http201(new ItemResult<sdk.Account>()
                {
                    success = true,
                    item = result
                }
                , string.Format("api/account/{0}", account.account_id));

            });

        }


        [HttpPut]
        [Route("{account_id}")]
        public object Update(Guid account_id, sdk.Account account)
        {
            return base.ExecuteFunction<object>("Update", delegate()
            {
                this.ValidateNotNull(account, "Account");
                this.ValidateRouteMatch(account_id, account.account_id, "Account");

                account.account_id = account_id;
                dm.Account update = account.ToDomainModel();


                update = this.API.Direct.Accounts.Update(update);
                
                
                sdk.Account existing = this.API.Index.Accounts.GetById(update.account_id);
                
                
                return base.Http200(new ItemResult<sdk.Account>()
                {
                    success = true,
                    item = existing
                });

            });

        }

        

        [HttpDelete]
        [Route("{account_id}")]
        public object Delete(Guid account_id)
        {
            return base.ExecuteFunction("Delete", delegate()
            {
                dm.Account delete = this.API.Direct.Accounts.GetById(account_id);
                
                
                this.API.Direct.Accounts.Delete(account_id);

                return Http200(new ActionResult()
                {
                    success = true,
                    message = account_id.ToString()
                });
            });
        }

    }
}

