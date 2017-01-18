using System;
using Android.Views;
using Android.Content;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stencil.Native.Core;

namespace Stencil.Native.Droid
{
    public abstract class CoreViewHolderCell<TData> : Java.Lang.Object
    {
        public CoreViewHolderCell(string trackPrefix)
        {
            this.TrackPrefix = trackPrefix;
        }

        public TData Data { get; set; }

        public string TrackPrefix { get; set; }
        public View View { get; set; }

        public abstract void BindData(object owner, Context context, TData data);

        public virtual void OnViewCreated(View view)
        {
            // extension only
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

