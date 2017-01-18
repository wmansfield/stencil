using Codeable.Foundation.Common;
using Codeable.Foundation.Common.Aspect;
using Codeable.Foundation.Common.Daemons;
using Codeable.Foundation.Core;
using Codeable.Foundation.UI.Web.Common.Plugins;
using Codeable.Foundation.UI.Web.Core;
using Stencil.Common;
using Stencil.Common.Configuration;
using Stencil.Common.Integration;
using Stencil.Plugins.AzurePush.Integration;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Stencil.Plugins.AzurePush
{
    public class AzurePushPlugin : ChokeableClass, IWebPlugin
    {
        public AzurePushPlugin()
            : base(CoreFoundation.Current)
        {
        }
        #region IPlugin Members

        public string DisplayName
        {
            get { return "AzurePush"; }
        }
        public string DisplayVersion
        {
            get { return WebCoreUtility.GetInformationalVersion(Assembly.GetExecutingAssembly()); }
        }

        public bool WebInitialize(IFoundation foundation, IDictionary<string, string> pluginConfig)
        {
            base.IFoundation = foundation;
            return true;
        }
        public object InvokeCommand(string name, Dictionary<string, object> caseInsensitiveParameters)
        {
            return null;
        }

        #endregion

        #region IWebPlugin Members


        public void RegisterCustomRouting(System.Web.Routing.RouteCollection routes)
        {
            base.ExecuteMethod("RegisterCustomRouting", delegate ()
            {
                this.IFoundation.Container.RegisterInstance<IPushNotifications>(new AzurePushNotifier(this.IFoundation));

            });
        }
        public void UnRegisterCustomRouting(System.Web.Routing.RouteCollection routes)
        {

        }

        public void RegisterLegacyOverrides(LegacyOverrideCollection overrides)
        {
        }
        public void UnRegisterLegacyOverrides(LegacyOverrideCollection overrides)
        {
        }

        public int DesiredRegistrationPriority
        {
            get { return 0; }
        }

        public void OnWebPluginRegistered(IWebPlugin plugin)
        {
        }
        public void OnAfterWebPluginsRegistered(IEnumerable<IWebPlugin> allWebPlugins)
        {
        }

        public void OnWebPluginUnRegistered(IWebPlugin iWebPlugin)
        {
        }
        public void OnAfterWebPluginsUnRegistered(IWebPlugin[] iWebPlugin)
        {
        }
        public T RetrieveMetaData<T>(string token)
        {
            return default(T);
        }
        #endregion

    }
}