using System;
using Android.Content;
using Android.Runtime;
using Android.Widget;
using Android.Util;
using Android.Content.Res;
using Android.Graphics;
using Java.Lang;
using Android.Text;
using Android.Text.Style;
using Android.Support.V7.Widget;
using Stencil.Native.Droid.Core;
using Stencil.Native.Core;

namespace Stencil.Native.Droid
{
    public class CoreButton : AppCompatButton
    {
        protected CoreButton(IntPtr javaReference, JniHandleOwnership transfer) 
            : base(javaReference, transfer)
        {
        }

        public CoreButton(Context context) 
            : this(context, null)
        {
        }

        public CoreButton(Context context, IAttributeSet attrs) 
            : this(context, attrs, Android.Resource.Attribute.EditTextStyle)
        {
        }

        public CoreButton(Context context, IAttributeSet attrs, int defStyle) 
            : base(context, attrs, defStyle)
        {
            TypedArray array = context.ObtainStyledAttributes(attrs, Resource.Styleable.CustomFont, defStyle, 0);
            this.CustomFontPath = array.GetString(Resource.Styleable.CustomFont_customFont);
            array.Recycle();

            SetCustomFont(this.CustomFontPath);
        }



        public string CustomFontPath { get; set; }
        public float TextKerning { get; set; }
        public void SetCustomFont(string assetPath)
        {
            try
            {
                if(!string.IsNullOrEmpty(assetPath))
                {
                    Typeface face = FontLoader.GetFont(Context.Assets, assetPath);
                    if (face != null)
                    {
                        TypefaceStyle style = TypefaceStyle.Normal;
                        if (this.Typeface != null) //Takes care of android:textStyle=""
                        {    
                            style = Typeface.Style;
                        }
                        this.SetTypeface(face, style);
                    }
                }
            }
            catch(System.Exception ex)
            {
                Container.Track.LogWarning(ex.Message, "CoreButton");
            }
        }
    }
}

