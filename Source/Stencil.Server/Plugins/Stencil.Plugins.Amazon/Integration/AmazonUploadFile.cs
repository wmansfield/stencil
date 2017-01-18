using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Codeable.Foundation.Common;
using Codeable.Foundation.Common.Aspect;
using Codeable.Foundation.Core.Caching;
using Codeable.Foundation.Core.Unity;
using Stencil.Common;
using Stencil.Common.Exceptions;
using Stencil.Common.Integration;
using Stencil.Primary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace Stencil.Plugins.Amazon.Integration
{
    public class AmazonUploadFile : ChokeableClass, IUploadFiles, INotifyEncoder
    {
        #region Constructor

        public AmazonUploadFile(IFoundation iFoundation)
            : base(iFoundation)
        {
            this.API = new StencilAPI(iFoundation);
            this.Cache15 = new AspectCache("AmazonUploadFile", iFoundation, new ExpireStaticLifetimeManager("AmazonUploadFile.Life15", System.TimeSpan.FromMinutes(15), false));
        }

        #endregion

        #region Properties

        public AspectCache Cache15 { get; set; }
        public StencilAPI API { get; set; }

        protected string EnvironmentName
        {
            get
            {
                return this.Cache15.PerLifetime("EnvironmentName", delegate ()
                {
                    return this.API.Integration.SettingsResolver.GetSetting(CommonAssumptions.APP_KEY_ENVIRONMENT);
                });
            }
        }
        protected string AmazonCloudFrontUrl
        {
            get
            {
                return this.Cache15.PerLifetime("AmazonCloudFrontUrl", delegate ()
                {
                    return this.API.Direct.GlobalSettings.GetValueOrDefault(string.Format(CommonAssumptions.CONFIG_KEY_AMAZON_CLOUDFRONT_URL, this.EnvironmentName), string.Empty);
                });
            }
        }
        protected string AmazonPublicUrl
        {
            get
            {
                return this.Cache15.PerLifetime("AmazonPublicUrl", delegate ()
                {
                    return this.API.Direct.GlobalSettings.GetValueOrDefault(string.Format(CommonAssumptions.CONFIG_KEY_AMAZON_PUBLIC_URL, this.EnvironmentName), string.Empty);
                });
            }
        }
        protected string AmazonKeyID
        {
            get
            {
                return this.Cache15.PerLifetime("AmazonKeyID", delegate ()
                {
                    return this.API.Direct.GlobalSettings.GetValueOrDefault(string.Format(CommonAssumptions.CONFIG_KEY_AMAZON_KEY, this.EnvironmentName), string.Empty);
                });
            }
        }
        protected string AmazonSecret
        {
            get
            {
                return this.Cache15.PerLifetime("AmazonSecret", delegate ()
                {
                    return this.API.Direct.GlobalSettings.GetValueOrDefault(string.Format(CommonAssumptions.CONFIG_KEY_AMAZON_SECRET, this.EnvironmentName), string.Empty);
                });
            }
        }
        protected string AmazonBucket
        {
            get
            {
                return this.Cache15.PerLifetime("AmazonBucket", delegate ()
                {
                    return this.API.Direct.GlobalSettings.GetValueOrDefault(string.Format(CommonAssumptions.CONFIG_KEY_AMAZON_BUCKET, this.EnvironmentName), string.Empty);
                });
            }
        }
        protected string DaemonUrl
        {
            get
            {
                return this.Cache15.PerLifetime("DaemonUrl", delegate ()
                {
                    return this.API.Direct.GlobalSettings.GetValueOrDefault(CommonAssumptions.CONFIG_KEY_BACKING_URL, "https://stencil-backing.socialhaven.com");
                });
            }
        }

        #endregion

        #region Public Methods

        public string ConstructUploadUrl(string filePathAndName)
        {
            return base.ExecuteFunction("ConstructUploadUrl", delegate ()
            {
                return AmazonUtility.ConstructAmazonUrl(this.AmazonCloudFrontUrl, this.AmazonPublicUrl, this.AmazonBucket, filePathAndName);
            });
        }
        public string GeneratePreSignedUploadUrl(string verb, string filePathAndName, string contentType = "multipart/form-data")
        {
            return base.ExecuteFunction("GeneratePreSignedUploadUrl", delegate ()
            {
                HttpVerb httpVerb = HttpVerb.PUT;
                Enum.TryParse(verb, true, out httpVerb);
                GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
                {
                    BucketName = this.AmazonBucket,
                    Key = filePathAndName,
                    Verb = httpVerb,
                    Expires = DateTime.UtcNow.AddMinutes(15),
                    ContentType = contentType,
                };

                using (var client = new AmazonS3Client(new BasicAWSCredentials(this.AmazonKeyID, this.AmazonSecret), RegionEndpoint.USEast1))
                {
                    return client.GetPreSignedURL(request);
                }
            });
        }
        public TemporaryUploadCredentials GenerateTemporaryUploadCredentials()
        {
            return base.ExecuteFunction("GenerateTemporaryUploadCredentials", delegate ()
            {
                Credentials credentials = this.Cache15.PerLifetime("GetSessionToken", delegate ()
                {
                    // Cache is every 15 minutes, we set the expire every 30, we're good. :)

                    using (var client = new AmazonSecurityTokenServiceClient(this.AmazonKeyID, this.AmazonSecret, RegionEndpoint.USEast1))
                    {
                        GetSessionTokenResponse response = client.GetSessionToken(new GetSessionTokenRequest
                        {
                            DurationSeconds = (int)TimeSpan.FromMinutes(30).TotalSeconds,
                        });

                        if (response == null || response.HttpStatusCode != System.Net.HttpStatusCode.OK || response.Credentials == null)
                        {
                            throw new UIException("Unable to generate File Upload Credentials. Please try again in a few moments");
                        }
                        return response.Credentials;
                    }
                });

                return new TemporaryUploadCredentials()
                {
                    bucket = this.AmazonBucket,
                    access_key_id = credentials.AccessKeyId,
                    secret_access_key = credentials.SecretAccessKey,
                    session_token = credentials.SessionToken,
                };
            });
        }

        public string UploadPhoto(Image image, ImageEncoding encoding, Size imageTargetSize, string filePathAndName)
        {
            return base.ExecuteFunction("UploadPhoto", delegate ()
            {
                filePathAndName = filePathAndName.Replace(" ", "%20"); //TODO:COULD: Sanitize this a littler better
                Image resizedImage = ImageFormatter.Resize(image, ResizeMode.Fill, (AnchorStyles.Middle | AnchorStyles.Center), imageTargetSize.Width, imageTargetSize.Height);

                using (AmazonS3Client client = new AmazonS3Client(this.AmazonKeyID, this.AmazonSecret, RegionEndpoint.USEast1))
                {
                    PutObjectResponse result = null;

                    ImageCodecInfo codecInfo = null;
                    EncoderParameters encoderParameters = null;
                    switch (encoding)
                    {
                        case ImageEncoding.PNG:
                            codecInfo = GetCodecInfo("image/png");
                            encoderParameters = new EncoderParameters(0);
                            if (!filePathAndName.ToLower().EndsWith("png"))
                            {
                                filePathAndName += ".png";
                            }
                            break;
                        case ImageEncoding.JPEG:
                        default:
                            encoderParameters = new EncoderParameters(1);
                            encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 90L);
                            codecInfo = GetCodecInfo("image/jpeg");
                            if (!filePathAndName.ToLower().EndsWith("jpg"))
                            {
                                filePathAndName += ".jpg";
                            }
                            break;
                    }

                    using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
                    {
                        resizedImage.Save(memoryStream, codecInfo, encoderParameters);
                        memoryStream.Position = 0;

                        PutObjectRequest request = new PutObjectRequest()
                        {
                            BucketName = this.AmazonBucket,
                            Key = filePathAndName,
                            CannedACL = S3CannedACL.PublicRead,
                            InputStream = memoryStream
                        };
                        result = client.PutObject(request);
                    }

                    if (result == null || (result.HttpStatusCode != HttpStatusCode.OK && result.HttpStatusCode != HttpStatusCode.Created))
                    {
                        throw new UIException("Error saving file to cloud storage. Please try again in a few moments.");
                    }
                    else
                    {
                        return AmazonUtility.ConstructAmazonUrl(this.AmazonCloudFrontUrl, this.AmazonPublicUrl, this.AmazonBucket, filePathAndName);
                    }
                }
            });

        }

        public void OnVideoAdded()
        {
            base.ExecuteMethod("OnVideoAdded", delegate ()
            {
                try
                {
                    //  never used directly from the backplane, so it'll just call it remotely
                    Task.Run(delegate ()
                    {
                        string url = this.DaemonUrl.Trim('/') + "/api/amazon/agitate?key=codeable&type=video";
                        WebRequest.Create(url).GetResponse();
                    });
                }
                catch (Exception ex)
                {
                    this.IFoundation.LogError(ex, "OnVideoAdded");
                }
            });
        }

        public void OnPhotoAdded()
        {
            base.ExecuteMethod("OnPhotoAdded", delegate ()
            {
                try
                {
                    //  never used directly from the backplane, so it'll just call it remotely
                    Task.Run(delegate ()
                    {
                        string url = this.DaemonUrl.Trim('/') + "/api/amazon/agitate?key=codeable&type=photo";
                        var request = WebRequest.CreateHttp(url);
                        request.Method = "POST";
                        request.ContentLength = 0;
                        request.GetResponse();
                    });
                }
                catch (Exception ex)
                {
                    this.IFoundation.LogError(ex, "OnPhotoAdded");
                }
            });
        }

        #endregion

        #region Protected Methods

        protected virtual ImageCodecInfo GetCodecInfo(string mimeType)
        {
            return base.ExecuteFunction<ImageCodecInfo>("GetCodecInfo", delegate ()
            {
                ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
                return encoders.FirstOrDefault(x => x.MimeType.Equals(mimeType, StringComparison.OrdinalIgnoreCase));
            });

        }

        #endregion
    }
}