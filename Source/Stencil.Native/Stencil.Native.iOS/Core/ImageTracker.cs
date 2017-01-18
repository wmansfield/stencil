using System;
using UIKit;
using CoreGraphics;
using Foundation;
using SDWebImage;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stencil.Native.Core;

namespace Stencil.Native.iOS.Core
{
    /// <summary>
    /// Not the best solution, but seems to be stable
    /// </summary>
    public class ImageTracker : BaseClass, IDisposable
    {
        public static void PreLoadImage(string url)
        {
            CoreUtility.ExecuteMethod("PreLoadImage", delegate()
            {
                if(!string.IsNullOrEmpty(url))
                {
                    SDWebImageManager.SharedManager.Download(
                        new NSUrl(url),
                        0,
                        delegate(nint receivedSize, nint expectedSize) {},
                        delegate(UIImage image, NSError error, SDImageCacheType cacheType, bool finished, NSUrl imageUrl) {}
                    );
                }
            });
        }
        public static List<UIImage> GetImagesSynchronous(string[] urls)
        {
            return CoreUtility.ExecuteFunction("GetImagesSynchronous", delegate()
            {
                int closureTotal = urls.Length;
                List<UIImage> result = new List<UIImage>();

                List<Task> tasks = new List<Task>();
                foreach (string url in urls) 
                {
                    var completion = new TaskCompletionSource<UIImage>();

                    SDWebImageManager.SharedManager.Download(
                        new NSUrl(url),
                        0,
                        delegate(nint receivedSize, nint expectedSize) {},
                        delegate(UIImage image, NSError error, SDImageCacheType cacheType, bool finished, NSUrl imageUrl) { 
                            if(image != null)
                            {
                                result.Add(image);
                                completion.TrySetResult(image);
                            }
                            else
                            {
                                completion.TrySetResult(null);
                            }
                        }
                    );

                    tasks.Add(completion.Task);
                }

                Task.WaitAll(tasks.ToArray());

                return result;
            });
        }

        public ImageTracker(UIImageView imageView)
            : base("ImageTracker")
        {
            this.ImageView = imageView;
        }
        ~ImageTracker()
        {
            this.Dispose(false);
        }
        public UIImageView ImageView { get; protected set; }
        public string CurrentUrl { get; protected set; }
        public string NewUrl { get; protected set; }
        public Action AfterImageDownloaded { get; set; }

        public CGSize ImageSize { get; protected set; }

        private ISDWebImageOperation _recentOperation;

        public void DownloadIfNeeded(string newUrl, Action afterDownloaded = null)
        {
            base.ExecuteMethod("DownloadIfNeeded", delegate()
            {
                this.AfterImageDownloaded = afterDownloaded;
                this.PrepareForDownload(newUrl);
                if(this.ShouldDownload())
                {
                    this.StartDownload();
                }
            });
        }
        public void StartDownload()
        {
            base.ExecuteMethod("StartDownload", delegate()
            {
                if(!string.IsNullOrEmpty(this.NewUrl))
                {
                    _recentOperation = SDWebImageManager.SharedManager.Download(
                        new NSUrl(this.NewUrl),
                        0,
                        delegate(nint receivedSize, nint expectedSize) {
                    },
                        this.CompleteDownload
                    );
                }
            });
        }
        public void CancelDownload()
        {
            base.ExecuteMethod("CancelDownload", delegate()
            {
                if(_recentOperation != null)
                {
                    if(this.CurrentUrl != this.NewUrl)
                    {
                        base.LogWarning("Cancelling image download");
                        _recentOperation.Cancel();
                    }
                }
            });
        }

        public void PrepareForDownload(string newUrl)
        {
            base.ExecuteMethod("PrepareForDownload", delegate()
            {
                if(string.IsNullOrEmpty(newUrl))
                {
                    this.NewUrl = string.Empty;
                    this.CurrentUrl = string.Empty;
                    this.ImageView.Image = this.ImageView.Image.DisposeSafe();
                }
                else if(newUrl != this.CurrentUrl)
                {
                    this.NewUrl = newUrl;
                    this.CurrentUrl = string.Empty;
                    this.ImageView.Image = this.ImageView.Image.DisposeSafe();
                }
                else // newUrl == this.CurrentUrl
                {
                    if(this.ImageView.Image == null)
                    {
                        // not sure how this would happen
                        this.NewUrl = newUrl;
                    }
                    else
                    {
                        this.NewUrl = string.Empty;
                    }
                }
                this.ImageSize = this.ImageView.Bounds.Size; // for future threadsafety
            });
        }
        public bool ShouldDownload()
        {
            return base.ExecuteFunction("ShouldDownload", delegate()
            {
                return (this.ImageView.Image == null) || !string.IsNullOrEmpty(this.NewUrl);
            });
        }

        protected void CompleteDownload(UIImage image, NSError error, SDImageCacheType cacheType, bool finished, NSUrl imageUrl)
        {
            base.ExecuteMethod("CompleteDownload", delegate()
            {
                if(!finished) { return; }
                if(imageUrl.ToString() == this.NewUrl)
                {
                    this.ImageView.Image = image;
                    _recentOperation = null;
                    this.CurrentUrl = imageUrl.ToString();

                    if(this.AfterImageDownloaded != null)
                    {
                        this.AfterImageDownloaded();
                    }
                }
            });
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ImageView = null;
            }
            base.Dispose(disposing);
        }
    }
}

