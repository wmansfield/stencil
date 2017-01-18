using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Stencil.Native.Core;

namespace Stencil.Native.Droid
{
    public class CoreSwipeRefreshLayout : SwipeRefreshLayout
    {
        public CoreSwipeRefreshLayout(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            Initialize();
        }
        public CoreSwipeRefreshLayout(Context context)
            : base(context)
        {
            Initialize();
        }

        public Action OnRefreshRequested { get; set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.OnRefreshRequested = null;
                this.Refresh -= Self_RefreshRequested;
            }
            base.Dispose(disposing);
        }
        protected void Self_RefreshRequested(object sender, EventArgs e)
        {
            CoreUtility.ExecuteMethod("Self_RefreshRequested", delegate()
            {
                if(OnRefreshRequested != null)
                {
                    this.OnRefreshRequested();
                }
            });
        }
        private void Initialize()
        {
            CoreUtility.ExecuteMethod("Initialize", delegate()
            {
                this.Refresh += Self_RefreshRequested;
                this.SetColorSchemeColors(Resource.Color.refresh_color1,
                    Resource.Color.refresh_color2,
                    Resource.Color.refresh_color3,
                    Resource.Color.refresh_color4);
            });

        }

    }
}

