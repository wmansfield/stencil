using System;
using UIKit;
using Foundation;
using System.Threading.Tasks;
using Stencil.Native.Core;

namespace Stencil.Native.iOS.Core
{
    public class BaseUITabBarController : UITabBarController
    {
        public BaseUITabBarController() : base()
        {

        }
        public BaseUITabBarController(NSCoder coder) : base(coder)
        {

        }
        public BaseUITabBarController(NSObjectFlag t) : base(t)
        {
        }
        public BaseUITabBarController(IntPtr handle) : base(handle)
        {
        }
        public BaseUITabBarController(string nibName, NSBundle bundle) : base(nibName, bundle)
        {

        }


        protected string TrackPrefix { get; set; }
        protected virtual IStencilApp StencilApp 
        { 
            get
            {
                return Container.StencilApp;
            }
        }
        protected UIViewController ViewControllerToDiposeOnAppear { get; set; }

        public virtual void PushViewControllerWithDisposeOnReturn(UIViewController controller, bool animated)
        {
            this.ExecuteMethod("PushViewControllerWithDisposeOnReturn", delegate()
            {
                this.ViewControllerToDiposeOnAppear = controller;
                this.NavigationController.PushViewController(controller, true);
            });
        }
        public virtual void PresentViewControllerWithDisposeOnReturn(UIViewController controller, bool animated, Action completion)
        {
            this.ExecuteMethod("PresentViewControllerWithDisposeOnReturn", delegate()
            {
                this.ViewControllerToDiposeOnAppear = controller;
                this.PresentViewController(controller, animated, completion);
            });
        }

        public override void ViewDidAppear(bool animated)
        {
            this.ExecuteMethod("ViewDidAppear", delegate()
            {
                base.ViewDidAppear(animated);

                this.ViewControllerToDiposeOnAppear = this.ViewControllerToDiposeOnAppear.DisposeSafe();

            });
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                #if DEBUG
                if(CoreUtility.ALLOW_DISPOSE_TRACE)
                {
                    this.LogTrace("TabBar.Disposing: " + this.TrackPrefix);
                }
                #endif
            }
            base.Dispose(disposing);
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

