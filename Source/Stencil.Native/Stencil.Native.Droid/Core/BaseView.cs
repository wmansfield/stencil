using System;
using Android.Views;
using Android.App;
using System.Threading.Tasks;
using System.Collections.Generic;
using Android.Content;
using Android.Util;
using Stencil.Native.Core;

namespace Stencil.Native.Droid.Core
{
    public class BaseView : View
    {
        public BaseView(Context context) 
            : base(context)
        {

        }
        public BaseView(Context context, IAttributeSet attrs) 
            : base(context, attrs)
        {

        }

        public BaseView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {

        }

        protected string TrackPrefix { get; set; }

        protected virtual void ExecuteMethodOnMainThread(string name, Action method)
        {
            ((Activity)this.Context).RunOnUiThread(delegate()
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ClearControlReferences();
            }
            base.Dispose(disposing);
        }

        protected Dictionary<int, View> _controls = new Dictionary<int, View>();        
        protected T GetControl<T>(int resourceId) where T:View
        {
            if (_controls.ContainsKey(resourceId))
            {
                if (_controls[resourceId] != null)
                {
                    return _controls[resourceId] as T;
                }
            }
            T result = this.FindViewById<T>(resourceId);
            _controls[resourceId] = result;
            return result;
        }
        protected void ClearControlReferences()
        {
            _controls.Clear();
        }
    }
}

