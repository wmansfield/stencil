using System;
using Android.Content;
using Android.Runtime;
using Android.Support.V4.View;
using Android.Util;

namespace Stencil.Native.Droid
{
    public class CoreViewPager : ViewPager
    {
        public CoreViewPager(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
        }

        public CoreViewPager(Context context)
            : base(context)
        {
        }

        protected CoreViewPager(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public bool DisableSwipePaging { get; set; }

        public override bool OnTouchEvent(Android.Views.MotionEvent e)
        {
            return !DisableSwipePaging && base.OnTouchEvent(e);
        }
        public override bool OnInterceptTouchEvent(Android.Views.MotionEvent ev)
        {
            return !DisableSwipePaging && base.OnInterceptTouchEvent(ev);
        }
        public override bool CanScrollHorizontally(int direction)
        {
            return !DisableSwipePaging && base.CanScrollHorizontally(direction);
        }
    }
}

