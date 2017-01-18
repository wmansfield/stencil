using System;
using UIKit;

namespace Stencil.Native.iOS.Core.UI
{
    public class SimultaneousGestureRecognizerDelegate : UIGestureRecognizerDelegate
    {
        public SimultaneousGestureRecognizerDelegate()
        {
        }
        public override bool ShouldRecognizeSimultaneously(UIGestureRecognizer gestureRecognizer, UIGestureRecognizer otherGestureRecognizer)
        {
            return true;
        }
    }
}

