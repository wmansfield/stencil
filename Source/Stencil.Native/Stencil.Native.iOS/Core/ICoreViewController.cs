using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;

namespace Stencil.Native.iOS.Core
{
    public interface ICoreViewController
    {
        event EventHandler ViewWillDissappear;
        bool SupressRefreshDataOnAppear { get; set; }
        void PresentViewControllerWithDisposeOnReturn(UIViewController controller, bool animated, Action completion);
        void PushViewControllerWithDisposeOnReturn(UIViewController controller, bool animated);
        UIViewController ViewControllerToDiposeOnAppear { get; set; }
    }
}