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
    [Register ("CellPost")]
    partial class CellPost
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblAccount { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblComments { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblPost { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (lblAccount != null) {
                lblAccount.Dispose ();
                lblAccount = null;
            }

            if (lblComments != null) {
                lblComments.Dispose ();
                lblComments = null;
            }

            if (lblPost != null) {
                lblPost.Dispose ();
                lblPost = null;
            }
        }
    }
}