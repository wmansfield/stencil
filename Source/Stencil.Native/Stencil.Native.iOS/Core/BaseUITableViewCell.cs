using System;
using UIKit;
using System.Threading.Tasks;
using Foundation;
using CoreGraphics;
using MessageUI;
using Stencil.Native.Core;

namespace Stencil.Native.iOS.Core
{
    public class BaseUITableViewCell : UITableViewCell
    {
        public BaseUITableViewCell() : base()
        {
            this.LayoutIfNeeded();
        }
        public BaseUITableViewCell(NSObjectFlag t) : base(t)
        {
            this.LayoutIfNeeded();
        }
        public BaseUITableViewCell(UITableViewCellStyle style, NSString reuseIdentifier) : base(style, reuseIdentifier)
        {
            this.LayoutIfNeeded();
        }
        public BaseUITableViewCell(CGRect frame) : base(frame)
        {
            this.LayoutIfNeeded();
        }
        public BaseUITableViewCell(IntPtr handle) : base(handle)
        {
            this.LayoutIfNeeded();
        }
        public BaseUITableViewCell(UITableViewCellStyle style, string reuseIdentifier) : base(style, reuseIdentifier)
        {
            this.LayoutIfNeeded();
        }

        public BaseUITableViewCell(NSCoder coder) : base(coder)
        {
            this.LayoutIfNeeded();
        }

        public virtual string TrackPrefix { get; set; }

        public override void WillMoveToSuperview(UIView newsuper)
        {
            this.ExecuteMethod("WillMoveToSuperview", delegate()
            {
                base.WillMoveToSuperview(newsuper);

                this.BackgroundColor = this.ContentView.BackgroundColor; // ios 9.1 on ipad doesnt inherit it for some weird reason
            });
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
            if(NSThread.IsMain)
            {
                this.ExecuteMethod(name, method);
            }
            else
            {
                this.InvokeOnMainThread(delegate ()
                {
                    this.ExecuteMethod(name, method);
                });
            }
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
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                #if DEBUG
                if(CoreUtility.ALLOW_DISPOSE_TRACE)
                {
                    this.LogTrace("Cell.Disposing: " + this.TrackPrefix);
                }
                #endif
            }
            base.Dispose(disposing);
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

