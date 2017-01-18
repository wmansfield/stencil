using System;
using System.IO;
using System.Threading.Tasks;
using Android.Graphics;
using System.Net;
using System.Text;
using System.Threading;
using Stencil.Native.Services.MediaUploader;
using Stencil.Native.Core;

namespace Stencil.Native.Droid.Core.Services
{
    public class DroidMediaUploader : BaseMediaUploader
    {
        public DroidMediaUploader()
            : base("DroidMediaUploader")
        {
        }

        public string AmazonSecret { get; set; }
        public string AmazonKey { get; set; }

        //Debug:Time Delay Highlight
        #if DEBUG
        private static bool _delayed = false;
        #endif

        protected override System.Threading.Tasks.Task<AmazonUploadInfo> PerformUploadVideoToAmazon(UploadRequest request)
        {
            return base.ExecuteFunctionAsync("PerformUploadVideoToAmazon", async delegate()
            {
                if(request.UploadInfo != null && request.UploadInfo.Success)
                {
                    return request.UploadInfo;
                }

                string path = string.Empty;
                if(request.NativePath != null && request.NativePath.Contains("tmpupload"))
                {
                    path = request.NativePath;
                }
                else
                {
                    string directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "StencilNative", "tmpupload");
                    Container.FileStore.EnsureFolderExists(directory);
                    path = System.IO.Path.Combine(directory, "uploadtemp.mp4");

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
                string path = string.Empty;
                if(request.NativePath != null && request.NativePath.Contains("tmpupload"))
                {
                    path = request.NativePath;
                }
                else
                {
                    if(request.NativeData == null)
                    {
                        path = request.NativePath;
                    }
                    else
                    {
                        Bitmap image = request.NativeData as Bitmap;
                        if(image == null)
                        {
                            return new AmazonUploadInfo()
                            {
                                Success = false
                            };
                        }
                        string directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "StencilNative", "tmpupload");
                        Container.FileStore.EnsureFolderExists(directory);
                        path = System.IO.Path.Combine(directory, "uploadtemp.jpg");
                        using(var fileStream = Container.FileStore.OpenWrite(path))
                        {
                            image.Compress(Bitmap.CompressFormat.Jpeg, 100, fileStream);
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
        protected virtual Task<bool> UploadDirect(string localFilePath, string presignedUrl, EventHandler<UploadProgressArgs> onProgressChanged)
        {
            return base.ExecuteFunctionAsync("UploadDirect", async delegate()
            {
                try 
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
                                Console.WriteLine("delaying");
                                await Task.Delay(700);
                                System.Threading.Thread.Sleep(1700);
                            }
                            #endif
                        }
                        requestStream.Close();

                        WebResponse response = request.GetResponse();

                        base.LogWarning(response.GetResponseStream().Length.ToString());
                        return true;
                    }
                } 
                catch (Exception ex) 
                {
                    base.LogError(ex, "UploadDirect");
                    return false;
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

