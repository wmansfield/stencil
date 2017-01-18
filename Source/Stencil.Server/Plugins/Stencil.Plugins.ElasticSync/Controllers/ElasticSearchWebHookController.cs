using Codeable.Foundation.Common;
using Stencil.Plugins.ElasticSync.Integration;
using Stencil.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Stencil.Plugins.ElasticSync.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("api/datasync")]
    public class ElasticSearchWebHookController : RestApiBaseController
    {
        public ElasticSearchWebHookController(IFoundation foundation)
            : base(foundation)
        {
        }

        [HttpPost]
        [Route("failed")]
        public object OnSynchronousFailed(GenericInput input)
        {
            return base.ExecuteFunction("OnSynchronousFailed", delegate ()
            {
                string message = WebHook.ProcessWebHook(this.IFoundation, input.key, "failed", input.type, input.arg);

                return this.Http200(message, "");
            });
        }

        [HttpPost]
        [Route("sync")]
        public object OnSyncWasRequested(GenericInput input)
        {
            return base.ExecuteFunction("OnSyncWasRequested", delegate ()
            {
                string message = WebHook.ProcessWebHook(this.IFoundation, input.key, "sync", input.type, "");

                return this.Http200(message, "");
            });
        }


        [HttpPost]
        [Route("agitate")]
        public object Agitate(AgitateInput input)
        {
            return base.ExecuteFunction("Agitate", delegate ()
            {
                string message = WebHook.ProcessAgitateWebHook(this.IFoundation, input.key, input.name);

                return this.Http200(message, "");
            });
        }

        public class AgitateInput
        {
            public string key { get; set; }
            public string name { get; set; }
        }

        public class GenericInput
        {
            public string key { get; set; }
            public string type { get; set; }
            public string arg { get; set; }
        }

    }
}