using Foundation;
using System;
using UIKit;
using Stencil.Native.iOS.Core;
using Stencil.SDK;
using CoreGraphics;
using Stencil.SDK.Models;
using Stencil.Native.ViewModels;

namespace Stencil.Native.iOS
{
    public partial class RemarkCreateController : BaseUIViewController, IViewModelView
    {
        

        #region Constructor

        public RemarkCreateController(IntPtr handle)
            : base(handle)
        {
            this.TrackPrefix = "RemarkCreateController";
        }

        #endregion

        #region Properties

        public const string IDENTIFIER = "RemarkCreateController";

        public RemarkCreateViewModel ViewModel { get; set; }
        public Post Route { get; set; }

        #endregion

        #region Overrides


        public override void ViewDidLoad()
        {
            base.ExecuteMethod("ViewDidLoad", delegate ()
            {
                base.ViewDidLoad();

                this.ViewModel = new RemarkCreateViewModel(this, Route);

                this.NavigationController.NavigationBar.TopItem.Title = ""; // back button text
                this.Title = this.ViewModel.Text_Title;

                this.ViewModel.Start();

                txtRemark.BecomeFirstResponder();
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

        #endregion


        #region Protected Methods

        protected void DoAddPost()
        {
            base.ExecuteMethodAsync("DoAddPost", async delegate ()
            {
                this.View.EndEditing(true);

                if(!this.IsValid())
                {
                    return; // pre-validate before we start processing
                }

                HUD.Show(this.ViewModel.Text_General_Sending);

                ActionResult response = await this.ViewModel.CreateRemarkAsync(txtRemark.Text);

                if(response.IsSuccess())
                {
                    HUD.Dismiss();
                    this.NavigationController.PopViewController(true);
                }
                else
                {
                    HUD.ShowErrorWithStatus(response.GetMessage(), 2800);
                }
            });
        }
        protected bool IsValid()
        {
            return base.ExecuteFunction("IsValid", delegate ()
            {
                string errorMessage = this.ViewModel.Validate(txtRemark.Text);
                if(!string.IsNullOrEmpty(errorMessage))
                {
                    HUD.ShowErrorWithStatus(errorMessage, 2800);
                    return false;
                }
                return true;
            });
        }


        #endregion

        #region Event Handlers

        partial void BtnCreate_Activated(UIBarButtonItem sender)
        {
            this.DoAddPost();
        }

        #endregion



    }
}
