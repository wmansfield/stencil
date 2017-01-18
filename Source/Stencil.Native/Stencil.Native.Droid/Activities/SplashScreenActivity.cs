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
using Android.Content.PM;
using Stencil.Native.Droid.Core;

namespace Stencil.Native.Droid.Activities
{
    [Activity(MainLauncher = true, LaunchMode = LaunchMode.SingleInstance, Theme = "@style/Theme.Stencil", Label = "@string/app_name", ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashScreenActivity : BaseActivity
    {
        public SplashScreenActivity()
            : base("SplashScreenActivity")
        {
        }

        protected bool _hasShown;
        private Bundle _initialBundle;

        #region Overrides

        protected override void OnCreate(Bundle bundle)
        {
            base.ExecuteMethod("OnCreate", delegate ()
            {
                base.OnCreate(bundle);


                this.SetContentView(Resource.Layout.SplashScreen);


                _initialBundle = this.GetInitialBundle();
            });
        }

        protected override void OnPostResume()
        {
            base.ExecuteMethod("OnPostResume", delegate ()
            {
                base.OnPostResume();

                if (this.StencilApp.CurrentAccount != null)
                {
                    //TODO:MUST: Launch proper initial activity
                    this.StartActivity<LoginActivity>(true, _initialBundle);
                }
                else
                {
                    this.StartActivity<LoginActivity>(true, _initialBundle);
                }
            });
        }


        #endregion

    }
}