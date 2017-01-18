using System;
using System.Collections.Generic;

namespace Stencil.Native.Droid.Core.UI
{
    public interface IMultiViewHost
    {
        event EventHandler<LayoutZoneChangedArgs> LayoutZoneChanged;
        bool Show(BaseFragment fragment, string tag = "");
        void ShowMain();
        Dictionary<int, BaseFragment> VisibleFragments { get; }
    }
}

