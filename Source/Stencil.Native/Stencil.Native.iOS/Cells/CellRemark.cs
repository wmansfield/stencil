using Foundation;
using System;
using UIKit;
using Stencil.Native.iOS.Core;
using Stencil.SDK.Models;
using Stencil.Native.Core;
using CoreGraphics;

namespace Stencil.Native.iOS
{
    public partial class CellRemark : BaseUITableViewCell
    {
        public CellRemark(IntPtr handle) : base(handle)
        {
            this.TrackPrefix = "CellRemark";
        }

        public const string IDENTIFIER = "CellRemark";

        public static float CalculateCellHeight(Remark item)
        {
            return CoreUtility.ExecuteFunction("CellRemark.CalculateCellHeight", delegate ()
            {
                float height = 0;

                float below = 30;
                float above = 15;
                if(!item.TagExists("totalHeight"))
                {
                    float bodySize = 0f;

                    if(!item.TagExists("bodySize"))
                    {
                        if(!string.IsNullOrWhiteSpace(item.text))
                        {
                            CGRect boudingRect = new NSString(item.text.Trim()).GetBoundingRect(new CGSize(290.SizingScaleFix(), 500), NSStringDrawingOptions.UsesLineFragmentOrigin | NSStringDrawingOptions.UsesFontLeading, new UIStringAttributes() { Font = UIFont.SystemFontOfSize(17) }, new NSStringDrawingContext());
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
        private Remark _data;

        private void EnsureInitialized()
        {
            base.ExecuteMethod("EnsureInitialized", delegate ()
            {
                if(_initialized) { return; }

                _initialized = true;

            });
        }

        public void BindData(UIViewController controller, Remark item)
        {
            base.ExecuteMethod("BindData", delegate ()
            {
                this.EnsureInitialized();

                _controller = controller;
                _data = item;

                lblPost.Text = item.text;
                //lblAccount.Text = item.account_name; //TODO: Add in to code!
            });
        }

    }
}
