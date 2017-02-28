using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Stencil.Native.Droid.Core;
using Stencil.Native.ViewModels;
using Android.Content.PM;
using Stencil.Native.Core;
using Stencil.SDK;
using Stencil.SDK.Models.Responses;
using Stencil.SDK.Models.Requests;
using Stencil.SDK.Models;

namespace Stencil.Native.Droid.Activities
{
    [Activity(Label = "@string/app_name", ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/Theme.Stencil")]
    public class PostsActivity : BaseActivity, IDataListViewModelView<Post>, IViewModelView
    {
        public PostsActivity()
            : base("PostsActivity")
        {
        }

        #region Controls

        protected TextView btnLogout { get { return this.GetControl<TextView>(Resource.Id.btn_logoff); } }
        protected TextView lblHeader { get { return this.GetControl<TextView>(Resource.Id.general_h1); } }
        protected TextView btnCreate { get { return this.GetControl<TextView>(Resource.Id.btn_create); } }

        protected ListView lvData { get { return this.GetControl<ListView>(Resource.Id.general_list); } }
        protected CoreSwipeRefreshLayout srlRefresh { get { return this.GetControl<CoreSwipeRefreshLayout>(Resource.Id.general_refresh_control); } }


        #endregion

        #region Properties

        public PostsViewModel ViewModel { get; set; }
        public CoreArrayAdapter<Post> ListAdapter { get; set; }

        #endregion

        #region Overrides

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.ExecuteMethod("OnCreate", delegate ()
            {
                base.OnCreate(savedInstanceState);

                this.ViewModel = new PostsViewModel(this);

                this.SetContentView(Resource.Layout.Posts);

                this.InitializeInfiniteScroll(this.ViewModel, srlRefresh, lvData);


                btnLogout.Click += btnLogout_Click;
                btnCreate.Click += btnCreate_Click;

                lblHeader.Text = this.ViewModel.Text_Title;

                srlRefresh.OnRefreshRequested = this.RefreshLayout_RefreshRequested;

                this.ViewModel.Start();
            });
        }

        protected override void OnResume()
        {
            base.ExecuteMethod("OnResume", delegate ()
            {
                base.OnResume();

                this.ViewModel.OnAppear();
            });
        }
        protected override void OnPause()
        {
            base.ExecuteMethod("OnPause", delegate ()
            {
                base.OnPause();

                this.ViewModel.OnDisappear();
            });
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            this.OverridePendingTransitionDefault();
        }


        #endregion

        #region Data Methods

        public void BindData(bool live_data, List<Post> data)
        {
            base.ExecuteMethodOnMainThread("BindData", delegate ()
            {
                if(this.ListAdapter == null) // reusing for scroll keep
                {
                    this.ListAdapter = new CoreArrayAdapter<Post>(this, this, this.ViewModel.Data, this.CellCreate, this.CellGetType);
                    lvData.Adapter = this.ListAdapter;
                }
                else
                {
                    this.ListAdapter.ReplaceData(this.ViewModel.Data);
                    this.ListAdapter.NotifyDataSetChanged();
                }

            });
        }
        public void AddData(List<Post> data)
        {
            base.ExecuteMethodOnMainThread("AddData", delegate ()
            {
                if(this.ListAdapter == null) // reusing for scroll keep
                {
                    this.ListAdapter = new CoreArrayAdapter<Post>(this, this, this.ViewModel.Data, this.CellCreate, this.CellGetType);
                    lvData.Adapter = this.ListAdapter;
                }
                else
                {
                    this.ListAdapter.ReplaceData(this.ViewModel.Data);
                    this.ListAdapter.NotifyDataSetChanged();
                }

            });
        }
        public void OnDataRefreshing(bool refreshing)
        {
            base.ExecuteMethodOnMainThread("OnDataRefreshing", delegate ()
            {
                srlRefresh.Refreshing = refreshing;
            });
        }
        public void OnDataGettingMore(bool gettingMore)
        {
        }

        private CoreCell<Post> CellCreate(int position, Android.Views.View convertView, Android.Views.ViewGroup parent)
        {
            return base.ExecuteFunction<CoreCell<Post>>("CellCreate", delegate ()
            {
                Post item = this.ViewModel.Data[position];
                switch(item.ui_token)
                {
                    case PostsViewModel.TOKEN_TEXT:
                        return new CellPostText();
                    case PostsViewModel.TOKEN_POST:
                    default:
                        return new CellPost();
                }
            });

        }
        private Type CellGetType(int position)
        {
            return base.ExecuteFunction("CellGetType", delegate ()
            {
                Post item = this.ViewModel.Data[position];

                switch(item.ui_token)
                {
                    case PostsViewModel.TOKEN_TEXT:
                        return typeof(CellPostText);
                    case PostsViewModel.TOKEN_POST:
                    default:
                        return typeof(CellPost);
                }
            });
        }

        #endregion


        #region Event Handlers

        private void btnLogout_Click(object sender, EventArgs e)
        {
            base.ExecuteMethod("btnLogout_Click", delegate ()
            {
                this.StencilApp.LogOff(true, true);
            });
        }
        private void btnCreate_Click(object sender, EventArgs e)
        {
            base.ExecuteMethod("btnCreate_Click", delegate ()
            {
                this.StartActivity<PostCreateActivity>();
            });
        }
        private void RefreshLayout_RefreshRequested()
        {
            base.ExecuteMethodOnMainThread("RefreshLayout_RefreshRequested", delegate ()
            {
                this.ViewModel.DoRefreshData(true);
            });
        }

        #endregion

    }
}
