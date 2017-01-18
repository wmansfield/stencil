using System;
using System.Threading.Tasks;
using UIKit;

namespace Stencil.Native.Core
{
    public partial interface IViewPlatform
    {
        UINavigationController GetRootNavigationController();
        UIViewController GetRootViewController();
        void ExecuteMethodOnMainThread(string name, Action method);
    }
}

