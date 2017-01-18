using System;
using Android.Content;
using Stencil.Native.Droid.Core.UI;

namespace Stencil.Native.Core
{
    public partial interface IViewPlatform
    {
        object RecentView { get; set; }
        IMultiViewHost RecentMultiViewHost { get; set; }
        Context Context { get; set; }
        void ShowOutDatedMessageIfNeeded(Context context);
    }
}

