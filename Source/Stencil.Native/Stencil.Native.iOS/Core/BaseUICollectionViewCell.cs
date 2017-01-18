using System;
using UIKit;
using Foundation;
using CoreGraphics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stencil.Native.Core;

namespace Stencil.Native.iOS.Core
{
    public class BaseUICollectionViewCell : UICollectionViewCell
    {
        public BaseUICollectionViewCell()
        {
        }
        public BaseUICollectionViewCell(IntPtr handle) : base(handle)
        {
        }

        public BaseUICollectionViewCell(CGRect frame) : base(frame)
        {
        }

        public BaseUICollectionViewCell(NSCoder coder) : base(coder)
        {
        }

        public BaseUICollectionViewCell(NSObjectFlag t) : base(t)
        {
        }

        protected virtual void AutoDispose(IDisposable disposable)
        {
            this.ExecuteMethod("AutoDispose", delegate()
            {
                this.Disposables.Add(disposable);
            });
        }
        protected virtual List<IDisposable> Disposables { get; set; }
        public virtual string TrackPrefix { get; set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.Disposables != null)
                {
                    foreach (var item in this.Disposables)
                    {
                        item.Dispose();
                    }
                    this.Disposables.Clear();
                    this.Disposables = null;
                }
                #if DEBUG
                if(CoreUtility.ALLOW_DISPOSE_TRACE)
                {
                    this.LogTrace("CollectionCell.Disposing: " + this.TrackPrefix);
                }
                #endif
            }
            base.Dispose(disposing);
        }

        protected virtual void ExecuteMethodOnMainThreadBegin(string name, Action method)
        {
            this.BeginInvokeOnMainThread(delegate()
            {
                this.ExecuteMethod(name, method);
            });
        }
        protected virtual void ExecuteMethodOnMainThread(string name, Action method)
        {
            this.InvokeOnMainThread(delegate()
            {
                this.ExecuteMethod(name, method);
            });
        }
        protected virtual void ExecuteMethod(string name, Action method, Action<Exception> onError = null, bool supressMethodLogging = false)
        {
            CoreUtility.ExecuteMethod(string.Format("{0}.{1}", this.TrackPrefix, name), method, onError, supressMethodLogging);
        }
        protected virtual Task ExecuteMethodAsync(string name, Func<Task> method, Action<Exception> onError = null)
        {
            return CoreUtility.ExecuteMethodAsync(string.Format("{0}.{1}", this.TrackPrefix, name), method, onError);
        }
        protected virtual T ExecuteFunction<T>(string name, Func<T> method, Action<Exception> onError = null)
        {
            return CoreUtility.ExecuteFunction<T>(string.Format("{0}.{1}", this.TrackPrefix, name), method, onError);
        }
        protected virtual Task<T> ExecuteFunctionAsync<T>(string name, Func<Task<T>> method, Action<Exception> onError = null)
        {
            return CoreUtility.ExecuteFunctionAsync<T>(string.Format("{0}.{1}", this.TrackPrefix, name), method, onError);
        }

        public virtual Action CreateNSAction(string name, Action method, Action<Exception> onError = null)
        {
            return delegate()
            {
                ExecuteMethod(name, method, null);
            };
        }


        protected virtual void LogWarning(string message, string tag = "")
        {
            Container.Track.LogWarning(this.TrackPrefix + ":" + message,tag);
        }
        protected virtual void LogTrace(string message, string tag = "")
        {
            Container.Track.LogTrace(this.TrackPrefix + ":" + message, tag);
        }
        protected virtual void LogError(Exception ex, string tag = "")
        {
            Container.Track.LogError(ex, this.TrackPrefix + ":" + tag);
        }
        protected virtual void LogError(NSError error, string tag = "")
        {
            Container.Track.LogError(error.ConvertToException(), this.TrackPrefix + ":" + tag);
        }

    }
}

