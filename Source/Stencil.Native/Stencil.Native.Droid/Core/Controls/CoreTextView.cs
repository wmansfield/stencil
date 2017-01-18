using System;
using Android.Runtime;
using Android.Content;
using Android.Util;
using Android.Graphics;
using Android.Widget;
using System.Collections.Generic;
using Android.Content.Res;
using Android.Support.V7.Widget;
using Stencil.Native.Core;
using Stencil.Native.Droid.Core;

namespace Stencil.Native.Droid
{
    public class CoreTextView : AppCompatTextView
    {
        protected CoreTextView(IntPtr javaReference, JniHandleOwnership transfer) 
            : base(javaReference, transfer)
        {
        }
        public CoreTextView(Context context) 
            : base(context)
        {
        }
        public CoreTextView(Context context, IAttributeSet attrs) 
            : base(context, attrs)
        {
            TypedArray array = context.ObtainStyledAttributes(attrs, Resource.Styleable.CustomFont, 0, 0);
            this.CustomFontPath = array.GetString(Resource.Styleable.CustomFont_customFont);
            array.Recycle();

            SetCustomFont(this.CustomFontPath);
        }
        public CoreTextView(Context context, IAttributeSet attrs, int defStyle) 
            : base(context, attrs, defStyle)
        {
            TypedArray array = context.ObtainStyledAttributes(attrs, Resource.Styleable.CustomFont, defStyle, 0);
            this.CustomFontPath = array.GetString(Resource.Styleable.CustomFont_customFont);
            array.Recycle();

            SetCustomFont(this.CustomFontPath);

        }


        public string CustomFontPath { get; protected set; }

        public void SetCustomFont(string assetPath)
        {
            try
            {
                this.CustomFontPath = assetPath;
                if(!string.IsNullOrEmpty(assetPath))
                {
                    Typeface face = FontLoader.GetFont(Context.Assets, assetPath);
                    if (face != null)
                    {
                        TypefaceStyle style = TypefaceStyle.Normal;
                        //Not all custom fonts have compatible text sytle, so we ignore it
                        /*
                        if (this.Typeface != null) //Takes care of android:textStyle=""
                        {    
                            style = Typeface.Style;
                        }
                        */
                        this.SetTypeface(face, style);
                    }
                }
            }
            catch(Exception ex)
            {
                Container.Track.LogWarning(ex.Message, "CoreTextView");
            }
        }

    }
}

