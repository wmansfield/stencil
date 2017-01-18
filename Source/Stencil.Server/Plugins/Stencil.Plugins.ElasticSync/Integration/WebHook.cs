using Codeable.Foundation.Common;
using Codeable.Foundation.Common.Daemons;
using Stencil.Plugins.ElasticSync.Daemons;
using Stencil.Primary.Daemons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stencil.Plugins.ElasticSync.Integration
{
    public static class WebHook
    {
        /// <summary>
        /// Not aspect wrapped
        /// </summary>
        public static string ProcessWebHook(IFoundation foundation, string secretkey, string hookType, string entityType, string argument)
        {
            string result = "";
            if (secretkey == "codeable")
            {
                IDaemonManager daemonManager = foundation.GetDaemonManager();

                switch (hookType)
                {
                    case "sync":
                    case "failed":
                        daemonManager.StartDaemon(string.Format(ElasticSearchDaemon.DAEMON_NAME_FORMAT, Agents.AGENT_DEFAULT));
                        daemonManager.StartDaemon(string.Format(ElasticSearchDaemon.DAEMON_NAME_FORMAT, Agents.AGENT_STATS));
                        result = "Queued Normal Sync";
                        break;
                    default:
                        break;
                }
            }
            return result;
        }

        /// <summary>
        /// Not aspect wrapped
        /// </summary>
        public static string ProcessAgitateWebHook(IFoundation foundation, string secretkey, string daemonName)
        {
            string result = "";
            if (secretkey == "codeable")
            {
                IDaemonManager daemonManager = foundation.GetDaemonManager();
                if (null != daemonManager.GetRegisteredDaemonTask(daemonName))
                {
                    daemonManager.StartDaemon(daemonName);
                    result = "Agitated";
                }
            }
            return result;
        }
    }
}
