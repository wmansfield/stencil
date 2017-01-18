using Stencil.Domain;
using Stencil.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Stencil.Web.Security
{
    public static class _SecurityExtensions
    {
        public static string GetClientPlatformAndVersion(this RestApiBaseController controller)
        {
            return string.Format("{0}-v{1}", controller.GetClientPlatform(), controller.GetClientPlatformVersion(2));
        }
        public static string GetClientPlatform(this RestApiBaseController controller)
        {
            try
            {
                if (controller != null && controller.Request != null && controller.Request.Headers != null)
                {
                    if (controller.Request.Headers.Contains("X-DevicePlatform"))
                    {
                        return controller.Request.Headers.GetValues("X-DevicePlatform").FirstOrDefault();
                    }
                }
            }
            catch
            {
                // gulp
            }

            return "web";
        }
        public static Version GetClientPlatformVersion(this RestApiBaseController controller, int precision)
        {
            Version result = new Version("1.0");
            try
            {
                if (controller != null && controller.Request != null && controller.Request.Headers != null)
                {
                    if (controller.Request.Headers.Contains("X-DeviceVersion"))
                    {
                        string version = controller.Request.Headers.GetValues("X-DeviceVersion").FirstOrDefault();
                        int count = precision - version.Count(x => x == '.') - 1;
                        for (int i = 0; i < count; i++)
                        {
                            version += ".0";
                        }
                        result = new Version(version);
                    }
                }
            }
            catch
            {
                // gulp
            }

            return result;
        }
        public static Version GetClientPlatformVersion(this RestApiBaseController controller, string platform, int precision)
        {
            Version result = new Version("1.0");
            try
            {
                if (controller != null && controller.Request != null && controller.Request.Headers != null)
                {
                    if (controller.Request.Headers.Contains("X-DeviceVersion") && controller.Request.Headers.Contains("X-DevicePlatform"))
                    {
                        if (controller.Request.Headers.GetValues("X-DevicePlatform").FirstOrDefault() == platform)
                        {
                            string version = controller.Request.Headers.GetValues("X-DeviceVersion").FirstOrDefault();
                            int count = precision - version.Count(x => x == '.') - 1;
                            for (int i = 0; i < count; i++)
                            {
                                version += ".0";
                            }
                            result = new Version(version);
                        }
                    }
                }
            }
            catch
            {
                // gulp
            }

            return result;
        }
        public static Account GetCurrentAccount(this RestApiBaseController controller)
        {
            if (controller != null && controller.Request != null && controller.Request.Properties != null
                && controller.Request.Properties.ContainsKey(ApiKeyHttpAuthorize.CURRENT_ACCOUNT_HTTP_CONTEXT_KEY))
            {
                return controller.Request.Properties[ApiKeyHttpAuthorize.CURRENT_ACCOUNT_HTTP_CONTEXT_KEY] as Account;
            }
            return null;
        }
        public static Account GetCurrentAccount(this MvcBaseController controller)
        {
            if (controller != null && controller.ControllerContext != null && controller.ControllerContext.HttpContext != null && controller.ControllerContext.HttpContext.Items != null
                && controller.ControllerContext.HttpContext.Items.Contains(ApiKeyHttpAuthorize.CURRENT_ACCOUNT_HTTP_CONTEXT_KEY))
            {
                return controller.ControllerContext.HttpContext.Items[ApiKeyHttpAuthorize.CURRENT_ACCOUNT_HTTP_CONTEXT_KEY] as Account;
            }
            return null;
        }
        public static void ValidateAdmin(this RestApiBaseController controller)
        {
            Account admin = controller.GetCurrentAccount();
            if (admin == null || !admin.IsAdmin())
            {
                string message = "Unable to access required item";
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent(message),
                    ReasonPhrase = message,
                });
            }
        }
        
        public static void ThrowUnauthorized(this RestApiBaseController controller)
        {
            string message = "Unable to access required item";
            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent(message),
                ReasonPhrase = message,
            });
        }

        [Obsolete("Do not keep these checks forever", false)]
        public static bool IsClientVersionLessThan(this RestApiBaseController controller, string version)
        {
            return controller.GetClientPlatformVersion(version.Count(x => x == '.') + 1) < new Version(version);
        }
        [Obsolete("Do not keep these checks forever", false)]
        public static bool IsClientVersionLessThanOrEqualTo(this RestApiBaseController controller, string version)
        {
            return controller.GetClientPlatformVersion(version.Count(x => x == '.') + 1) <= new Version(version);
        }
        [Obsolete("Do not keep these checks forever", false)]
        public static bool IsClientVersionEqualToOrGreaterThan(this RestApiBaseController controller, string version)
        {
            return controller.GetClientPlatformVersion(version.Count(x => x == '.') + 1) >= new Version(version);
        }
    }
}
