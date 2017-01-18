using Codeable.Foundation.Common;
using Codeable.Foundation.Common.Aspect;
using Codeable.Foundation.Common.Daemons;
using Codeable.Foundation.Core;
using Codeable.Foundation.UI.Web.Common.Plugins;
using Codeable.Foundation.UI.Web.Core;
using Stencil.Common;
using Stencil.Common.Configuration;
using Stencil.Common.Integration;
using Stencil.Plugins.Amazon.Daemons;
using Stencil.Plugins.Amazon.Integration;
using Stencil.Primary.Integration;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Stencil.Plugins.Amazon
{
    public class AmazonPlugin : ChokeableClass, IWebPlugin
    {
        public AmazonPlugin()
            : base(CoreFoundation.Current)
        {
        }
        #region IPlugin Members

        public string DisplayName
        {
            get { return "Amazon"; }
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
                AmazonUploadFile uploadFile = new AmazonUploadFile(this.IFoundation);
                this.IFoundation.Container.RegisterInstance<IUploadFiles>(uploadFile);
                this.IFoundation.Container.RegisterInstance<INotifyEncoder>(uploadFile);

                this.IFoundation.Container.RegisterInstance<IProcessImage>(new AmazonImageResizeDaemon(this.IFoundation)); // isolated instance, isnt actually ran as a daemon

                IDaemonManager daemonManager = this.IFoundation.GetDaemonManager();
                ISettingsResolver settingsResolver = this.IFoundation.Resolve<ISettingsResolver>();
                bool isBackPane = settingsResolver.IsBackPane();
                bool isLocalHost = settingsResolver.IsLocalHost();
                bool isHydrate = settingsResolver.IsHydrate();
                if (isBackPane && !isLocalHost && !isHydrate)
                {
                    DaemonConfig config = new DaemonConfig()
                    {
                        InstanceName = AmazonEncodingDaemon.DAEMON_NAME,
                        ContinueOnError = true,
                        IntervalMilliSeconds = (int)TimeSpan.FromMinutes(1).TotalMilliseconds,
                        StartDelayMilliSeconds = 15,
                        TaskConfiguration = string.Empty
                    };
                    daemonManager.RegisterDaemon(config, new AmazonEncodingDaemon(this.IFoundation), true);

                    config = new DaemonConfig()
                    {
                        InstanceName = AmazonImageResizeDaemon.DAEMON_NAME,
                        ContinueOnError = true,
                        IntervalMilliSeconds = (int)TimeSpan.FromMinutes(1).TotalMilliseconds,
                        StartDelayMilliSeconds = 15,
                        TaskConfiguration = string.Empty
                    };
                    daemonManager.RegisterDaemon(config, new AmazonImageResizeDaemon(this.IFoundation), true);
                }

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