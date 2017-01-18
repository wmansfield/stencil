using System;
using CoreGraphics;
using UIKit;

namespace Stencil.Native.iOS.Core.UI
{
    public class TopBubbleMark : UIView
    {
        public TopBubbleMark(CGRect frame)
            : base(frame)
        {
            this.BackgroundColor = UIColor.Clear;
        }


        public const float WIDTH = 22f;
        public const float HEIGHT = 14f;
        public const float WH_RATIO = WIDTH / HEIGHT;
        public const float HW_RATIO = HEIGHT / WIDTH;

        public float Width { get; set; }
        public float Height { get; set; }
        public float _scale;

        public UIColor FillColor { get; set; }
        public override CGRect Frame
        {
            get
            {
                return base.Frame;
            }
            set
            {
                base.Frame = value;
                this.SetDimensions(this.Frame.Size);
            }
        }
        public override CGRect Bounds
        {
            get
            {
                return base.Bounds;
            }
            set
            {
                base.Bounds = value;
                this.SetDimensions(this.Bounds.Size);
            }
        }
        private CGSize getOriginalCanvasSize()
        {
            return new CGSize(WIDTH, HEIGHT);
        }

        public void SetDimensions(CGSize size)
        {
            this.Width = (float)size.Width;
            this.Height = (float)size.Height;
            this.Reset();
        }

        private void Reset()
        {
            // Scale to fit drawing inside the available frame
            float newScale = this.GetScaleToFit();
            if (newScale != _scale)
            {
                _scale = newScale;
                this.SetNeedsDisplay();
            }
        }

        private float GetScaleToFit()
        {
            float orgWidth = WIDTH;
            float orgHeight = HEIGHT;
            return Math.Min((this.Width / orgWidth), (this.Height / orgHeight));
        }

        public override void Draw(CGRect rect)
        {
            this.DrawToContext(UIGraphics.GetCurrentContext());
        }

        public void DrawToContext(CGContext ctx)
        {
            if (_scale == 0)
            {
                return;
            }


            ctx.ScaleCTM(_scale, _scale);
            if (_scale != this.GetScaleToFit())
            {
                ctx.TranslateCTM(((this.Width - (this.Width * _scale)) / 2), ((this.Height - (this.Height * _scale)) / 2));
            }



            ctx.SaveState();

            // float WIDTH = 22f;
            //pub    HEIGHT = 14f;

            UIColor color = this.FillColor;
            if (color == null)
            {
                color = UIColor.Gray;
            }
            // triangle
            ctx.SaveState();
            ctx.SetShouldAntialias(true);
            ctx.SetLineCap(CGLineCap.Butt);
            ctx.SetFillColor(color.CGColor);
            ctx.MoveTo(0, 14f);
            ctx.AddLineToPoint(11f, 0);
            ctx.AddLineToPoint(22f, 14f);
            ctx.ClosePath();
            ctx.FillPath();
            ctx.RestoreState();


            ctx.RestoreState();
        }
    }
}

