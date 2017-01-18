using System;
using UIKit;
using Foundation;
using System.Threading.Tasks;
using Stencil.Native.Core;

namespace Stencil.Native.iOS.Core
{
    public class BaseUIPageViewController : UIPageViewController
    {
        public BaseUIPageViewController() : base()
        {

        }
        public BaseUIPageViewController(NSCoder coder) : base(coder)
        {

        }
        public BaseUIPageViewController(NSObjectFlag t) : base(t)
        {
        }
        public BaseUIPageViewController(IntPtr handle) : base(handle)
        {
        }
        public BaseUIPageViewController(string nibName, NSBundle bundle) : base(nibName, bundle)
        {

        }

       
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
        }
        protected string TrackPrefix { get; set; }
        protected virtual IStencilApp StencilApp
        {
            get
            {
                return Container.StencilApp;
            }
        }

        protected virtual void ExecuteMethodOnMainThread(string name, Action method)
        {
            this.BeginInvokeOnMainThread(delegate()
            {
                this.ExecuteMethod(name, method);
            });
        }
        protected virtual void ExecuteMethodOnMainThreadBegin(string name, Action method)
        {
            this.InvokeOnMainThread(delegate()
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

