using System;
using Android.App;
using System.Threading.Tasks;
using System.Collections.Generic;
using Android.Views;
using Android.Content;
using Android.Support.V7.App;
using Android.Widget;
using Android.Views.InputMethods;
using Android.Support.V4.Content;
using sdk = Stencil.SDK.Models;
using Stencil.SDK;
using Android.OS;
using Stencil.Native.Core;
using Stencil.Native.ViewModels;

namespace Stencil.Native.Droid.Core
{
    public class BaseActivity : AppCompatActivity
    {
        public BaseActivity(string trackPrefix)
        {
            this.TrackPrefix = trackPrefix;
        }

        public const string INIT_BUNDLE_KEY = "initValues";


        public virtual IStencilApp StencilApp
        {
            get
            {
                return Container.StencilApp;
            }
        }
        public string AnalyticsPrefix { get; set; }
        public virtual bool IsActivityVisible { get; set; }

        protected override void OnResume()
        {
            this.ExecuteMethod("OnResume", delegate()
            {
                base.OnResume();

                Container.ViewPlatform.RecentView = this;

                this.IsActivityVisible = true;
            });
        }
        protected override void OnPause()
        {
            this.IsActivityVisible = false;

            base.OnPause();
        }

        protected string TrackPrefix { get; set; }

        public virtual void ExecuteMethodOnMainThread(string name, Action method)
        {
            this.RunOnUiThread(delegate()
            {
                this.ExecuteMethod(name, method);
            });
        }

        public virtual void StartActivity<T>(bool finishCurrent, string extraValues = "", bool animate = true, int enterAnimation = Resource.Animation.slide_in_from_right, int exitAnimation = Resource.Animation.slide_out_to_left) 
            where T : Activity
        {
            var newActivity = new Intent(this, typeof(T));
            if (!string.IsNullOrEmpty(extraValues))
            {
                newActivity.PutExtra(INIT_BUNDLE_KEY, extraValues);
            }
            this.StartActivity(newActivity);
            if (finishCurrent)
            {
                Finish();
            }
            if (animate)
            {
                OverridePendingTransition(enterAnimation, exitAnimation);
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


        protected override void OnDestroy()
        {
            this.ExecuteMethod("OnDestroy", delegate()
            {
                this.ClearControlReferences();
                this.ClearKeyboardMethods();

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


        private HashSet<string> _executingCommands = new HashSet<string>();
        protected virtual HashSet<string> executingCommands
        {
            get
            {
                return _executingCommands;
            }
        }

        /// <summary>
        /// Executes the command unless the command is already running, then its skipped
        /// </summary>
        protected virtual void ExecuteMethodOrSkip(string name, Action method, Action<Exception> onError = null)
        {
            this.ExecuteMethod("ExecuteOrSkip", delegate()
            {
                bool added = _executingCommands.Add(name);
                if (!added) { return; }
                try
                {
                    method();
                }
                finally
                {
                    _executingCommands.Remove(name);
                }
            });
        }
        /// <summary>
        /// Executes the command unless the command is already running, then its skipped
        /// </summary>
        protected virtual Task ExecuteMethodOrSkipAsync(string name, Func<Task> method, Action<Exception> onError = null)
        {
            return this.ExecuteMethodAsync("ExecuteMethodOrSkipAsync", async delegate()
            {
                bool added = _executingCommands.Add(name);
                if (!added) { return; }
                try
                {
                    await method();
                }
                finally
                {
                    _executingCommands.Remove(name);
                }
            });
        }

        protected virtual bool IsExecutingCommand(string name)
        {
            return this.ExecuteFunction("IsExecutingCommand", delegate()
            {
                return _executingCommands.Contains(name);
            });
        }


        #region Keyboard Helpers

        public void HideKeyboard()
        {
            this.ExecuteMethod("HideKeyboard", delegate()
            {
                InputMethodManager imm = (InputMethodManager) this.GetSystemService(Context.InputMethodService);

                if(imm.IsAcceptingText && this.CurrentFocus != null) 
                {               
                    imm.HideSoftInputFromWindow(this.CurrentFocus.WindowToken, 0);
                }
            });
        }

        protected Dictionary<EditText, Action> ReturnExecuteFields { get; set; }

        protected void ClearKeyboardMethods()
        {
            this.ExecuteMethod("ClearKeyboardMethods", delegate()
            {
                if (this.ReturnExecuteFields != null)
                {
                    foreach (var item in ReturnExecuteFields)
                    {
                        item.Key.EditorAction -= OnReturnExecute_EditorAction;
                    }
                    this.ReturnExecuteFields.Clear();
                    this.ReturnExecuteFields = null;
                }
            });
        }
        public virtual void OnReturnDismiss(EditText textField)
        {
            this.ExecuteMethod("OnReturnDismiss", delegate()
            {
                this.OnReturnExecute(textField, this.HideKeyboard);
            });
        }
        public virtual void OnReturnExecute(EditText textField, Action action)
        {
            this.ExecuteMethod("OnReturnExecute", delegate()
            {
                if(this.ReturnExecuteFields == null)
                {
                    this.ReturnExecuteFields = new Dictionary<EditText, Action>();
                }
                textField.EditorAction += OnReturnExecute_EditorAction;
                this.ReturnExecuteFields[textField] = action;
            });
        }
        private void OnReturnExecute_EditorAction(object sender, EditText.EditorActionEventArgs e) 
        {
            this.ExecuteMethod("OnReturnExecute_EditorAction", delegate()
            {
                if (e.ActionId == ImeAction.Done || e.ActionId == ImeAction.Next || e.ActionId == ImeAction.Go)
                {
                    EditText editText = sender as EditText;
                    if(this.ReturnExecuteFields.ContainsKey(editText))
                    {
                        this.ReturnExecuteFields[editText]();
                    }
                }
            });
        }

        #endregion

        #region Default Scroll

        private InfiniteScroll _infiniteScroller;
        public void InitializeInfiniteScroll(IDataListViewModel viewModel, CoreSwipeRefreshLayout refreshLayout, ListView listView)
        {
            this.ExecuteMethod("InitializeInfiniteScroll", delegate ()
            {
                _infiniteScroller = new Droid.InfiniteScroll(viewModel, refreshLayout, listView);
                listView.SetOnScrollListener(_infiniteScroller);
            });
        }

        #endregion
    }
}

