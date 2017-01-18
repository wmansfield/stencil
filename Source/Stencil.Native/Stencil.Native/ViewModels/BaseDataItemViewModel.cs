using Stencil.SDK;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Native.ViewModels
{
    public abstract class BaseDataItemViewModel<TModel, TView> : BaseViewModel
        where TView : IDataItemViewModelView<TModel>
        where TModel : class
    {
        #region Constructor

        public BaseDataItemViewModel(TView view, string trackPrefix)
            : base(view, trackPrefix)
        {
            this.View = view;
            this.Data = default(TModel);
        }

        #endregion

        #region Public Properties

        public virtual TModel Data { get; set; }
        public new virtual TView View { get; set; }

        public virtual bool SupressRefreshDataOnAppear { get; set; }

        public virtual bool ShowNoData { get; set; }

        public DateTime? PagingStartedUtc { get; set; }

        /// <summary>
        /// internal for data management, do not use
        /// </summary>
        protected DateTime? DataTrackerPagingStarted { get; set; }
        #endregion

        #region Protected Methods

        protected virtual Guid DataTrackerRefresh { get; set; }

        #endregion

        #region Abstract Methods

        protected abstract Task PerformRefreshData(bool force);

        #endregion

        #region Public Methods

        public Task DoRefreshData(bool force, bool suppressRefreshing = false, Action<bool> onFetching = null)
        {
            return base.ExecuteMethodOrSkipAsync("DoRefreshData", async delegate ()
            {
                if (!suppressRefreshing)
                {
                    this.OnDataRefreshing(true);
                }

                if (onFetching != null)
                {
                    onFetching(true);
                }
                this.DataTrackerRefresh = Guid.NewGuid();
                this.DataTrackerPagingStarted = DateTime.UtcNow;
                if (force)
                {
                    this.PagingStartedUtc = this.DataTrackerPagingStarted;
                }

                try
                {
                    await PerformRefreshData(force);
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

        public override void OnAppear()
        {
            base.ExecuteMethod("OnAppear", delegate ()
            {
                if (!this.HasAppeared || !this.SupressRefreshDataOnAppear)
                {
                    this.DoRefreshData(!this.HasAppeared);
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
                this.View.BindData(true, this.Data);
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
                this.View.OnDataRefreshing(refreshing);
            });
        }

        protected virtual void OnDataRetrieved(bool live_data, ItemResult<TModel> data)
        {
            base.ExecuteMethod("OnDataRetrieved", delegate ()
            {
                this.Data = data.item;

                if (live_data)
                {
                    this.PagingStartedUtc = this.DataTrackerPagingStarted;
                }

                this.ShowNoData = (this.Data == default(TModel));

                this.View.BindData(live_data, this.Data);
            });
        }
        protected virtual void OnDataRetrieved(ItemResult<TModel> data)
        {
            base.ExecuteMethod("OnDataRetrieved", delegate ()
            {
                this.Data = data.item;

                this.PagingStartedUtc = this.DataTrackerPagingStarted;

                this.ShowNoData = (this.Data == default(TModel));

                this.View.BindData(true, this.Data);
            });
        }
        #endregion

    }
}
