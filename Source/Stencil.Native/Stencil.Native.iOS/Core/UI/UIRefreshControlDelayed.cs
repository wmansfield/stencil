using System;
using UIKit;
using Foundation;
using CoreGraphics;
using System.Threading.Tasks;
using Stencil.Native.Core;

namespace Stencil.Native.iOS.Core.UI
{
    public class UIRefreshControlDelayed : UIRefreshControl
    {
        public UIRefreshControlDelayed() 
            : base()
        {
            this.MillisecondDelayStart = 350;
            this.DisableScrollFix = true;
        }
        public UIRefreshControlDelayed(IntPtr handle) 
            : base(handle)
        {
            this.MillisecondDelayStart = 350;
            this.DisableScrollFix = true;
        }

        public UIRefreshControlDelayed(NSObjectFlag t) 
            : base(t)
        {
            this.MillisecondDelayStart = 350;
            this.DisableScrollFix = true;
        }
        public UIRefreshControlDelayed(NSCoder coder) 
            : base(coder)
        {
            this.MillisecondDelayStart = 350;
            this.DisableScrollFix = true;
        }

        private bool _shouldStartRefresh;

        public UIScrollView ScrollView { get; set; }
        public int MillisecondDelayStart { get; set; }

        /// <summary>
        /// Can't remember what this is for, but surely its years old
        /// </summary>
        public bool DisableScrollFix { get; set; }

        public override void BeginRefreshing()
        {
            if(this.DisableScrollFix)
            {
                base.BeginRefreshing();
                return;
            }
            CoreUtility.ExecuteMethodAsync("BeginRefreshing", async delegate()
            {
                _shouldStartRefresh = true;
                await Task.Delay(this.MillisecondDelayStart);
                if(_shouldStartRefresh)
                {
                    base.BeginRefreshing();
                    UIScrollView tableViewToAdjust = this.ScrollView;
                    if (tableViewToAdjust != null && tableViewToAdjust.ContentOffset.Y == 0) 
                    {
                        UIView.Animate(.25, delegate() 
                        {
                            tableViewToAdjust.ContentOffset = new CGPoint(0, - this.Frame.Size.Height);
                        });
                    }
                }
            });
        }
        public override void EndRefreshing()
        {
            _shouldStartRefresh = false;
            base.EndRefreshing();
        }
    }
}

