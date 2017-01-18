using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Stencil.Web.Controllers
{
    public static class _ControllerExtensions
    {
        public static string GetRequestRootUrl(this ApiController controller)
        {
            if (controller.Request.IsLocal())
            {
                return controller.Request.RequestUri.GetComponents(UriComponents.SchemeAndServer | UriComponents.HostAndPort, UriFormat.Unescaped);
            }
            else
            {
                return controller.Request.RequestUri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Host, UriFormat.Unescaped);
            }
        }
    }
}
