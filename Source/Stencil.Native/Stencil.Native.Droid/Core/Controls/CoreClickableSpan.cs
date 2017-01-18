using System;
using Android.Text.Style;
using Android.Views;
using Android.Text;
using Android.Graphics;
using Android.Content;

namespace Stencil.Native.Droid
{
    public class CoreClickableSpan : ClickableSpan
    {
        public CoreClickableSpan()
        {
        }
        public CoreClickableSpan(Action<CoreClickableSpan, View> click, string argument, string text)
        {
            this.Click = click;
            this.Argument = argument;
            this.Text = text;
        }
        public Action<CoreClickableSpan, View> Click;
        public bool HideUnderline { get; set; }
        public string Argument { get; set; }
        public string Text { get; set; }
        public override void UpdateDrawState(TextPaint ds)
        {
            base.UpdateDrawState(ds);
            if (HideUnderline)
            {
                ds.UnderlineText = false;
            }
        }

        public override void OnClick(View widget)
        {
            if (Click != null)
            {
                Click(this, widget);
            }
        }

    }
}

