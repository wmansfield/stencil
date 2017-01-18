using Amazon;
using Amazon.ElasticTranscoder;
using Amazon.ElasticTranscoder.Model;
using Codeable.Foundation.Common;
using Codeable.Foundation.Common.Aspect;
using Codeable.Foundation.Common.Daemons;
using Codeable.Foundation.Core;
using Codeable.Foundation.Core.Caching;
using Codeable.Foundation.Core.Unity;
using Stencil.Common;
using Stencil.Common.Configuration;
using Stencil.Common.Integration;
using Stencil.Domain;
using Stencil.Primary;
using Stencil.Primary.Health;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stencil.Plugins.Amazon.Daemons
{
    public class AmazonEncodingDaemon : ChokeableClass, IDaemonTask
    {
        #region Constructor

        public AmazonEncodingDaemon(IFoundation iFoundation)
            : base(iFoundation)
        {
            this.API = iFoundation.Resolve<StencilAPI>();
            this.Cache = new AspectCache("AmazonEncodingDaemon", iFoundation, new ExpireStaticLifetimeManager("AmazonEncodingDaemon.Life15", System.TimeSpan.FromMinutes(15), false));
        }

        #endregion

        #region Public Properties

        public StencilAPI API { get; set; }
        public AspectCache Cache { get; set; }

        public const string DAEMON_NAME = "AmazonEncodingDaemon";
        public const string OUTPUT_PREFIX = "public/";

        #endregion

        #region Protected Properties

        protected static bool _executing;
        //property for 5 min 
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
        protected string AmazonPipeLineID
        {
            get
            {
                return this.Cache.PerLifetime("AmazonPipeLineID", delegate ()
                {
                    return this.API.Direct.GlobalSettings.GetValueOrDefault(string.Format(CommonAssumptions.CONFIG_KEY_AMAZON_PIPELINE_ID, this.EnvironmentName), string.Empty);
                });
            }
        }
        protected string AmazonPresetID
        {
            get
            {
                return this.Cache.PerLifetime("AmazonPresetID", delegate ()
                {
                    return this.API.Direct.GlobalSettings.GetValueOrDefault(string.Format(CommonAssumptions.CONFIG_KEY_AMAZON_PRESET_ID, this.EnvironmentName), string.Empty);
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
        protected int RetryWindow
        {
            get
            {
                return this.Cache.PerLifetime("RetryWindow", delegate ()
                {
                    int retryWindow;
                    int.TryParse(this.API.Direct.GlobalSettings.GetValueOrDefault(CommonAssumptions.CONFIG_KEY_ENCODE_RETRY_WINDOW_MINUTES, "5"), out retryWindow);
                    return retryWindow;
                });
            }
        }

        #endregion


        #region IDaemonTask Members

        public DaemonSynchronizationPolicy SynchronizationPolicy
        {
            get { return DaemonSynchronizationPolicy.SingleAppDomain; }
        }
        public string DaemonName
        {
            get { return DAEMON_NAME; }
        }
        public void Execute(IFoundation iFoundation)
        {
            if (_executing) { return; } // safety

            base.ExecuteMethod("Execute", delegate ()
            {
                try
                {
                    _executing = true;
                    this.PerformProcessVideos();
                }
                finally
                {
                    _executing = false;
                }
            });
        }

        #endregion

        #region Protected Methods

        protected virtual void PerformProcessVideos()
        {
            base.ExecuteMethod("PerformProcessVideos", delegate ()
            {
                List<Asset> assetsFailed = this.API.Direct.Assets.GetVideosFailedProcessingAfter(MaximumRetries + 1, DateTime.UtcNow.AddMinutes(-1));
                if (assetsFailed.Count > 0)
                {
                    INotifyAdmin notifyAdmin = this.IFoundation.SafeResolve<INotifyAdmin>();
                    if (notifyAdmin != null)
                    {
                        string body = string.Empty;
                        foreach (var item in assetsFailed)
                        {
                            body += string.Format("asset_id: {0}\nraw_url: {1}\nlog:: {2} \n\n--------------------\n\n", item.asset_id, item.raw_url, item.encode_log);
                        }
                        notifyAdmin.SendAdminEmail("Videos Have Failed To Encode", body);
                    }
                }
                List<Asset> assetsToQueue = this.API.Direct.Assets.GetVideosForProcessing(EncoderStatus.not_processed.ToString(), true);

                if (assetsToQueue.Count > 0)
                {
                    using (AmazonElasticTranscoderClient client = new AmazonElasticTranscoderClient(this.AmazonKeyID, this.AmazonSecret, RegionEndpoint.USEast1))
                    {
                        foreach (Asset item in assetsToQueue)
                        {
                            PerformProcessVideo(client, item);
                        }
                    }
                }

                DateTime minimumAttemptTime = DateTime.UtcNow.AddMinutes(-RetryWindow);
                List<Asset> assetsToRetry = this.API.Direct.Assets.GetVideosForRetrying(MaximumRetries + 1, minimumAttemptTime);

                if (assetsToRetry.Count > 0)
                {
                    using (AmazonElasticTranscoderClient client = new AmazonElasticTranscoderClient(this.AmazonKeyID, this.AmazonSecret, RegionEndpoint.USEast1))
                    {
                        foreach (var item in assetsToRetry)
                        {
                            PerformProcessVideo(client, item);
                        }
                    }
                }
            });
        }

        protected virtual void PerformProcessVideo(AmazonElasticTranscoderClient client, Asset asset)
        {
            base.ExecuteMethod("PerformProcessVideo", delegate ()
            {
                try
                {
                    string actualFile = asset.raw_url;

                    // strip bucket
                    int ix = actualFile.ToLower().IndexOf(this.AmazonBucket.ToLower());
                    if (ix > -1)
                    {
                        actualFile = actualFile.Substring(ix + this.AmazonBucket.Length).Trim('/');
                    }

                    // strip cloud front
                    if (!string.IsNullOrEmpty(this.AmazonCloudFrontUrl))
                    {
                        ix = actualFile.ToLower().IndexOf(this.AmazonCloudFrontUrl.ToLower());
                        if (ix > -1)
                        {
                            actualFile = actualFile.Substring(ix + this.AmazonCloudFrontUrl.Length).Trim('/');
                        }
                        ix = asset.raw_url.LastIndexOf('/');
                    }
                    string prefix = asset.raw_url.Substring(0, ix) + "/";

                    CreateJobResponse response = client.CreateJob(new CreateJobRequest()
                    {
                        PipelineId = this.AmazonPipeLineID,
                        OutputKeyPrefix = OUTPUT_PREFIX + asset.asset_id.ToString() + "/",
                        Input = new JobInput()
                        {
                            Key = actualFile,
                            AspectRatio = "auto",
                            Container = "auto",
                            FrameRate = "auto",
                            Interlaced = "auto",
                            Resolution = "auto"
                        },
                        Outputs = new List<CreateJobOutput>()
                        {
                            new CreateJobOutput()
                            {
                                Key = actualFile,
                                PresetId = this.AmazonPresetID,
                                ThumbnailPattern = "thumb_{count}",
                                Rotate = "auto"
                            }
                        }
                    });

                    if (response.Job != null)
                    {
                        HealthReporter.Current.UpdateMetric(HealthTrackType.Each, HealthReporter.VIDEO_TRANSCODE_QUEUE_SUCCESS, 0, 1);
                        this.API.Direct.Assets.UpdateEncodingInfo(asset.asset_id, response.Job.Id, true, EncoderStatus.queued.ToString(), "Amazon Queued on " + DateTime.UtcNow.ToString(), true);
                    }
                    else
                    {
                        HealthReporter.Current.UpdateMetric(HealthTrackType.Each, HealthReporter.VIDEO_TRANSCODE_QUEUE_FAILED, 0, 1);
                        this.API.Direct.Assets.UpdateEncodingInfo(asset.asset_id, string.Empty, false, EncoderStatus.raw.ToString(), "Amazon Queue Failed on " + DateTime.UtcNow.ToString(), true);
                    }
                }
                catch (Exception ex)
                {
                    this.API.Direct.Assets.UpdateEncodingInfo(asset.asset_id, string.Empty, false, EncoderStatus.raw.ToString(), CoreUtility.FormatException(ex), true);
                    HealthReporter.Current.UpdateMetric(HealthTrackType.Each, HealthReporter.VIDEO_TRANSCODE_QUEUE_FAILED, 0, 1);
                    this.IFoundation.LogError(ex, "PerformProcessVideo");
                }
            });
        }

        #endregion
    }
}
