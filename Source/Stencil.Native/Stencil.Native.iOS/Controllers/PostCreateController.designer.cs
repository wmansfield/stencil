// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace Stencil.Native.iOS
{
    [Register ("PostCreateController")]
    partial class PostCreateController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIBarButtonItem btnCreate { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView txtPost { get; set; }

        [Action ("BtnCreate_Activated:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void BtnCreate_Activated (UIKit.UIBarButtonItem sender);

        void ReleaseDesignerOutlets ()
        {
            if (btnCreate != null) {
                btnCreate.Dispose ();
                btnCreate = null;
            }

            if (txtPost != null) {
                txtPost.Dispose ();
                txtPost = null;
            }
        }
    }
}