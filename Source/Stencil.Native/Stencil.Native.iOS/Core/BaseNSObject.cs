using System;
using Foundation;
using System.Threading.Tasks;
using Stencil.Native.Core;

namespace Stencil.Native.iOS.Core
{
    public abstract class BaseNSObject : NSObject
    {
        public BaseNSObject(IntPtr handle, string trackPrefix)
            : base(handle)
        {
            this.TrackPrefix = trackPrefix;
        }
        public BaseNSObject(string trackPrefix)
            : base()
        {
            this.TrackPrefix = trackPrefix;
        }

        protected string TrackPrefix { get; set; }

        protected virtual void ExecuteMethodOnMainThread(string name, Action method)
        {
            this.BeginInvokeOnMainThread(delegate()
            {
                this.ExecuteMethod(name, method);
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                #if DEBUG
                if(CoreUtility.ALLOW_DISPOSE_TRACE)
                {
                    this.LogTrace("BaseNSObject.Disposing: " + this.TrackPrefix);
                }
                #endif
            }
            base.Dispose(disposing);
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

