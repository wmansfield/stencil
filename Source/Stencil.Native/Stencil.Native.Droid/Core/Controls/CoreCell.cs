using System;
using Android.Views;
using Android.App;
using Android.Content;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stencil.Native.Core;

namespace Stencil.Native.Droid
{
    
    public abstract class CoreCell<TData> : Java.Lang.Object
    {
        public CoreCell(string trackPrefix)
        {
            this.TrackPrefix = trackPrefix;
        }

        public virtual TData Data { get; set; }

        public virtual string TrackPrefix { get; set; }
        public virtual View View { get; set; }
        public abstract int ResourceID { get;  }
        public virtual Context Context { get; set; }

        public virtual void OnViewCreated(View view)
        {
            // extension only
        }
        /// <summary>
        /// Should be called by Array Adapter only
        /// </summary>
        public virtual void BindCellData(object owner, Context context, TData data)
        {
            this.ExecuteMethod("BindCellData", delegate()
            {
                this.Context = context;

                this.BindData(owner, context, data);
            });
        }
        public abstract void BindData(object owner, Context context, TData data);

        protected virtual void ExecuteMethodOnMainThread(string name, Action method)
        {
            Activity activity = this.Context as Activity;
            if (activity != null)
            {
                activity.RunOnUiThread(delegate()
                {
                    this.ExecuteMethod(name, method);
                });
            }
            else
            {
                // even possible?
                this.ExecuteMethod(name, method);
            }
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
            T result = this.View.FindViewById<T>(resourceId);
            _controls[resourceId] = result;
            return result;
        }
        protected void ClearControlReferences()
        {
            _controls.Clear();
        }
    }
}

