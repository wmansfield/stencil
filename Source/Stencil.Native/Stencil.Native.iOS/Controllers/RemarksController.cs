using Foundation;
using System;
using UIKit;
using Stencil.Native.iOS.Core;
using Stencil.Native.Core;
using Stencil.Native.ViewModels;
using Stencil.SDK.Models;
using Stencil.Native.iOS.Core.Data;
using CoreGraphics;
using System.Collections.Generic;

namespace Stencil.Native.iOS
{
    public partial class RemarksController : BaseUIViewController, IDataListViewModelView<Remark>
    {
        

        #region Constructor

        public RemarksController(IntPtr handle)
            : base(handle)
        {
            this.TrackPrefix = "RemarksController";
        }

        #endregion

        #region Properties

        public const string IDENTIFIER = "RemarksController";

        public RemarksViewModel ViewModel { get; set; }
        public Post Route { get; set; }

        private CoreFlexibleTableSource _dataSource;
        private UIRefreshControl _refreshControl;

        #endregion

        #region Overrides


        public override void ViewDidLoad()
        {
            base.ExecuteMethod("ViewDidLoad", delegate ()
            {
                base.ViewDidLoad();

                this.ViewModel = new RemarksViewModel(this, this.Route);

                this.NavigationController.NavigationBar.TopItem.Title = ""; // back button text
                this.Title = this.ViewModel.Text_Title;

                _refreshControl = this.InjectRefreshControl(this.tblData, Refresh_Requested);

                tblData.TableFooterView = new UIView(); // when blank, removes empty lines

                this.ViewModel.Start();
            });
        }
        public override void ViewWillAppear(bool animated)
        {
            base.ExecuteMethod("ViewWillAppear", delegate ()
            {
                base.ViewWillAppear(animated);

                this.Title = this.ViewModel.Text_Title;
            });

        }
        public override void ViewDidAppear(bool animated)
        {
            base.ExecuteMethod("ViewDidAppear", delegate ()
            {
                base.ViewDidAppear(animated);

                this.ViewModel.OnAppear();
            });
        }
        public override void ViewDidDisappear(bool animated)
        {
            base.ExecuteMethod("ViewDidDisappear", delegate ()
            {
                base.ViewDidDisappear(animated);

                this.ViewModel.OnDisappear();
            });
        }

        public override void ScrollToTop()
        {
            base.ExecuteMethod("ScrollToTop", delegate ()
            {
                tblData.SetContentOffset(new CGPoint(0, 0), true);
            });
        }

        #endregion

        #region Data Methods

        public void BindData(bool live_data, List<Remark> data)
        {
            base.ExecuteMethodOnMainThread("BindData", delegate ()
            {
                if(_dataSource == null)
                {
                    _dataSource = new CoreFlexibleTableSource()
                        .WhenCountingFlexibleRows(this.CountRowsInSection)
                        .WhenCreatingFlexibleCell(this.CellCreate)
                        .WhenSizingFlexibleRows(this.CellSize);
                }

                if(this.ViewModel.HasMoreData)
                {
                    _dataSource.ScrollListener = new InfiniteScroll(tblData, this.ViewModel.DoGetMoreData, this.ViewModel.ScrollThresholdSize);
                }
                else
                {
                    _dataSource.ScrollListener = null;
                }

                tblData.Source = _dataSource;
                tblData.ReloadData();
            });
        }
        public void AddData(List<Remark> data)
        {
            base.ExecuteMethodOnMainThread("AddData", delegate ()
            {
                if(_dataSource.ScrollListener != null)
                {
                    _dataSource.ScrollListener.ListeningDisabled = !this.ViewModel.HasMoreData;
                }

                tblData.Source = _dataSource;
                tblData.ReloadData();
            });
        }
        protected nint CountRowsInSection(nint section)
        {
            return base.ExecuteFunction("CountRowsInSection", delegate ()
            {
                return this.ViewModel.Data.Count;
            });
        }
        protected UITableViewCell CellCreate(NSIndexPath path)
        {
            return base.ExecuteFunction<UITableViewCell>("CellCreate", delegate ()
            {
                Remark item = this.ViewModel.Data[path.Row];
                switch(item.ui_token)
                {
                    case RemarksViewModel.TOKEN_TEXT:
                        CellRemarkText textCell = tblData.DequeueReusableCell(CellRemarkText.IDENTIFIER, path) as CellRemarkText;
                        textCell.BindData(item);
                        textCell.SetDefaultInsets();
                        return textCell;
                    case RemarksViewModel.TOKEN_POST:
                    default:
                        CellRemark remarkCell = tblData.DequeueReusableCell(CellRemark.IDENTIFIER, path) as CellRemark;
                        remarkCell.BindData(this, item);
                        remarkCell.SetDefaultInsets();
                        return remarkCell;
                }

            });
        }

        protected nfloat CellSize(NSIndexPath path)
        {
            return base.ExecuteFunction("CellSize", delegate ()
            {
                Remark item = this.ViewModel.Data[path.Row];
                switch(item.ui_token)
                {
                    case RemarksViewModel.TOKEN_TEXT:
                        return CellRemarkText.CalculateCellHeight(item);
                    case RemarksViewModel.TOKEN_POST:
                    default:
                        return CellRemark.CalculateCellHeight(item);
                }

            });
        }

        public void OnDataRefreshing(bool refreshing)
        {
            base.ExecuteMethodOnMainThread("OnDataRefreshing", delegate ()
            {
                if(refreshing)
                {
                    _refreshControl.BeginRefreshing();
                }
                else
                {
                    _refreshControl.EndRefreshing();
                }
            });
        }
        public void OnDataGettingMore(bool gettingMore)
        {
            // direct cell callback uses
        }


        #endregion

        #region Protected Methods

        protected void NavigateToRemarks()
        {
            base.ExecuteMethod("NavigateToRemarks", delegate ()
            {
                RemarkCreateController remarkController = this.Storyboard.InstantiateViewController(RemarkCreateController.IDENTIFIER) as RemarkCreateController;
                remarkController.Route = this.Route;
                this.NavigationController.PushViewController(remarkController, true);

            });
        }

        #endregion


        #region Event Handlers

        partial void BtnAdd_Activated(UIBarButtonItem sender)
        {
            this.NavigateToRemarks();
        }

        protected void Refresh_Requested(object sender, EventArgs e)
        {
            base.ExecuteMethod("Refresh_Requested", delegate ()
            {
                _refreshControl.EndRefreshing(); //We have our own
                this.ViewModel.DoRefreshData(true);
            });
        }

        #endregion



    }
}
