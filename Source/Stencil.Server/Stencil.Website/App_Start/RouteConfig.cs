using Codeable.Foundation.Common;
using Codeable.Foundation.Core;
using Codeable.Foundation.UI.Web.Core.MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Stencil.Website
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            // Foundation: Step 2b
            IWebPluginLoader pluginLoader = CoreFoundation.Current.Resolve<IWebPluginLoader>();
            pluginLoader.RegisterPluginRoutes(routes);

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
