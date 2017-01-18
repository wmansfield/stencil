using System;
using Android.Text;
using Android.Views;
using Android.Text.Style;
using Android.Graphics;
using Stencil.Native.Core;

namespace Stencil.Native.Droid
{
    public static class _SpannableStringExtensions
    {
        public static void SetClickableSpan(this SpannableString spannableString, string allText, string clickableText, Action<CoreClickableSpan, View> onClick, string argument, Color? textColor = null, bool hideUnderline = false)
        {
            CoreUtility.ExecuteMethod("SetClickableSpan", delegate()
            {
                if(!textColor.HasValue)
                {
                    textColor = Color.Black;
                }
                int ix = allText.IndexOf(clickableText);
                CoreClickableSpan clickableSpan = new CoreClickableSpan(onClick, argument, clickableText);
                clickableSpan.HideUnderline = hideUnderline;
                while(ix >= 0)
                {
                    spannableString.SetSpan(clickableSpan, ix, ix + clickableText.Length, SpanTypes.ExclusiveExclusive);
                    spannableString.SetSpan(new StyleSpan(Android.Graphics.TypefaceStyle.Bold), ix, ix + clickableText.Length, SpanTypes.ExclusiveExclusive);
                    spannableString.SetSpan(new ForegroundColorSpan(textColor.Value), ix, ix + clickableText.Length, SpanTypes.ExclusiveExclusive);  
                    int nextIndex = allText.Substring(ix + clickableText.Length).IndexOf(clickableText);
                    if(nextIndex >= 0)
                    {
                        ix = nextIndex + (ix + clickableText.Length);
                    }
                    else
                    {
                        ix = -1; // break out
                    }
                }
            });

        }
        public static void SetStyleSpan(this SpannableString spannableString, string allText, string styleText, TypefaceStyle style, Color textColor, Typeface typeFace = null, float typeFaceSize = 0)
        {
            CoreUtility.ExecuteMethod("SetStyleSpan", delegate()
            {
                int ix = allText.IndexOf(styleText);
                while(ix >= 0)
                {
                    spannableString.SetSpan(new StyleSpan(style), ix, ix + styleText.Length, SpanTypes.ExclusiveExclusive);
                    if(textColor != Color.Transparent)
                    {
                        spannableString.SetSpan(new ForegroundColorSpan(textColor), ix, ix + styleText.Length, SpanTypes.ExclusiveExclusive);
                    }
                    if(typeFace != null)
                    {
                        spannableString.SetSpan(new CustomTypefaceSpan(typeFace, typeFaceSize), ix, ix + styleText.Length, SpanTypes.ExclusiveExclusive);  
                    }
                    int nextIndex = allText.Substring(ix + styleText.Length).IndexOf(styleText);
                    if(nextIndex >= 0)
                    {
                        ix = nextIndex + (ix + styleText.Length);
                    }
                    else
                    {
                        ix = -1; // break out
                    }
                }
            });
        }

        public static void SetEmptyLineSpace(this SpannableString spannableString, string allText, int size = 8)
        {
            CoreUtility.ExecuteMethod("SetNewLineSpace", delegate()
            {
                string newLine = "\n \n";
                int ix = allText.IndexOf(newLine);

                while(ix >= 0)
                {
                    spannableString.SetSpan(new AbsoluteSizeSpan(size), ix, ix + newLine.Length, SpanTypes.ExclusiveExclusive);
                    int nextIndex = allText.Substring(ix + newLine.Length).IndexOf(newLine);
                    if(nextIndex >= 0)
                    {
                        ix = nextIndex + (ix + newLine.Length);
                    }
                    else
                    {
                        ix = -1; // break out
                    }
                }
            });

        }
    }
}

