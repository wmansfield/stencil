using Stencil.Native.Caching;
using Stencil.SDK;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Native.ViewModels
{
    public abstract class BaseDataListViewModel<TModel> : BaseDataListViewModel<TModel, IDataListViewModelView<TModel>>
    {
        public BaseDataListViewModel(IDataListViewModelView<TModel> view, string trackPrefix)
            : base(view, trackPrefix)
        {
        }
    }

    public abstract class BaseDataListViewModel<T, TView> : BaseViewModel<TView>, IDataListViewModel
        where TView : IDataListViewModelView<T>
    {
        public BaseDataListViewModel(TView view, string trackPrefix)
            : base(view, trackPrefix)
        {
            this.DataViewModelView = view;
            this.Data = new List<T>();
        }

        #region Public Properties

        /// <summary>
        /// Used to allow multiple refreshes. Without, calling DoRefreshData when its already running will be skipped
        /// </summary>
        public virtual string RefreshParameter { get; }

        public virtual List<T> Data { get; set; }
        public virtual TView DataViewModelView { get; set; }

        public virtual bool SupressRefreshDataOnAppear { get; set; }

        public virtual bool HasMoreData { get; set; }
        /// <summary>
        /// Could be skip or page, depending on caller
        /// </summary>
        public virtual int NextStart { get; set; }
        public virtual bool ShowNoData { get; set; }

        public virtual bool AllowPagedStaleData { get; set; }
        public DateTime? PagingStartedUtc { get; set; }

        /// <summary>
        /// internal for data management, do not use
        /// </summary>
        protected DateTime? DataTrackerPagingStarted { get; set; }

        #endregion

        #region Protected Methods

        protected virtual RequestToken DataTrackerRefresh { get; set; }
        protected virtual RequestToken DataTrackerMore { get; set; }

        #endregion

        #region Abstract Methods

        protected abstract Task PerformRefreshData(RequestToken requestToken, bool force);
        protected abstract Task PerformGetMoreData(RequestToken requestToken, int start);

        public abstract int ScrollThresholdCount { get; }
        public abstract int ScrollThresholdSize { get; }

        #endregion

        #region Public Methods

        public Task DoRefreshData(bool force, bool suppressRefreshing = false, Action<bool> onFetching = null)
        {
            return base.ExecuteMethodOrSkipAsync(string.Format("DoRefreshData{0}", this.RefreshParameter), async delegate ()
            {
                if (!suppressRefreshing)
                {
                    this.OnDataRefreshing(true);
                }

                if (onFetching != null)
                {
                    onFetching(true);
                }
                RequestToken requestToken = new RequestToken(this.RefreshParameter);
                this.DataTrackerRefresh = requestToken;
                this.DataTrackerPagingStarted = DateTime.UtcNow;
                if (force)
                {
                    this.PagingStartedUtc = this.DataTrackerPagingStarted;
                }
                try
                {
                    await PerformRefreshData(requestToken, force);
                }
                finally
                {
                    if (!suppressRefreshing)
                    {
                        this.OnDataRefreshing(false);
                    }
                    if (onFetching != null)
                    {
                        onFetching(false);
                    }
                }
            });
        }

#if __IOS__

        public Task DoGetMoreData(Stencil.Native.iOS.Core.Data.IScrollListener scrollListener)
        {
            return base.ExecuteMethodAsync("DoGetMoreDataIos", async delegate ()
            {
                if (!this.HasMoreData) { return; }

                await DoGetMoreData(false, null);

                if (scrollListener != null)
                {
                    scrollListener.ListeningDisabled = (!this.HasMoreData); // assumes OnMoreDataRetrieved is properly called 
                }
            });
        }

#endif

        public Task DoGetMoreData()
        {
            return DoGetMoreData(false, null);
        }
        public Task DoGetMoreData(Action<bool> onGettingMore)
        {
            return DoGetMoreData(true, onGettingMore);
        }
        public Task DoGetMoreData(bool suppressRefreshing, Action<bool> onGettingMore)
        {
            return base.ExecuteMethodOrSkipAsync(string.Format("DoGetMoreData{0}", this.RefreshParameter), async delegate ()
            {
                if (!this.HasMoreData)
                {
                    this.LogTrace("Tried to get more when we have no more!");
                    return;
                }
                if (!suppressRefreshing)
                {
                    this.OnDataRefreshing(true);
                }
                if (onGettingMore != null)
                {
                    onGettingMore(true);
                }
                RequestToken requestToken = this.DataTrackerRefresh;
                this.DataTrackerMore = requestToken;

                try
                {
                    await PerformGetMoreData(requestToken, this.NextStart);
                }
                catch (Exception ex)
                {
                    this.LogError(ex, "DoGetMoreData");
                }
                finally
                {
                    if (!suppressRefreshing)
                    {
                        this.OnDataRefreshing(false);
                    }
                    if (onGettingMore != null)
                    {
                        onGettingMore(false);
                    }
                }

            });
        }

        public override void OnAppear()
        {
            base.ExecuteMethod("OnAppear", delegate ()
            {
                if (this.HasAppeared && !this.SupressRefreshDataOnAppear)
                {
                    this.DoRefreshData(true);
                }
                this.SupressRefreshDataOnAppear = false;
                base.OnAppear();
            });
        }

        public override void Start()
        {
            base.ExecuteMethod("Start", delegate ()
            {
                this.DoRefreshData(false);

                base.Start();
            });
        }

        public virtual void ReBindData()
        {
            base.ExecuteMethod("ReBindData", delegate ()
            {
                this.DataViewModelView.BindData(true, this.Data);
            });
        }

        #endregion

        #region Protected Methods

        protected virtual void OnDataRefreshing(bool refreshing)
        {
            base.ExecuteMethod("OnDataRefreshing", delegate ()
            {
                if (refreshing)
                {
                    this.ShowNoData = false;
                }
                this.DataViewModelView.OnDataRefreshing(refreshing);
            });
        }

        protected virtual void OnDataRetrieved(RequestToken requestToken, bool live_data, ListResult<T> data)
        {
            base.ExecuteMethod("OnDataRetrieved", delegate ()
            {

                if (this.DataTrackerRefresh != requestToken)
                {
                    // we've changed, don't update
                    return;
                }
                this.Data = data.items;

                if (live_data)
                {
                    this.PagingStartedUtc = this.DataTrackerPagingStarted;
                }
                if (data != null && data.stepping != null)
                {
                    this.HasMoreData = data.stepping.more;
                    this.NextStart = (int)data.stepping.skip;
                }
                if (data != null && data.paging != null)
                {
                    this.HasMoreData = data.paging.total_pages > data.paging.current_page;
                    this.NextStart = (int)data.paging.current_page + 1;

                    this.ShowNoData = (this.Data.Count == 0);
                }


                this.DataViewModelView.BindData(live_data, this.Data);
            });
        }
        protected virtual void OnDataRetrieved(RequestToken requestToken, ListResult<T> data)
        {
            base.ExecuteMethod("OnDataRetrieved", delegate ()
            {
                if (this.DataTrackerRefresh != requestToken)
                {
                    // we've changed, don't update
                    return;
                }
                this.Data = new List<T>(data.items);

                this.PagingStartedUtc = this.DataTrackerPagingStarted;

                if (data != null && data.stepping != null)
                {
                    this.HasMoreData = data.stepping.more;
                    this.NextStart = (int)data.stepping.skip;
                }
                if (data != null && data.paging != null)
                {
                    this.HasMoreData = data.paging.total_pages > data.paging.current_page;
                    this.NextStart = (int)data.paging.current_page + 1;

                    this.ShowNoData = (this.Data.Count == 0);
                }
                this.DataViewModelView.BindData(true, this.Data);
            });
        }

        protected virtual void OnMoreDataRetrieved(RequestToken requestToken, ListResult<T> items)
        {
            this.OnMoreDataRetrieved(requestToken, true, items);
        }
        protected virtual void OnMoreDataRetrieved(RequestToken requestToken, bool isLiveData, ListResult<T> data)
        {
            base.ExecuteMethod("OnMoreDataRetrieved", delegate ()
            {
                if (isLiveData || this.AllowPagedStaleData)
                {
                    if (this.DataTrackerMore != this.DataTrackerRefresh || this.DataTrackerMore != requestToken)
                    {
                        // we've changed, don't add
                        return;
                    }
                    if (data != null && data.stepping != null)
                    {
                        this.HasMoreData = data.stepping.more;
                        this.NextStart = (int)data.stepping.skip;
                    }
                    if (data != null && data.paging != null)
                    {
                        this.HasMoreData = data.paging.total_pages > data.paging.current_page;
                        this.NextStart = (int)data.paging.current_page + 1;
                    }
                    if (data != null && data.items != null)
                    {
                        this.Data.AddRange(data.items);
                    }
                    this.DataViewModelView.AddData(data.items);
                }
            });
        }

        #endregion

    }
}
