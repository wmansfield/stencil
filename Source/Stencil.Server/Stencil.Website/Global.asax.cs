using Codeable.Foundation.Core;
using Codeable.Foundation.UI.Web.Core.Http;
using Stencil.Web.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Stencil.Website
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private IHttpApplicationBinder _iHttpApplicationBinder;
        protected void Application_Start()
        {


            ServicePointManager.DefaultConnectionLimit = 500;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;
            ThreadPool.SetMinThreads(100, 100);

            // Foundation: Step 1
            CoreFoundation.Initialize(new StencilWebBootstrap(), true);

            // Foundation: Step 2 (inside RouteConfig.RegisterRoutes)

            GlobalConfiguration.Configure(WebApiConfig.Register);
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Foundation: Step 3
            _iHttpApplicationBinder = CoreFoundation.Current.SafeResolve<IHttpApplicationBinder>();
        }
    }
}
