using System;
using Android.App;
using System.Threading.Tasks;
using Stencil.Native.Core;

namespace Stencil.Native.Droid.Core
{
    public abstract class BaseService : Service
    {
        public BaseService(string trackPrefix)
            : base()
        {
            this.TrackPrefix = trackPrefix;
        }
        protected string TrackPrefix { get; set; }

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

    }
}

