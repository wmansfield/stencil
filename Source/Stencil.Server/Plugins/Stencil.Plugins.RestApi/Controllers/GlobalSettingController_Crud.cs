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
    [RoutePrefix("api/globalsettings")]
    public partial class GlobalSettingController : HealthRestApiController
    {
        public GlobalSettingController(IFoundation foundation)
            : base(foundation, "GlobalSetting")
        {
        }

        [HttpGet]
        [Route("{global_setting_id}")]
        public object GetById(Guid global_setting_id)
        {
            return base.ExecuteFunction<object>("GetById", delegate()
            {
                dm.GlobalSetting result = this.API.Direct.GlobalSettings.GetById(global_setting_id);
                if (result == null)
                {
                    return Http404("GlobalSetting");
                }

                

                return base.Http200(new ItemResult<sdk.GlobalSetting>()
                {
                    success = true,
                    item = result.ToSDKModel()
                });
            });
        }
        
        [HttpGet]
        [Route("")]
        public object Find(int skip = 0, int take = 10, string order_by = "", bool descending = false, string keyword = "")
        {
            return base.ExecuteFunction<object>("Find", delegate()
            {


                int takePlus = take;
                if (take != int.MaxValue)
                {
                    takePlus++; // for stepping
                }

                List<dm.GlobalSetting> result = this.API.Direct.GlobalSettings.Find(skip, takePlus, keyword, order_by, descending);
                return base.Http200(result.ToSteppedListResult(skip, take));

            });
        }
        
        
        
        
       

        [HttpPost]
        [Route("")]
        public object Create(sdk.GlobalSetting globalsetting)
        {
            return base.ExecuteFunction<object>("Create", delegate()
            {
                this.ValidateNotNull(globalsetting, "GlobalSetting");

                dm.GlobalSetting insert = globalsetting.ToDomainModel();

                
                insert = this.API.Direct.GlobalSettings.Insert(insert);
                

                
                sdk.GlobalSetting result = insert.ToSDKModel();

                return base.Http201(new ItemResult<sdk.GlobalSetting>()
                {
                    success = true,
                    item = result
                }
                , string.Format("api/globalsetting/{0}", globalsetting.global_setting_id));

            });

        }


        [HttpPut]
        [Route("{global_setting_id}")]
        public object Update(Guid global_setting_id, sdk.GlobalSetting globalsetting)
        {
            return base.ExecuteFunction<object>("Update", delegate()
            {
                this.ValidateNotNull(globalsetting, "GlobalSetting");
                this.ValidateRouteMatch(global_setting_id, globalsetting.global_setting_id, "GlobalSetting");

                globalsetting.global_setting_id = global_setting_id;
                dm.GlobalSetting update = globalsetting.ToDomainModel();


                update = this.API.Direct.GlobalSettings.Update(update);
                
                
                sdk.GlobalSetting existing = this.API.Direct.GlobalSettings.GetById(update.global_setting_id).ToSDKModel();
                
                return base.Http200(new ItemResult<sdk.GlobalSetting>()
                {
                    success = true,
                    item = existing
                });

            });

        }

        

        [HttpDelete]
        [Route("{global_setting_id}")]
        public object Delete(Guid global_setting_id)
        {
            return base.ExecuteFunction("Delete", delegate()
            {
                dm.GlobalSetting delete = this.API.Direct.GlobalSettings.GetById(global_setting_id);
                
                
                this.API.Direct.GlobalSettings.Delete(global_setting_id);

                return Http200(new ActionResult()
                {
                    success = true,
                    message = global_setting_id.ToString()
                });
            });
        }

    }
}

