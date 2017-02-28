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
    [Register ("RemarkCreateController")]
    partial class RemarkCreateController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIBarButtonItem btnCreate { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView txtRemark { get; set; }

        [Action ("BtnCreate_Activated:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void BtnCreate_Activated (UIKit.UIBarButtonItem sender);

        void ReleaseDesignerOutlets ()
        {
            if (btnCreate != null) {
                btnCreate.Dispose ();
                btnCreate = null;
            }

            if (txtRemark != null) {
                txtRemark.Dispose ();
                txtRemark = null;
            }
        }
    }
}