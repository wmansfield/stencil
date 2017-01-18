using Codeable.Foundation.Common.Aspect;
using Codeable.Foundation.Core;
using Codeable.Foundation.Core.Caching;
using Codeable.Foundation.UI.Web.Common.Plugins;
using Codeable.Foundation.UI.Web.Core;
using Microsoft.Practices.Unity;
using Stencil.Plugins.Emails.Emailing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Codeable.Foundation.Common.Emailing;
using Stencil.Common.Integration;
using Stencil.Common.Configuration;
using Codeable.Foundation.Common;
using Stencil.Common;

namespace Stencil.Plugins.Emails
{
    public class EmailPlugin : ChokeableClass, IWebPlugin
    {
        public EmailPlugin()
            : base(CoreFoundation.Current)
        {
        }

        public static AspectCache Cache15 { get; set; }

        public int DesiredRegistrationPriority
        {
            get { return 0; }
        }

        public void OnAfterWebPluginsRegistered(IEnumerable<IWebPlugin> allWebPlugins)
        {
        }

        public void OnAfterWebPluginsUnRegistered(IWebPlugin[] iWebPlugin)
        {
        }

        public void OnWebPluginRegistered(IWebPlugin plugin)
        {
        }

        public void OnWebPluginUnRegistered(IWebPlugin iWebPlugin)
        {
        }

        public void RegisterCustomRouting(System.Web.Routing.RouteCollection routes)
        {
            base.ExecuteMethod("RegisterCustomRouting", delegate ()
            {
                SmtpEmailTransport transport = this.IFoundation.Container.Resolve<SmtpEmailTransport>();
                base.IFoundation.Container.RegisterInstance<IEmailTransport>(transport, new ContainerControlledLifetimeManager());
                base.IFoundation.Container.RegisterInstance<INotifyAdmin>(transport, new ContainerControlledLifetimeManager());
                
            });
        }

        public void RegisterLegacyOverrides(LegacyOverrideCollection overrides)
        {
        }

        public void UnRegisterCustomRouting(System.Web.Routing.RouteCollection routes)
        {
            base.ExecuteMethod("UnRegisterCustomRouting", delegate ()
            {
                
            });
        }

        public void UnRegisterLegacyOverrides(LegacyOverrideCollection overrides)
        {
        }

        public bool WebInitialize(Codeable.Foundation.Common.IFoundation foundation, IDictionary<string, string> pluginConfig)
        {
            return true;
        }

        public string DisplayName
        {
            get { return "EmailPlugin"; }
        }

        public string DisplayVersion
        {
            get { return WebCoreUtility.GetInformationalVersion(Assembly.GetExecutingAssembly()); }
        }

        public object InvokeCommand(string name, Dictionary<string, object> caseInsensitiveParameters)
        {
            return null;
        }

        public T RetrieveMetaData<T>(string token)
        {
            return default(T);
        }
    }
}
