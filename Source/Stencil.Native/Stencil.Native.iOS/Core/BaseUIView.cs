using CoreGraphics;
using Foundation;
using Stencil.Native.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace Stencil.Native.iOS.Core
{
    public abstract class BaseUIView : UIView
    {
        public BaseUIView()
            : base()
        {
        }
        public BaseUIView(string trackPrefix)
            : base()
        {
            this.TrackPrefix = trackPrefix;
        }
        public BaseUIView(NSCoder coder)
            : base(coder)
        {
        }

        public BaseUIView(IntPtr handle)
            : base(handle)
        {
        }

        public BaseUIView(CGRect frame)
            : base(frame)
        {
        }

        public BaseUIView(NSObjectFlag t)
            : base(t)
        {
        }


        protected string TrackPrefix { get; set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
#if DEBUG
                if (CoreUtility.ALLOW_DISPOSE_TRACE)
                {
                    this.LogTrace("View.Disposing: " + this.TrackPrefix);
                }
#endif
            }
            base.Dispose(disposing);
        }
        protected virtual void ExecuteMethodOnMainThread(string name, Action method)
        {
            if (NSThread.IsMain)
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
        protected virtual void ExecuteMethodOnMainThreadBegin(string name, Action method)
        {
            this.BeginInvokeOnMainThread(delegate ()
            {
                this.ExecuteMethod(name, method);
            });
        }

        protected virtual void ExecuteMethod(string name, Action method, Action<Exception> onError = null)
        {
            CoreUtility.ExecuteMethod(string.Format("{0}.{1}", this.TrackPrefix, name), method, onError);
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
            return delegate ()
            {
                ExecuteMethod(name, method, onError);
            };
        }


        protected virtual void LogWarning(string message, string tag = "")
        {
            Container.Track.LogWarning(this.TrackPrefix + ":" + message, tag);
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

