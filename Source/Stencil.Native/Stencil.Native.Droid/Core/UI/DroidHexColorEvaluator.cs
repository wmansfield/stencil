using System;
using Android.Runtime;
using Android.Animation;
using Stencil.Native.Core;

namespace Stencil.Native.Droid.Core.UI
{
    public class DroidHexColorEvaluator : ArgbEvaluator
    {
        public DroidHexColorEvaluator()
            :base()
        {
        }
        public DroidHexColorEvaluator(IntPtr handle, JniHandleOwnership transfer)       
            :base(handle, transfer)
        {
        }

        public override Java.Lang.Object Evaluate(float fraction, Java.Lang.Object startValue, Java.Lang.Object endValue)
        {
            return CoreUtility.ExecuteFunction<Java.Lang.Object>("Evaluate", delegate()
            {
                Java.Lang.Object result;

                int startInt = Convert.ToInt32(startValue);
                int startA = (startInt >> 24) & 0xff;
                int startR = (startInt >> 16) & 0xff;
                int startG = (startInt >> 8) & 0xff;
                int startB = startInt & 0xff;

                int endInt = Convert.ToInt32(endValue);
                int endA = (endInt >> 24) & 0xff;
                int endR = (endInt >> 16) & 0xff;
                int endG = (endInt >> 8) & 0xff;
                int endB = endInt & 0xff;

                result = ((startA + (int)(fraction * (endA - startA))) << 24) |
                    ((startR + (int)(fraction * (endR - startR))) << 16) |
                    ((startG + (int)(fraction * (endG - startG))) << 8) |
                    ((startB + (int)(fraction * (endB - startB))));

                return result;
            });
        }
    }
}

