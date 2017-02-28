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

namespace Stencil.Native.Droid.Activities
{
    [Activity(Label = "@string/app_name", ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/Theme.Stencil")]
    public class PostCreateActivity : BaseActivity, IViewModelView
    {
        public PostCreateActivity()
            : base("PostCreateActivity")
        {
        }

        #region Controls

        protected EditText txtBody { get { return this.GetControl<EditText>(Resource.Id.text_input); } }

        protected TextView btnBack { get { return this.GetControl<TextView>(Resource.Id.btn_back); } }
        protected TextView lblHeader { get { return this.GetControl<TextView>(Resource.Id.general_h1); } }
        protected TextView btnCreate { get { return this.GetControl<TextView>(Resource.Id.btn_create); } }

        #endregion

        #region Properties

        public PostCreateViewModel ViewModel { get; set; }

        #endregion

        #region Overrides

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.ExecuteMethod("OnCreate", delegate ()
            {
                base.OnCreate(savedInstanceState);

                this.ViewModel = new PostCreateViewModel(this);

                this.SetContentView(Resource.Layout.PostCreate);


                lblHeader.Text = this.ViewModel.Text_Title;

                btnBack.Click += btnBack_Click;
                btnCreate.Click += btnCreate_Click;


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

        #endregion

        #region Protected Methods


        protected void DoAddPost()
        {
            base.ExecuteMethodAsync("DoAddPost", async delegate ()
            {
                this.HideKeyboard();

                if(!this.IsValid())
                {
                    return; // pre-validate before we start processing
                }

                HUD.Show(this, this.ViewModel.Text_General_Sending);

                ActionResult response = await this.ViewModel.CreatePostAsync(txtBody.Text);

                if(response.IsSuccess())
                {
                    HUD.Dismiss();
                    this.FinishWithAnimation();
                }
                else
                {
                    HUD.ShowErrorWithStatus(this, response.GetMessage(), 2800);
                }
            });
        }
        protected bool IsValid()
        {
            return base.ExecuteFunction("IsValid", delegate ()
            {
                string errorMessage = this.ViewModel.Validate(txtBody.Text);
                if(!string.IsNullOrEmpty(errorMessage))
                {
                    HUD.ShowErrorWithStatus(this, errorMessage, 2800);
                    return false;
                }
                return true;
            });
        }

        #endregion

        #region Event Handlers


        private void btnBack_Click(object sender, EventArgs e)
        {
            base.ExecuteMethod("btnBack_Click", delegate ()
            {
                this.FinishWithAnimation();
            });
        }
        private void btnCreate_Click(object sender, EventArgs e)
        {
            base.ExecuteMethod("btnCreate_Click", delegate ()
            {
                this.DoAddPost();
            });
        }
        #endregion
    }
}