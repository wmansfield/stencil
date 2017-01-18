using Codeable.Foundation.Common;
using Codeable.Foundation.Common.Aspect;
using Codeable.Foundation.Common.Daemons;
using Codeable.Foundation.Core.Caching;
using Codeable.Foundation.Core.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Stencil.Common.Configuration;
using Stencil.Common;

namespace Stencil.Primary.Health.Daemons
{
    public class HealthReportDaemon : ChokeableClass, IDaemonTask
    {
        public HealthReportDaemon(IFoundation iFoundation)
            : base(iFoundation)
        {
        }

        #region IDaemonTask Members

        public const string DAEMON_NAME = "HealthReportDaemon";

        protected static bool _executing;

        public string DaemonName
        {
            get
            {
                return DAEMON_NAME;
            }
            protected set
            {
            }
        }

        public void Execute(Codeable.Foundation.Common.IFoundation iFoundation)
        {
            base.ExecuteMethod("Execute", delegate()
            {
                if (_executing) { return; } // safety

                try
                {
                    _executing = true;
                    this.PersistHealth();
                }
                finally
                {
                    _executing = false;
                }
            });
        }

        public DaemonSynchronizationPolicy SynchronizationPolicy
        {
            get { return DaemonSynchronizationPolicy.SingleAppDomain; }
        }

        #endregion

        protected void PersistHealth()
        {
            base.ExecuteMethod("PersistHealth", delegate()
            {
                base.IFoundation.LogWarning("Sending Health Reports");

                string hostName = Dns.GetHostName();
                Dictionary<string, decimal> metrics = null;
                List<string> logs = null;
                HealthReporter.Current.ResetMetrics(out metrics, out logs);

                string suffix = DateTime.UtcNow.ToUnixSecondsUTC().ToString();
                foreach (var item in metrics)
                {
                    logs.Add(string.Format("{0}.{1} {2} {3}", hostName, item.Key, (int)item.Value, suffix));
                }

                ISettingsResolver settingsResolver = this.IFoundation.Resolve<ISettingsResolver>();

                if (!settingsResolver.IsLocalHost())
                {
                    string apiKey = this.ApiKey;
                    if(!string.IsNullOrEmpty(apiKey))
                    {
                        HostedGraphiteTcpClient client = new HostedGraphiteTcpClient(apiKey);
                        client.SendMany(logs);
                    }
                }
            });
        }


        protected virtual string ApiKey
        {
            get
            {
                ISettingsResolver settings = this.IFoundation.Resolve<ISettingsResolver>();
                return settings.GetSetting(CommonAssumptions.APP_KEY_HEALTH_APIKEY);
            }
        }
    }
}
