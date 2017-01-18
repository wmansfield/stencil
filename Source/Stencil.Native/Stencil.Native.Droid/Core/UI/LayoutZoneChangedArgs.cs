using System;

namespace Stencil.Native.Droid.Core.UI
{
    public class LayoutZoneChangedArgs : EventArgs
    {
        public LayoutZoneChangedArgs()
        {
        }
        public IMultiViewHost Host { get; set; }
        public LayoutZone Zone { get; set; }
        public BaseFragment Fragment { get; set; }
    }
}

