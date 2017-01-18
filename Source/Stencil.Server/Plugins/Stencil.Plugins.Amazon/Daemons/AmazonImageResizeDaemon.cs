using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
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
using Stencil.Plugins.Amazon.Externals;
using Stencil.Plugins.Amazon.Integration;
using Stencil.Primary;
using Stencil.Primary.Health;
using Stencil.Primary.Integration;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace Stencil.Plugins.Amazon.Daemons
{
    public class AmazonImageResizeDaemon : ChokeableClass, IDaemonTask, IProcessImage
    {
        #region Constructor

        public AmazonImageResizeDaemon(IFoundation iFoundation)
            : base(iFoundation)
        {
            this.API = iFoundation.Resolve<StencilAPI>();
            this.Cache = new AspectCache("AmazonImageResizeDaemon", iFoundation, new ExpireStaticLifetimeManager("AmazonImageResizeDaemon.Life15", System.TimeSpan.FromMinutes(15), false));
        }

        #endregion

        #region Public Properties
        public StencilAPI API { get; set; }

        public AspectCache Cache { get; set; }

        public const string DAEMON_NAME = "AmazonImageResizeDaemon";

        #endregion

        #region Protected Properties

        protected static bool _executing;
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
        protected int ResizeAttemptLimit
        {
            get
            {
                return this.Cache.PerLifetime("ResizeAttemptLimit", delegate ()
                {
                    return int.Parse(this.API.Direct.GlobalSettings.GetValueOrDefault(CommonAssumptions.CONFIG_KEY_RESIZE_ATTEMPT_LIMIT, "5"));
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
                    this.PerformProcessPhotos();
                }
                finally
                {
                    _executing = false;
                }
            });
        }

        #endregion

        #region Protected Methods

        protected virtual void PerformProcessPhotos()
        {
            base.ExecuteMethod("PerformProcessPhotos", delegate ()
            {
                List<Asset> assetsToProcess = this.API.Direct.Assets.GetPhotosForProcessing(EncoderStatus.not_processed.ToString(), true, this.ResizeAttemptLimit);

                foreach (var item in assetsToProcess)
                {
                    ProcessPhoto(item);
                }

            });
        }
        protected void FlipImageIfNeeded(Image source, Image destination)
        {
            base.ExecuteMethod("FlipImageIfNeeded", delegate ()
            {
                // Rotate the image according to EXIF data
                EXIFextractor exif = new EXIFextractor(ref source, "n");

                if (exif["Orientation"] != null)
                {
                    RotateFlipType flip = this.ExifOrientationToFlipType(exif["Orientation"].ToString());

                    if (flip != RotateFlipType.RotateNoneFlipNone) // don't flip of orientation is correct
                    {
                        destination.RotateFlip(flip);
                    }
                }
            });
        }

        protected virtual RotateFlipType ExifOrientationToFlipType(string orientation)
        {
            switch (int.Parse(orientation))
            {
                case 1:
                    return RotateFlipType.RotateNoneFlipNone;
                case 2:
                    return RotateFlipType.RotateNoneFlipX;
                case 3:
                    return RotateFlipType.Rotate180FlipNone;
                case 4:
                    return RotateFlipType.Rotate180FlipX;
                case 5:
                    return RotateFlipType.Rotate90FlipX;
                case 6:
                    return RotateFlipType.Rotate90FlipNone;
                case 7:
                    return RotateFlipType.Rotate270FlipX;
                case 8:
                    return RotateFlipType.Rotate270FlipNone;
                default:
                    return RotateFlipType.RotateNoneFlipNone;
            }
        }

        protected Image GetImageFromUrl(string imageUrl)
        {
            return base.ExecuteFunction("GetImageFromUrl", delegate ()
            {
                Image result = null;
                string[] webPrefixList = new string[] { "http", "ftp" };
                foreach (string webPrefix in webPrefixList)
                {
                    if (imageUrl.ToLower().StartsWith(webPrefix))
                    {
                        WebClient wClient = new WebClient();
                        using (System.IO.MemoryStream memStream = new System.IO.MemoryStream(wClient.DownloadData(imageUrl)))
                        {
                            result = System.Drawing.Bitmap.FromStream(memStream);
                        }
                        break;
                    }
                }
                if (result == null)
                {
                    // even possible?
                    result = System.Drawing.Image.FromFile(imageUrl);
                }
                return result;
            });
        }
        /// <summary>
        /// Returns empty string on success
        /// </summary>
        public virtual Asset ProcessPhoto(Asset asset)
        {
            return base.ExecuteFunction("ProcessPhoto", delegate ()
            {
                try
                {
                    // get recent, just in case
                    asset = this.API.Direct.Assets.GetById(asset.asset_id);
                    if (asset == null || !asset.resize_required)
                    {
                        return null; // short circuit
                    }
                    asset.resize_attempts++;
                    asset.resize_attempt_utc = DateTime.UtcNow;
                    this.API.Direct.Assets.UpdateResizeAttemptInfo(asset.asset_id, asset.resize_attempts, asset.resize_attempt_utc.GetValueOrDefault(), "Attempting to resize");
                    System.Drawing.Image original = null;
                    try
                    {
                        string raw_url = asset.raw_url;
                        if (asset.type == AssetType.Video)
                        {
                            raw_url = asset.thumb_large_url;
                        }
                        original = this.GetImageFromUrl(raw_url);

                        if (original == null)
                        {
                            return null;
                        }

                        int ix = raw_url.LastIndexOf('/');

                        string prefix = asset.raw_url.Substring(0, ix).Trim('/') + "/";
                        ix = prefix.ToLower().IndexOf(this.AmazonBucket.ToLower());
                        if (ix > -1)
                        {
                            prefix = prefix.Substring(ix + this.AmazonBucket.Length);
                        }
                        ix = prefix.ToLower().IndexOf(this.AmazonCloudFrontUrl.ToLower());
                        if (ix > -1)
                        {
                            prefix = prefix.Substring(ix + this.AmazonCloudFrontUrl.Length);
                        }
                        prefix = prefix.TrimStart('/');

                        string extension = Path.GetExtension(asset.raw_url);
                        string imageCodecName = "jpeg";
                        EncoderParameters encoderParameters = new EncoderParameters(1);
                        encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 90L);

                        PixelFormat format = PixelFormat.Format24bppRgb;
                        ResizeMode resizeMode = ResizeMode.Fill;
                        if (!string.IsNullOrEmpty(asset.resize_mode))
                        {
                            if (!Enum.TryParse<ResizeMode>(asset.resize_mode, true, out resizeMode))
                            {
                                resizeMode = ResizeMode.Fill; // fallback
                            }
                        }

                        if (resizeMode == ResizeMode.Fit) // we never want a backcolor
                        {
                            imageCodecName = "png";
                            extension = ".png";
                            format = PixelFormat.Format32bppArgb;
                        }

                        string cacheBuster = string.Format("{0}{1:HHmmss}", DateTime.UtcNow.DayOfYear, DateTime.UtcNow);
                        string destinationSmall = prefix + cacheBuster + Path.GetFileNameWithoutExtension(asset.raw_url) + "_sm" + extension;
                        string destinationMedium = prefix + cacheBuster + Path.GetFileNameWithoutExtension(asset.raw_url) + "_md" + extension;
                        string destinationLarge = prefix + cacheBuster + Path.GetFileNameWithoutExtension(asset.raw_url) + "_lg" + extension;

                        // create thumb
                        Image smallImage = null;
                        Image mediumImage = null;
                        Image largeImage = null;

                        try
                        {
                            Size dimensions = new Size();
                            if (TryParseDimensions(asset.thumb_small_dimensions, out dimensions))
                            {
                                smallImage = ImageFormatter.Resize(original, resizeMode, (AnchorStyles.Middle | AnchorStyles.Center), dimensions.Width, dimensions.Height, format, InterpolationMode.HighQualityBicubic, Color.Transparent);
                            }
                            if (TryParseDimensions(asset.thumb_medium_dimensions, out dimensions))
                            {
                                mediumImage = ImageFormatter.Resize(original, resizeMode, (AnchorStyles.Middle | AnchorStyles.Center), dimensions.Width, dimensions.Height, format, InterpolationMode.HighQualityBicubic, Color.Transparent);
                            }
                            if (TryParseDimensions(asset.thumb_large_dimensions, out dimensions))
                            {
                                largeImage = ImageFormatter.Resize(original, resizeMode, (AnchorStyles.Middle | AnchorStyles.Center), dimensions.Width, dimensions.Height, format, InterpolationMode.HighQualityBicubic, Color.Transparent);
                            }

                            using (AmazonS3Client client = new AmazonS3Client(this.AmazonKeyID, this.AmazonSecret, RegionEndpoint.USEast1))
                            {
                                // save small
                                if (smallImage != null)
                                {
                                    this.FlipImageIfNeeded(original, smallImage);

                                    using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
                                    {
                                        smallImage.Save(memoryStream, GetCodecInfo(imageCodecName), encoderParameters);
                                        memoryStream.Position = 0;

                                        PutObjectRequest request = new PutObjectRequest()
                                        {
                                            BucketName = this.AmazonBucket,
                                            Key = destinationSmall,
                                            CannedACL = S3CannedACL.PublicRead,
                                            InputStream = memoryStream
                                        };
                                        PutObjectResponse result = client.PutObject(request);

                                        if (result == null || (result.HttpStatusCode != HttpStatusCode.OK && result.HttpStatusCode != HttpStatusCode.Created))
                                        {
                                            throw new Exception("Error saving to amazon");
                                        }
                                    }
                                }
                                // save medium
                                if (mediumImage != null)
                                {
                                    this.FlipImageIfNeeded(original, mediumImage);

                                    using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
                                    {
                                        mediumImage.Save(memoryStream, GetCodecInfo(imageCodecName), encoderParameters);
                                        memoryStream.Position = 0;

                                        PutObjectRequest request = new PutObjectRequest()
                                        {
                                            BucketName = this.AmazonBucket,
                                            Key = destinationMedium,
                                            CannedACL = S3CannedACL.PublicRead,
                                            InputStream = memoryStream
                                        };
                                        PutObjectResponse result = client.PutObject(request);
                                        if (result == null || (result.HttpStatusCode != HttpStatusCode.OK && result.HttpStatusCode != HttpStatusCode.Created))
                                        {
                                            throw new Exception("Error saving to amazon");
                                        }
                                    }
                                }

                                // save large
                                if (largeImage != null)
                                {
                                    this.FlipImageIfNeeded(original, largeImage);

                                    using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
                                    {
                                        largeImage.Save(memoryStream, GetCodecInfo(imageCodecName), encoderParameters);
                                        memoryStream.Position = 0;

                                        PutObjectRequest request = new PutObjectRequest()
                                        {
                                            BucketName = this.AmazonBucket,
                                            Key = destinationLarge,
                                            CannedACL = S3CannedACL.PublicRead,
                                            InputStream = memoryStream
                                        };
                                        PutObjectResponse result = client.PutObject(request);
                                        if (result == null || (result.HttpStatusCode != HttpStatusCode.OK && result.HttpStatusCode != HttpStatusCode.Created))
                                        {
                                            throw new Exception("Error saving to amazon");
                                        }
                                    }
                                }

                                asset = this.API.Direct.Assets.GetById(asset.asset_id); // get recent
                                if (asset != null)
                                {
                                    if (smallImage != null)
                                    {
                                        asset.thumb_small_url = AmazonUtility.ConstructAmazonUrl(this.AmazonCloudFrontUrl, this.AmazonPublicUrl, this.AmazonBucket, destinationSmall);
                                        asset.thumb_small_dimensions = string.Format("{0}x{1}", smallImage.Width, smallImage.Height);
                                    }
                                    if (mediumImage != null)
                                    {
                                        asset.thumb_medium_url = AmazonUtility.ConstructAmazonUrl(this.AmazonCloudFrontUrl, this.AmazonPublicUrl, this.AmazonBucket, destinationMedium);
                                        asset.thumb_medium_dimensions = string.Format("{0}x{1}", mediumImage.Width, mediumImage.Height);
                                    }
                                    if (largeImage != null)
                                    {
                                        asset.thumb_large_url = AmazonUtility.ConstructAmazonUrl(this.AmazonCloudFrontUrl, this.AmazonPublicUrl, this.AmazonBucket, destinationLarge);
                                        asset.thumb_large_dimensions = string.Format("{0}x{1}", largeImage.Width, largeImage.Height);
                                    }
                                    asset.available = true;
                                    asset.resize_processing = false;
                                    asset.resize_required = false;
                                    asset.resize_status = EncoderStatus.complete.ToString();
                                    asset.resize_log += "Resize Completed Processing on " + DateTime.UtcNow.ToString();
                                    this.API.Direct.Assets.Update(asset);

                                    HealthReporter.Current.UpdateMetric(HealthTrackType.Each, HealthReporter.PHOTO_RESIZE_SUCCESS, 0, 1);
                                }
                                else
                                {
                                    HealthReporter.Current.UpdateMetric(HealthTrackType.Each, HealthReporter.PHOTO_RESIZE_FAILED, 0, 1);
                                }
                                return asset;
                            }
                        }
                        finally
                        {
                            if (smallImage != null) { smallImage.Dispose(); }
                            if (mediumImage != null) { mediumImage.Dispose(); }
                            if (largeImage != null) { largeImage.Dispose(); }
                        }
                    }
                    finally
                    {
                        if (original != null) { original.Dispose(); }
                    }
                }
                catch (Exception ex)
                {
                    this.API.Direct.Assets.UpdateResizeInfo(asset.asset_id, false, EncoderStatus.not_processed.ToString(), CoreUtility.FormatException(ex));
                    HealthReporter.Current.UpdateMetric(HealthTrackType.Each, HealthReporter.PHOTO_RESIZE_FAILED, 0, 1);
                    this.IFoundation.LogError(ex, "PerformProcessPhoto");
                    return null;
                }
            });
        }


        protected virtual ImageCodecInfo GetCodecInfo(string mimeType)
        {
            return base.ExecuteFunction<ImageCodecInfo>("GetCodecInfo", delegate ()
            {
                ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
                return encoders.FirstOrDefault(x => x.MimeType.ToLower().Contains(mimeType.ToLower()));
            });

        }

        protected bool TryParseDimensions(string dimensions, out Size size)
        {
            size = new Size();
            try
            {
                if (!string.IsNullOrEmpty(dimensions) && dimensions.Contains("x"))
                {
                    string[] split = dimensions.Split('x');
                    if (split != null && split.Length == 2)
                    {
                        size = new Size(int.Parse(split[0]), int.Parse(split[1]));
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        #endregion
    }
}
