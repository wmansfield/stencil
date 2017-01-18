using System;
using Android.Support.V4.App;
using System.Threading.Tasks;
using System.Collections.Generic;
using Android.Views;
using Android.Views.InputMethods;
using Android.OS;
using Android.Widget;
using Stencil.Native.Droid.Core.UI;
using Stencil.Native.Core;
using Stencil.Native.ViewModels;

namespace Stencil.Native.Droid.Core
{
    
    public abstract class BaseFragment : Fragment
    {
        public BaseFragment(string trackPrefix)
        {
            this.TrackPrefix = trackPrefix;
        }

        #region Public Properties


        public virtual bool IsActivityVisible { get; set; }
        public LayoutZone LayoutZone { get; set; }
        public string Title { get; set; }
        public string TrackPrefix { get; protected set; }
        public virtual IStencilApp StencilApp
        {
            get
            {
                return Container.StencilApp;
            }
        }
        public string AnalyticsPrefix { get; set; }
        public bool ViewCreated { get; protected set; }


        #endregion

        #region Overrides

        public override void OnActivityResult(int requestCode, int resultCode, Android.Content.Intent data)
        {
            this.ExecuteMethod("OnActivityResult", delegate()
            {
                base.OnActivityResult(requestCode, resultCode, data);

                IList<Fragment> fragments = this.ChildFragmentManager.Fragments;
                if (fragments != null) 
                {
                    foreach (Fragment fragment in fragments) 
                    {
                        fragment.OnActivityResult(requestCode, resultCode, data);
                    }
                }
            });
        }
        public override void OnResume()
        {
            base.OnResume();

            this.IsActivityVisible = true;

            Container.ViewPlatform.RecentView = this;
        }
        public override void OnPause()
        {
            base.OnPause();

            this.IsActivityVisible = false;
        }

        public override void OnDestroy()
        {
            this.ExecuteMethod("OnDestroy", delegate()
            {
                this.ClearControlReferences();
                this.ClearKeyboardMethods();

                base.OnDestroy();
            });
        }
        public override void OnStop()
        {
            this.ExecuteMethod("OnStop", delegate()
            {
                this.ClearControlReferences();

                base.OnStop();
            });
        }
        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            this.ViewCreated = true;
        }
        public override void OnDestroyView()
        {
            this.ViewCreated = false;
            base.OnDestroyView();
        }
        #endregion

        #region MultiView Support

        public virtual void OnAddedToMultiHost(IMultiViewHost host, LayoutZone zone)
        {
        }
        public virtual void OnMadeVisibleInPager()
        {
            
        }
        #endregion

        #region Core Methods

        public virtual void ScrollToTop()
        {
            // extensibility
        }

        protected virtual void ExecuteMethodOnMainThread(string name, Action method)
        {
            this.Activity.RunOnUiThread(delegate()
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


        #endregion

        #region Keyboard Helpers

        public void HideKeyboard()
        {
            this.ExecuteMethod("HideKeyboard", delegate()
            {
                InputMethodManager imm = (InputMethodManager) this.Activity.GetSystemService(Android.Content.Context.InputMethodService);

                if(imm.IsAcceptingText && this.Activity.CurrentFocus != null) 
                {               
                    imm.HideSoftInputFromWindow(this.Activity.CurrentFocus.WindowToken, 0);
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

        #region Control Helpers

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

