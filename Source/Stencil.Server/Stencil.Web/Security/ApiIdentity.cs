using System;
using System.Security.Principal;

namespace Stencil.Web.Security
{
    public class ApiIdentity : GenericIdentity
    {
        public ApiIdentity(Guid appId, string appName)
            : base(appName, "api")
        {
            this.ApiID = appId;
            this.ApiName = appName;
        }

        public Guid ApiID { get; set; }
        public string ApiName { get; set; }
    }
}