using System;
using System.Threading.Tasks;
using UIKit;
using System.IO;
using Foundation;
using System.Net;
using System.Text;
using System.Collections.Generic;
using Stencil.Native;
using Stencil.Native.Services.MediaUploader;
using Stencil.Native.Core;

namespace Stencil.Native.iOS.Core.Services
{
    public class IOSMediaUploader : BaseMediaUploader
    {
        public IOSMediaUploader()
            : base("IOSMediaUploader")
        {
        }

        public string AmazonSecret { get; set; }
        public string AmazonKey { get; set; }

        private static int _fileNameHelper = 0;

        //Debug:Time Delay Highlight
        #if DEBUG
        private static bool _delayed = false;
        #endif

        public static string GenerateTempUploadFileName()
        {
            _fileNameHelper++;
            if(_fileNameHelper > 10)
            {
                _fileNameHelper = 0;
            }
            string directory = Path.Combine(System.IO.Path.GetTempPath(), "StencilNative", "tmpupload");
            Container.FileStore.EnsureFolderExists(directory);
            return Path.Combine(directory, string.Format("uploadtemp{0}.mp4", _fileNameHelper));
        }

        public override void EnqueueRequest(UploadRequest request)
        {
            base.EnqueueRequest(request);
        }


        protected override System.Threading.Tasks.Task<AmazonUploadInfo> PerformUploadVideoToAmazon(UploadRequest request)
        {
            return base.ExecuteFunctionAsync("PerformUploadVideoToAmazon", async delegate()
            {
                if(request.UploadInfo != null && request.UploadInfo.Success)
                {
                    return request.UploadInfo;
                }

                string path = request.NativePath;

                if(!request.NativePath.Contains("uploadtemp"))
                {
                    // copy local now
                    path = GenerateTempUploadFileName();
                    using (var inputStream = Container.FileStore.OpenRead(request.NativePath))
                    {
                        using(var fileStream = Container.FileStore.OpenWrite(path))
                        {
                            CoreUtility.CopyStream(inputStream, fileStream);
                        }
                    }
                }

                bool uploaded = await this.UploadDirect(path, request.FileName, request.OnUploadProgressChanged);

                if(!uploaded)
                {
                    return new AmazonUploadInfo()
                    {
                        Success = false
                    };
                }
                request.UploadInfo = new AmazonUploadInfo()
                {
                    Success = true,
                    AmazonKey = request.FileName
                };
                return request.UploadInfo;
            });
        }
        protected override Task<AmazonUploadInfo> PerformUploadPhotoToAmazon(UploadRequest request)
        {
            return base.ExecuteFunctionAsync("PerformUploadPhotoToAmazon", async delegate()
            {
                if(request.UploadInfo != null && request.UploadInfo.Success)
                {
                    return request.UploadInfo;
                }

                UIImage image = request.NativeData as UIImage;
                if(image == null)
                {
                    return new AmazonUploadInfo()
                    {
                        Success = false
                    };
                }
                string directory = Path.Combine(System.IO.Path.GetTempPath(), "StencilNative", "tmpupload");
                Container.FileStore.EnsureFolderExists(directory);
                string path = Path.Combine(directory, "uploadtemp.jpg");
                NSData data = image.AsJPEG(0.9f);

                using (var inputStream = data.AsStream())
                {
                    using(var fileStream = Container.FileStore.OpenWrite(path))
                    {
                        CoreUtility.CopyStream(inputStream, fileStream);
                    }
                }

                bool uploaded = await this.UploadDirect(path, request.FileName, request.OnUploadProgressChanged);

                if(!uploaded)
                {
                    return new AmazonUploadInfo()
                    {
                        Success = false
                    };
                }
                request.UploadInfo = new AmazonUploadInfo()
                {
                    Success = true,
                    AmazonKey = request.FileName
                };
                return request.UploadInfo;
            });
        }
        protected virtual Task<bool> UploadDirect(string localFilePath, string presignedUrl, EventHandler<UploadProgressArgs> onProgressChanged)
        {
            return base.ExecuteFunctionAsync("UploadDirect", async delegate()
            {
                // can't use webclient, has async callback issue on android

                using (FileStream fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read))
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(presignedUrl);
                    request.Method = "PUT";
                    request.ContentType = "application/octet-stream";
                    request.Timeout = -1; //Infinite wait for the response.
                    request.AllowWriteStreamBuffering = false;
                    request.ContentLength = fileStream.Length;
                    // Create 32KB buffer which is file page size.
                    byte[] tempBuffer = new byte[1024 * 64];
                    int bytesRead = 0;
                    int totalBytesRead = 0;

                    long percentage = 0;

                    // Write the source data to the network stream.
                    Stream requestStream = request.GetRequestStream();
                    // Loop till the file content is read completely.
                    while ((bytesRead = fileStream.Read(tempBuffer, 0, tempBuffer.Length)) > 0)
                    {
                        totalBytesRead += bytesRead;
                        // Write the 8 KB data in the buffer to the network stream.
                        requestStream.Write(tempBuffer, 0, bytesRead);

                        // Update your progress bar here using segment count.
                        if(onProgressChanged != null)
                        {
                            long newPercentage = (int)(100 * ((double)totalBytesRead  / (double)fileStream.Length));
                            if(newPercentage != percentage)
                            {
                                percentage = newPercentage;
                                onProgressChanged(this, new UploadProgressArgs(0, totalBytesRead, fileStream.Length));
                            }
                        }

                        #if DEBUG
                        if(_delayed)
                        {
                        System.Threading.Thread.Sleep(700);
                        }
                        #endif
                    }
                    requestStream.Close();

                    WebResponse response = request.GetResponse();

                    base.LogWarning(response.GetResponseStream().Length.ToString());
                    return true;
                }
            });
        }
        protected override Task<bool> PerformAddUploadedVideoToApp(UploadRequest request, AmazonUploadInfo info)
        {
            // we dont keep assets isolated, they are associated in a single call
            return Task.FromResult(true);
        }
        protected override Task<bool> PerformAddUploadedPhotoToApp(UploadRequest request, AmazonUploadInfo info)
        {
            // we dont keep assets isolated, they are associated in a single call
            return Task.FromResult(true);
        }

        protected override void InvalidateCacheForPrefix(string cachePrefix)
        {
            // nothing yet
        }


    }
}

