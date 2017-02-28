using Foundation;
using System;
using UIKit;
using Stencil.Native.iOS.Core;
using Stencil.Native.Core;
using Stencil.SDK.Models;
using CoreGraphics;

namespace Stencil.Native.iOS
{
    public partial class CellPostText : BaseUITableViewCell
    {
        public CellPostText(IntPtr handle) : base (handle)
        {
            this.TrackPrefix = "CellPostText";
        }

        public const string IDENTIFIER = "CellPostText";

        public static float CalculateCellHeight(Post item)
        {
            return CoreUtility.ExecuteFunction("CellPostText.CalculateCellHeight", delegate ()
            {
                float height = 0;

                float below = 5;
                float above = 5;
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

        public void BindData(Post item)
        {
            base.ExecuteMethod("BindData", delegate ()
            {
                lblText.Text = item.body;
            });
        }
    }
}
