using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Android.Views;
using Android.Support.V7.App;
using Stencil.Native.Core;

namespace Stencil.Native.Droid.Core
{
    public class BaseFragmentActivity : AppCompatActivity
    {
        public BaseFragmentActivity(string trackPrefix)
        {
            this.TrackPrefix = trackPrefix;
        }


        public virtual bool IsActivityVisible { get; set; }
        public virtual IStencilApp StencilApp
        {
            get
            {
                return Container.StencilApp;
            }
        }
        protected override void OnResume()
        {
            base.OnResume();

            Container.ViewPlatform.RecentView = this;

            this.IsActivityVisible = true;

        }
        protected override void OnPause()
        {
            base.OnPause();

            this.IsActivityVisible = false;
        }

        protected string TrackPrefix { get; set; }

        protected virtual void ExecuteMethodOnMainThread(string name, Action method)
        {
            this.RunOnUiThread(delegate()
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


        protected override void OnDestroy()
        {
            this.ExecuteMethod("OnDestroy", delegate()
            {
                this.ClearControlReferences();

                base.OnDestroy();
            });
        }
        protected override void OnStop()
        {
            this.ExecuteMethod("OnStop", delegate()
            {
                this.ClearControlReferences();

                base.OnStop();
            });
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

