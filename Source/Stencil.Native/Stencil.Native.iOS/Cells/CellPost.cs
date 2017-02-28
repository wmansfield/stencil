using Foundation;
using System;
using UIKit;
using Stencil.Native.iOS.Core;
using Stencil.SDK.Models;
using Stencil.Native.Core;
using CoreGraphics;

namespace Stencil.Native.iOS
{
    public partial class CellPost : BaseUITableViewCell
    {
        public CellPost(IntPtr handle) : base (handle)
        {
            this.TrackPrefix = "CellPost";
        }

        public const string IDENTIFIER = "CellPost";

        public static float CalculateCellHeight(Post item)
        {
            return CoreUtility.ExecuteFunction("CellPost.CalculateCellHeight", delegate ()
            {
                float height = 0;

                float below = 30;
                float above = 15;
                if(!item.TagExists("totalHeight"))
                {
                    float bodySize = 0f;

                    if(!item.TagExists("bodySize"))
                    {
                        if(!string.IsNullOrWhiteSpace(item.body))
                        {
                            CGRect boudingRect = new NSString(item.body.Trim()).GetBoundingRect(new CGSize(290.SizingScaleFix(), 500), NSStringDrawingOptions.UsesLineFragmentOrigin | NSStringDrawingOptions.UsesFontLeading, new UIStringAttributes() { Font = UIFont.SystemFontOfSize(17) }, new NSStringDrawingContext());
                            bodySize = 2 + (int)Math.Ceiling((double)(boudingRect.Size.Height));
                        }
                        item.TagSet("bodySize", bodySize.ToString());
                    }
                    else
                    {
                        bodySize = item.TagGetAsInt("bodySize", 0);
                    }

                    height = above + bodySize + below;

                    item.TagSet("totalHeight", height.ToString());
                }
                else
                {
                    height = item.TagGetAsInt("totalHeight", 31);
                }

                return height;
            });
        }

        private bool _initialized;
        private UIViewController _controller;
        private Post _data;

        private void EnsureInitialized()
        {
            base.ExecuteMethod("EnsureInitialized", delegate ()
            {
                if(_initialized) { return; }

                _initialized = true;
                lblComments.AddGestureRecognizer(new UITapGestureRecognizer(NavigateToRemarks) { NumberOfTapsRequired = 1 });
                lblComments.UserInteractionEnabled = true;

            });
        }

        public void BindData(UIViewController controller, Post item)
        {
            base.ExecuteMethod("BindData", delegate ()
            {
                this.EnsureInitialized();

                _controller = controller;
                _data = item;

                lblPost.Text = item.body;
                lblAccount.Text = item.account_name;
                lblComments.Text = string.Format("{0} Comments", item.remark_total);
            });
        }

        protected void NavigateToRemarks()
        {
            base.ExecuteMethod("NavigateToRemarks", delegate ()
            {
                RemarksController remarksController = _controller.Storyboard.InstantiateViewController(RemarksController.IDENTIFIER) as RemarksController;
                remarksController.Route = _data;
                _controller.NavigationController.PushViewController(remarksController, true);

            });
        }
    }
}
