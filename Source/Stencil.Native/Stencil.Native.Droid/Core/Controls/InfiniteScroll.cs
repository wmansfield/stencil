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
using Stencil.Native.ViewModels;
using Stencil.Native.Core;

namespace Stencil.Native.Droid
{
    /// <summary>
    /// Don't forget to call
    /// this.ListView.SetOnScrollListener(this);
    /// </summary>
    public class InfiniteScroll : Java.Lang.Object, global::Android.Widget.AbsListView.IOnScrollListener
    {
        public InfiniteScroll(IDataListViewModel viewModel, CoreSwipeRefreshLayout refreshLayout, ViewGroup listView)
        {
            this.ViewModel = viewModel;
            this.RefreshLayout = refreshLayout;
            this.ListView = listView;

            
        }
        public IDataListViewModel ViewModel { get; set; }
        public CoreSwipeRefreshLayout RefreshLayout { get; set; }
        public ViewGroup ListView { get; set; }

        public void OnScroll(AbsListView view, int firstVisibleItem, int visibleItemCount, int totalItemCount)
        {
        }

        public void OnScrollStateChanged(AbsListView listView, ScrollState scrollState)
        {
            CoreUtility.ExecuteMethod("OnScrollStateChanged", delegate ()
            {
                if (this.ViewModel != null)
                {
                    if (this.ViewModel.HasMoreData)
                    {
                        if (listView.LastVisiblePosition >= listView.Count - 1 - this.ViewModel.ScrollThresholdCount)
                        {
                            Container.Track.LogTrace("Getting more data");
                            this.ViewModel.DoGetMoreData(); // it has its own built-in skip mechanism
                        }
                    }
                    int topRowVerticalPosition = 0;
                    if (this.ListView != null && this.ListView.ChildCount > 0)
                    {
                        topRowVerticalPosition = this.ListView.GetChildAt(0).Top;
                    }
                    if (this.RefreshLayout != null)
                    {
                        this.RefreshLayout.Enabled = (topRowVerticalPosition >= 0);
                    }
                }
            });
        }
    }
}