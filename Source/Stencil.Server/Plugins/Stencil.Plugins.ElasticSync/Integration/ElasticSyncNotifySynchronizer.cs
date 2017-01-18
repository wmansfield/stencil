using Codeable.Foundation.Common;
using Codeable.Foundation.Common.Aspect;
using Codeable.Foundation.Core.Caching;
using Codeable.Foundation.Core.Unity;
using Stencil.Common;
using Stencil.Common.Configuration;
using Stencil.Common.Synchronization;
using Stencil.Plugins.ElasticSync.Daemons;
using Stencil.Primary;
using Stencil.Primary.Daemons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Stencil.Plugins.ElasticSync.Integration
{
    public class ElasticSyncNotifySynchronizer : ChokeableClass, INotifySynchronizer
    {
        public ElasticSyncNotifySynchronizer(IFoundation foundation)
            : base(foundation)
        {
            this.API = foundation.Resolve<StencilAPI>();
            this.Cache = new AspectCache("ElasticSyncNotifySynchronizer", foundation, new ExpireStaticLifetimeManager("ElasticSyncNotifySynchronizer.Life15", System.TimeSpan.FromMinutes(15), false));
        }

        public AspectCache Cache { get; set; }
        public StencilAPI API { get; set; }

        protected string DaemonUrl
        {
            get
            {
                return this.Cache.PerLifetime("DaemonUrl", delegate ()
                {
                    if (this.API.Integration.SettingsResolver.IsLocalHost())
                    {
                        return "http://localhost:4361/";
                    }
                    return this.API.Direct.GlobalSettings.GetValueOrDefault(CommonAssumptions.CONFIG_KEY_BACKING_URL, "http://stencil-backing.socialhaven.com");
                });
            }
        }

        public void OnSyncFailed(string type, string argument)
        {
            base.ExecuteMethod("OnSyncFailed", delegate ()
            {
                try
                {
                    ISettingsResolver settingsResolver = this.IFoundation.Resolve<ISettingsResolver>();
                    bool isBackPlane = settingsResolver.IsBackPane();
                    if (isBackPlane)
                    {
                        WebHook.ProcessWebHook(this.IFoundation, "codeable", "failed", type, argument);
                    }
                    else
                    {
                        Task.Run(delegate ()
                        {
                            string url = string.Format("{0}/api/datasync/failed", this.DaemonUrl.Trim('/'));
                            string content = string.Format("key=codeable&type={0}&arg={1}", type, argument);
                            SendPostInNewThread(url, content);
                        });
                    }
                }
                catch (Exception ex)
                {
                    this.IFoundation.LogError(ex, "OnSyncFailed");
                }
            });
        }



        public void SyncEntity(string entityType)
        {
            base.ExecuteMethod("SyncEntity", delegate ()
            {
                try
                {
                    ISettingsResolver settingsResolver = this.IFoundation.Resolve<ISettingsResolver>();
                    bool isBackPlane = settingsResolver.IsBackPane();
                    if (isBackPlane)
                    {
                        WebHook.ProcessWebHook(this.IFoundation, "codeable", "sync", entityType, "");
                    }
                    else
                    {
                        Task.Run(delegate ()
                        {
                            string url = string.Format("{0}/api/datasync/sync", this.DaemonUrl.Trim('/'));
                            string content = string.Format("key=codeable&type={0}", entityType);
                            SendPostInNewThread(url, content);
                        });
                    }
                }
                catch (Exception ex)
                {
                    this.IFoundation.LogError(ex, "SyncEntity");
                }
            });
        }

        public void AgitateSyncDaemon()
        {
            this.AgitateDaemon(string.Format(ElasticSearchDaemon.DAEMON_NAME_FORMAT, Agents.AGENT_DEFAULT));
            this.AgitateDaemon(string.Format(ElasticSearchDaemon.DAEMON_NAME_FORMAT, Agents.AGENT_STATS));
        }
        public void AgitateDaemon(string name)
        {
            base.ExecuteMethod("AgitateDaemon", delegate ()
            {
                try
                {
                    ISettingsResolver settingsResolver = this.IFoundation.Resolve<ISettingsResolver>();
                    bool isBackPlane = settingsResolver.IsBackPane();
                    if (isBackPlane)
                    {
                        WebHook.ProcessAgitateWebHook(this.IFoundation, "codeable", name);
                    }
                    else
                    {
                        Task.Run(delegate ()
                        {
                            string url = string.Format("{0}/api/datasync/agitate", this.DaemonUrl.Trim('/'));
                            string content = "key=codeable&name=" + name;
                            SendPostInNewThread(url, content);
                        });
                    }
                }
                catch (Exception ex)
                {
                    this.IFoundation.LogError(ex, "AgitateDaemon");
                }
            });
        }
        private void SendPostInNewThread(string url, string content)
        {
            Task.Run(delegate ()
            {
                try
                {
                    byte[] requestBytes = new ASCIIEncoding().GetBytes(content);

                    var request = WebRequest.CreateHttp(url);
                    request.Method = "POST";
                    request.ContentType = @"application/x-www-form-urlencoded";
                    request.ContentLength = content.Length;
                    var reqStream = request.GetRequestStream();
                    reqStream.Write(requestBytes, 0, requestBytes.Length);
                    reqStream.Close();
                    request.GetResponse();
                }
                catch
                {
                    // gulp
                }
            });
        }


    }
}
