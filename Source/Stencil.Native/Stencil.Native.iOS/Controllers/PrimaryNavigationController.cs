using Foundation;
using System;
using UIKit;

namespace Stencil.Native.iOS
{
    public partial class PrimaryNavigationController : UINavigationController
    {
        public PrimaryNavigationController (IntPtr handle) : base (handle)
        {
        }

        public const string IDENTIFIER = "PrimaryNavigationController";
    }
}