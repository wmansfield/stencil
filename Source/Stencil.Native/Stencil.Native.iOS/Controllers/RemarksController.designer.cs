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
    [Register ("RemarksController")]
    partial class RemarksController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIBarButtonItem btnAdd { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView tblData { get; set; }

        [Action ("BtnAdd_Activated:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void BtnAdd_Activated (UIKit.UIBarButtonItem sender);

        void ReleaseDesignerOutlets ()
        {
            if (btnAdd != null) {
                btnAdd.Dispose ();
                btnAdd = null;
            }

            if (tblData != null) {
                tblData.Dispose ();
                tblData = null;
            }
        }
    }
}