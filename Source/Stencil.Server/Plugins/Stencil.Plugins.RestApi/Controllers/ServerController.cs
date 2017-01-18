using Codeable.Foundation.Common;
using Stencil.SDK;
using Stencil.SDK.Models;
using Stencil.SDK.Models.Responses;
using Stencil.Web.Controllers;
using Stencil.Web.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Stencil.Plugins.RestAPI.Controllers
{
    [RoutePrefix("api/server")]
    public partial class ServerController : RestApiBaseController
    {
        public ServerController(IFoundation foundation)
            : base(foundation)
        {
        }


        [HttpGet]
        [Route("update_required")]
        public object GetUpdateRequired(string platform)
        {
            return base.ExecuteFunction<object>("GetUpdateRequired", delegate ()
            {
                UpdateRequiredInfo result = new UpdateRequiredInfo();
                #pragma warning disable 612, 618

                switch (platform)
                {
                    case "ios":
                        if (this.IsClientVersionLessThan("1.0.0"))
                        {
                            result = new UpdateRequiredInfo()
                            {
                                required = true,
                                message = "There is a new version of the app!"
                            };
                        }
                        break;
                    case "android":
                        if (this.IsClientVersionLessThan("1.0.0"))
                        {
                            result = new UpdateRequiredInfo()
                            {
                                required = true,
                                message = "There is a new version of the app!"
                            };
                        }
                        break;
                    default:
                        break;
                }
                #pragma warning restore 612, 618

                return base.Http200(new ItemResult<UpdateRequiredInfo>()
                {
                    success = true,
                    item = result
                });
            });
        }


        [HttpGet]
        [Route("app_config")]
        public object GetAppConfig(string platform)
        {
            return base.ExecuteFunction<object>("GetAppConfig", delegate ()
            {
                // nothing yet.
                return base.Http200(new ItemResult<AppConfig>()
                {
                    success = false,
                    item = null
                });
            });
        }

    }
}