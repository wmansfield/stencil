using System;
using UIKit;
using BigTed;

namespace Stencil.Native.iOS.Core
{
    public class HUD
    {
        public HUD()
        {
        }

        private static UIView _modal;

        public static void ShowTopToast(UIView view, float height)
        {
            HUD.Dismiss();

            UIWindow window = UIApplication.SharedApplication.KeyWindow;
            view.Frame = new CoreGraphics.CGRect(window.Bounds.X, window.Bounds.Y, window.Bounds.Width, height);
            window.AddSubview(view);
            _modal = view;
        }
        public static void ShowModal(UIView view)
        {
            HUD.Dismiss();

            UIWindow window = UIApplication.SharedApplication.KeyWindow;
            view.Frame = window.Bounds;
            window.AddSubview(view);

            _modal = view;
        }
       
        public static void ShowToast(string message, bool centered = true, double timeoutMS = 1000)
        {
            BTProgressHUD.ShowToast(message, centered, timeoutMS);
        }
        public static void Show(string message, bool underlay = true)
        {
            if (underlay)
            {
                BTProgressHUD.Show(message, -1f, ProgressHUD.MaskType.Black);
            }
            else
            {
                BTProgressHUD.Show(message, -1f, ProgressHUD.MaskType.Clear);
            }
        }
        public static void ShowSuccessWithStatus(string status, double timeoutMs)
        {
            ProgressHUD.Shared.ShowSuccessWithStatus(status, timeoutMs);
        }
        public static void ShowErrorWithStatus(string status, double timeoutMs)
        {
            BTProgressHUD.ShowErrorWithStatus(status, timeoutMs);
        }
        public static void Dismiss()
        {
            BTProgressHUD.Dismiss();

            UIView modal = _modal;
            _modal = null;
            if(modal != null && modal.Superview != null)
            {
                try
                {
                    modal.RemoveFromSuperview();
                }
                catch { }
            }
        }
    }
}

