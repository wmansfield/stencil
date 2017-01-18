using Codeable.Foundation.Common;
using Codeable.Foundation.Common.Daemons;
using Codeable.Foundation.Core.Caching;
using Codeable.Foundation.Core.Unity;
using Stencil.Common;
using Stencil.Common.Configuration;
using Stencil.Common.Integration;
using Stencil.Domain;
using Stencil.Plugins.Amazon.Daemons;
using Stencil.Plugins.Amazon.Integration;
using Stencil.Plugins.Amazon.Models.Amazon;
using Stencil.Primary;
using Stencil.Primary.Health;
using Stencil.Web.Controllers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Stencil.Plugins.Amazon.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("api/amazon")]
    public class AmazonWebHookController : RestApiBaseController
    {
        public AmazonWebHookController(IFoundation iFoundation)
            : base(iFoundation)
        {
            this.Cache = new AspectCache("AmazonWebHookController", iFoundation, new ExpireStaticLifetimeManager("AmazonWebHookController.Life15", System.TimeSpan.FromMinutes(15), false));
        }

        public AspectCache Cache { get; set; }

        protected string EnvironmentName
        {
            get
            {
                return this.Cache.PerLifetime("EnvironmentName", delegate ()
                {
                    return this.IFoundation.Resolve<ISettingsResolver>().GetSetting(CommonAssumptions.APP_KEY_ENVIRONMENT);
                });
            }
        }
        protected string AmazonCloudFrontUrl
        {
            get
            {
                return this.Cache.PerLifetime("AmazonCloudFrontUrl", delegate ()
                {
                    return this.API.Direct.GlobalSettings.GetValueOrDefault(string.Format(CommonAssumptions.CONFIG_KEY_AMAZON_CLOUDFRONT_URL, this.EnvironmentName), string.Empty);
                });
            }
        }
        protected string AmazonPublicUrl
        {
            get
            {
                return this.Cache.PerLifetime("AmazonPublicUrl", delegate ()
                {
                    return this.API.Direct.GlobalSettings.GetValueOrDefault(string.Format(CommonAssumptions.CONFIG_KEY_AMAZON_PUBLIC_URL, this.EnvironmentName), string.Empty);
                });
            }
        }
        protected string AmazonKeyID
        {
            get
            {
                return this.Cache.PerLifetime("AmazonKeyID", delegate ()
                {
                    return this.API.Direct.GlobalSettings.GetValueOrDefault(string.Format(CommonAssumptions.CONFIG_KEY_AMAZON_KEY, this.EnvironmentName), string.Empty);
                });
            }
        }
        protected string AmazonSecret
        {
            get
            {
                return this.Cache.PerLifetime("AmazonSecret", delegate ()
                {
                    return this.API.Direct.GlobalSettings.GetValueOrDefault(string.Format(CommonAssumptions.CONFIG_KEY_AMAZON_SECRET, this.EnvironmentName), string.Empty);
                });
            }
        }
        protected string AmazonBucket
        {
            get
            {
                return this.Cache.PerLifetime("AmazonBucket", delegate ()
                {
                    return this.API.Direct.GlobalSettings.GetValueOrDefault(string.Format(CommonAssumptions.CONFIG_KEY_AMAZON_BUCKET, this.EnvironmentName), string.Empty);
                });
            }
        }
        protected int MaximumRetries
        {
            get
            {
                return this.Cache.PerLifetime("MaximumRetries", delegate ()
                {
                    int maxRetries;
                    int.TryParse(this.API.Direct.GlobalSettings.GetValueOrDefault(CommonAssumptions.CONFIG_KEY_AMAZON_ENCODE_RETRY_MAX, "5"), out maxRetries);
                    return maxRetries;
                });
            }
        }

        [HttpPost]
        [Route("webhook")]
        public object AmazonWebHook(AmazonNotification notification)
        {
            return base.ExecuteFunction("AmazonWebHook", delegate ()
            {
                if (notification != null)
                {
                    if (notification.Type == "SubscriptionConfirmation")
                    {
                        if (!string.IsNullOrEmpty(notification.SubscribeURL))
                        {
                            WebRequest.Create(notification.SubscribeURL).GetResponse();
                        }
                    }
                    if (notification.Type == "Notification")
                    {
                        if (!string.IsNullOrEmpty(notification.Message))
                        {
                            AmazonJobInfo jobInfo = JsonConvert.DeserializeObject<AmazonJobInfo>(notification.Message);
                            if (jobInfo != null)
                            {
                                Guid asset_id = Guid.Empty;
                                Asset asset = null;
                                if (Guid.TryParse(jobInfo.outputKeyPrefix.Replace(AmazonEncodingDaemon.OUTPUT_PREFIX, "").Trim('/'), out asset_id))
                                {
                                    switch (jobInfo.state)
                                    {
                                        case "ERROR":
                                            asset = this.API.Direct.Assets.UpdateEncodingInfo(asset_id, jobInfo.jobId, false, EncoderStatus.fail.ToString(), "Amazon Error: " + jobInfo.messageDetails, false);
                                            if (asset != null && asset.encode_attempts >= this.MaximumRetries)
                                            {
                                                this.API.Integration.Email.SendAdminEmail("Video Encoder", string.Format("The following video has failed to encode after {0} attempts.<br/><br/>asset_id: {1}<br/>raw_url: {2}<br/>log: {3}", asset.encode_attempts, asset.asset_id, asset.raw_url, asset.encode_log));
                                            }
                                            break;
                                        case "WARNING":
                                            this.API.Direct.Assets.UpdateEncodingInfo(asset_id, jobInfo.jobId, true, EncoderStatus.processing.ToString(), "Amazon Warning: " + jobInfo.messageDetails, false);
                                            break;
                                        case "COMPLETED":
                                            asset = this.API.Direct.Assets.GetById(asset_id);
                                            if (asset != null)
                                            {
                                                HealthReporter.Current.UpdateMetric(HealthTrackType.Each, HealthReporter.VIDEO_TRANSCODE_COMPLETE_SUCCESS, 0, 1);
                                                string mediaFile = string.Format("{0}/{1}", jobInfo.outputKeyPrefix.Trim('/'), jobInfo.outputs[0].key.TrimStart('/'));
                                                string thumbFile = string.Format("{0}/{1}", jobInfo.outputKeyPrefix.Trim('/'), jobInfo.outputs[0].thumbnailPattern.TrimStart('/').Replace("{count}", "00001") + ".png");
                                                asset.raw_url = AmazonUtility.ConstructAmazonUrl(string.Empty, this.AmazonPublicUrl, this.AmazonBucket, mediaFile);
                                                asset.public_url = AmazonUtility.ConstructAmazonUrl(this.AmazonCloudFrontUrl, this.AmazonPublicUrl, this.AmazonBucket, mediaFile);
                                                asset.thumb_large_url = AmazonUtility.ConstructAmazonUrl(this.AmazonCloudFrontUrl, this.AmazonPublicUrl, this.AmazonBucket, thumbFile);
                                                asset.thumb_medium_url = asset.thumb_large_url;
                                                asset.thumb_small_url = asset.thumb_large_url;
                                                asset.encode_required = false;
                                                asset.encode_processing = false;
                                                asset.available = true;
                                                asset.encode_status = EncoderStatus.complete.ToString();
                                                asset.encode_log += "Amazon Completed Processing on " + DateTime.UtcNow.ToString() + "|" + notification.Message;
                                                asset.resize_required = true;
                                                asset.resize_mode = "fitnopadding";
                                                this.API.Direct.Assets.Update(asset);
                                                this.API.Integration.Synchronization.AgitateDaemon(AmazonImageResizeDaemon.DAEMON_NAME);
                                            }
                                            else
                                            {
                                                HealthReporter.Current.UpdateMetric(HealthTrackType.Each, HealthReporter.VIDEO_TRANSCODE_COMPLETE_FAILED, 0, 1);
                                            }
                                            break;
                                        case "PROGRESSING":
                                            this.API.Direct.Assets.UpdateEncodingInfo(asset_id, jobInfo.jobId, true, EncoderStatus.processing.ToString(), "Amazon Started Processing on " + DateTime.UtcNow.ToString(), false);
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
                return this.Http200("OK", "");
            });
        }

        [HttpPost]
        [Route("agitate")]
        public object AgitateDaemons(string key, string type)
        {
            return base.ExecuteFunction("AgitateDaemons", delegate ()
            {
                if (key == "codeable")
                {
                    IDaemonManager daemonManager = this.IFoundation.GetDaemonManager();
                    if (type == "photo")
                    {
                        daemonManager.StartDaemon(AmazonImageResizeDaemon.DAEMON_NAME);
                    }
                    if (type == "video")
                    {
                        daemonManager.StartDaemon(AmazonEncodingDaemon.DAEMON_NAME);
                    }
                }
                return this.Http200("OK", "");
            });
        }

    }
}