using System;
using Android.Animation;
using System.Drawing;
using System.Collections.Generic;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using System.Numerics;
using Stencil.Native.Core;

namespace Stencil.Native.Droid.Core.UI
{
    public class DroidAnimator : BaseClass
    {
        #region Constructors

        public DroidAnimator(View targetView)
            : base("DroidAnimator")
        {
            this.TargetView = targetView;
        }

        #endregion

        #region Private Properties

        private List<Animator> _animators = new List<Animator>();

        public virtual View TargetView { get; protected set; }

        #endregion

        #region DroidAnimator Implementation

        public double DefaultDurationSeconds { get; set; }
        public double? DefaultDelaySeconds { get; set; }
        public AnimationInterpolation? DefaultInterpolation { get; set; }

        public virtual DroidAnimator Begin()
        {
            return this;
        }

        public virtual DroidAnimator SetDefaultDuration(double durationSeconds)
        {
            this.DefaultDurationSeconds = durationSeconds;
            return this;
        }
        public virtual DroidAnimator SetDefaultDelay(double delaySeconds)
        {
            this.DefaultDelaySeconds = delaySeconds;
            return this;
        }
        public virtual DroidAnimator SetDefaultInterpolation(AnimationInterpolation interpolation)
        {
            this.DefaultInterpolation = interpolation;
            return this;
        }

        public virtual DroidAnimator MoveTo(PointF point)
        {
            return MoveTo(point, this.DefaultDurationSeconds, this.DefaultDelaySeconds, this.DefaultInterpolation);
        }
        public virtual DroidAnimator MoveTo(PointF point, double? durationSeconds, double? delaySeconds, AnimationInterpolation? interpolation)
        {
            return base.ExecuteFunction<DroidAnimator>("MoveTo", delegate()
            {
                PropertyValuesHolder toX = PropertyValuesHolder.OfFloat("X", point.X);                
                PropertyValuesHolder toY = PropertyValuesHolder.OfFloat("Y", point.Y);

                ObjectAnimator positionAnimator = ObjectAnimator.OfPropertyValuesHolder(this.TargetView, toX, toY);

                this.SetDefaultsFor(positionAnimator, durationSeconds, delaySeconds, interpolation);
                _animators.Add(positionAnimator);

                return this;
            });
        }
        public virtual DroidAnimator MoveTo(PointF fromPoint, PointF toPoint)
        {
            return MoveTo(fromPoint, toPoint, this.DefaultDurationSeconds, this.DefaultDelaySeconds, this.DefaultInterpolation);
        }        
        public virtual DroidAnimator MoveTo(PointF fromPoint, PointF toPoint, double? durationSeconds, double? delaySeconds, AnimationInterpolation? interpolation)
        {
            return base.ExecuteFunction<DroidAnimator>("MoveTo", delegate()
            {
                PropertyValuesHolder propX = PropertyValuesHolder.OfFloat("X", fromPoint.X, toPoint.X);
                PropertyValuesHolder propY = PropertyValuesHolder.OfFloat("Y", fromPoint.Y, toPoint.Y);

                ObjectAnimator positionAnimator = ObjectAnimator.OfPropertyValuesHolder(this.TargetView, propX, propY);
                this.SetDefaultsFor(positionAnimator, durationSeconds, delaySeconds, interpolation);
                _animators.Add(positionAnimator);

                return this;
            });
        }

        public virtual DroidAnimator Rotate(float fromDegree, float toDegree)
        {
            return Rotate(fromDegree, toDegree, this.DefaultDurationSeconds, this.DefaultDelaySeconds, this.DefaultInterpolation);
        }
        public virtual DroidAnimator Rotate(float fromDegree, float toDegree, double? durationSeconds, double? delaySeconds, AnimationInterpolation? interpolation)
        {
            return base.ExecuteFunction<DroidAnimator>("Rotate", delegate()
            {                
                ObjectAnimator rotateAnimator = ObjectAnimator.OfFloat(this.TargetView, "rotation", fromDegree, toDegree);
                SetDefaultsFor(rotateAnimator, durationSeconds, delaySeconds, interpolation);
                _animators.Add(rotateAnimator);

                return this;
            });
        }

        public virtual DroidAnimator FadeTo(float? fromOpacity, float toOpacity)
        {
            return this.FadeTo(fromOpacity, toOpacity, this.DefaultDurationSeconds, this.DefaultDelaySeconds, this.DefaultInterpolation);
        }
        public virtual DroidAnimator FadeTo(float? fromOpacity, float toOpacity, double? durationSeconds, double? delaySeconds, AnimationInterpolation? interpolation)
        {
            return base.ExecuteFunction<DroidAnimator>("FadeTo", delegate()
            {
                List<float> opacityValues = new List<float>();
                if (fromOpacity.HasValue)
                {
                    opacityValues.Add(fromOpacity.Value);
                }
                opacityValues.Add(toOpacity);

                ObjectAnimator fadeAnimator = ObjectAnimator.OfFloat(this.TargetView, "alpha", opacityValues.ToArray());
                SetDefaultsFor(fadeAnimator, durationSeconds, delaySeconds, interpolation);
                _animators.Add(fadeAnimator);

                return this;
            });
        }

        public DroidAnimator SizeTo(SizeF size)
        {
            return SizeTo(size, this.DefaultDurationSeconds, this.DefaultDelaySeconds, this.DefaultInterpolation);
        }
        public DroidAnimator SizeTo(SizeF size, double? durationSeconds, double? delaySeconds, AnimationInterpolation? interpolation)
        {
            return base.ExecuteFunction<DroidAnimator>("SizeTo", delegate()
            {
                PropertyValuesHolder propX = PropertyValuesHolder.OfFloat("scaleX", size.Width / this.TargetView.Width);
                PropertyValuesHolder propY = PropertyValuesHolder.OfFloat("scaleY", size.Height / this.TargetView.Height);

                ObjectAnimator fadeAnimator = ObjectAnimator.OfPropertyValuesHolder(this.TargetView, propX, propY);
                SetDefaultsFor(fadeAnimator, durationSeconds, delaySeconds, interpolation);
                _animators.Add(fadeAnimator);

                return this;
            });
        }
        public DroidAnimator ScaleTo(float scale)
        {
            return ScaleTo(scale, this.DefaultDurationSeconds, this.DefaultDelaySeconds, this.DefaultInterpolation);
        }

        public DroidAnimator ScaleTo(float scale, double? durationSeconds, double? delaySeconds, AnimationInterpolation? interpolation)
        {
            return ScaleTo(new float[] { scale }, null, this.DefaultDurationSeconds, this.DefaultDelaySeconds, this.DefaultInterpolation);
        }
        public DroidAnimator ScaleTo(float[] scales, float[] scaleDurations, double? durationSeconds, double? delaySeconds, AnimationInterpolation? interpolation)
        {
            return base.ExecuteFunction<DroidAnimator>("ScaleTo", delegate()
            {
                PropertyValuesHolder propX = PropertyValuesHolder.OfFloat("scaleX", scales);                
                PropertyValuesHolder propY = PropertyValuesHolder.OfFloat("scaleY", scales);
                if (scaleDurations != null && (scaleDurations.Length > 0))
                { 
                    int keyFrameCount = scaleDurations.Length;
                    if(scales.Length < keyFrameCount)
                    {
                        keyFrameCount-= keyFrameCount - scales.Length;
                    }
                    Keyframe[] keyFrames = new Keyframe[keyFrameCount];
                    for (int i = 0; i < keyFrameCount; i++)
                    {
                        keyFrames[i] = Keyframe.OfFloat(scaleDurations[i], scales[i]);
                    }
                    propX.SetKeyframes(keyFrames);
                    propY.SetKeyframes(keyFrames);
                }

                ObjectAnimator fadeAnimator = ObjectAnimator.OfPropertyValuesHolder(this.TargetView, propX, propY);
                SetDefaultsFor(fadeAnimator, durationSeconds, delaySeconds, interpolation);
                _animators.Add(fadeAnimator);

                return this;
            });
        }

        public DroidAnimator FloatValue(float fromValue, float toValue)
        {
            return FloatValue(fromValue, toValue, this.DefaultDurationSeconds, this.DefaultDelaySeconds, this.DefaultInterpolation);
        }
        public DroidAnimator FloatValue(float fromValue, float toValue, double? durationSeconds, double? delaySeconds, AnimationInterpolation? interpolation)
        {
            return base.ExecuteFunction<DroidAnimator>("FloatValue", delegate()
            {
                ValueAnimator textValueAnimator = ValueAnimator.OfFloat(fromValue, toValue);
                textValueAnimator.SetEvaluator(new FloatEvaluator());
                SetDefaultsFor(textValueAnimator, durationSeconds, delaySeconds, interpolation);
                textValueAnimator.Update += textValueAnimator_Float_Update;
                _animators.Add(textValueAnimator);

                return this;
            });
        }

        public DroidAnimator TransitionBackgroundColor(params int[] colors)
        {
            return TransitionBackgroundColor(colors, null, this.DefaultDurationSeconds, this.DefaultDelaySeconds, this.DefaultInterpolation);
        }
        public DroidAnimator TransitionBackgroundColor(int[] colors, float[] colorTransformDurations, double? durationSeconds, double? delaySeconds, AnimationInterpolation? interpolation)
        {
            return base.ExecuteFunction<DroidAnimator>("BackgroundColorValue", delegate()
            {
                PropertyValuesHolder colorValues = PropertyValuesHolder.OfInt("backgroundColor", colors);
                if(colorTransformDurations == null || colorTransformDurations.Length == 0)
                {
                    colorTransformDurations = new float[colors.Length];
                    for (int i = 0; i < colors.Length; i++)
                    {
                        colorTransformDurations[i] = i / colors.Length;
                    }
                }
                if (colorTransformDurations != null && colorTransformDurations.Length > 0)
                {
                    int keyFrameCount = colorTransformDurations.Length;
                    if (colors.Length < keyFrameCount)
                    {
                        keyFrameCount -= (keyFrameCount - colors.Length);
                    }
                    Keyframe[] keyFrames = new Keyframe[keyFrameCount];
                    for (int i = 0; i < keyFrameCount; i++)
                    {
                        keyFrames[i] = Keyframe.OfFloat(colorTransformDurations[i], colors[i]);
                    }
                    colorValues.SetKeyframes(keyFrames);
                }

                ValueAnimator valueAnimator = ValueAnimator.OfPropertyValuesHolder(colorValues);
                valueAnimator.SetEvaluator(new ArgbEvaluator());
                SetDefaultsFor(valueAnimator, durationSeconds, delaySeconds, interpolation);
                valueAnimator.Update += colorValueAnimator_Update;
                _animators.Add(valueAnimator);

                return this;
            });
        }

        public virtual AnimatorSet Commit()
        {
            return base.ExecuteFunction<AnimatorSet>("Commit", delegate()
            {
                AnimatorSet animatorSet = new AnimatorSet();
                animatorSet.PlayTogether(_animators.ToArray());
                _animators.Clear();
                return animatorSet;
            });
        }


        #endregion

        #region Protected Methods

        protected virtual Animator SetDefaultsFor(Animator animator, double? durationSeconds, double? delaySeconds, AnimationInterpolation? interpolation)
        {
            return base.ExecuteFunction<Animator>("SetDefaultFor", delegate()
            {
                if (animator != null)
                {
                    if (!durationSeconds.HasValue) { durationSeconds = DefaultDurationSeconds; }
                    if (!delaySeconds.HasValue) { delaySeconds = DefaultDelaySeconds; }
                    if (!interpolation.HasValue) { interpolation = DefaultInterpolation; }

                    if (durationSeconds.Value > 0)
                    {
                        animator.SetDuration((long)(durationSeconds.Value * 1000));
                    }
                    if (delaySeconds.HasValue)
                    {
                        animator.StartDelay = (long)(delaySeconds.Value * 1000);
                    }
                    if (interpolation.HasValue)
                    {
                        animator.SetInterpolator(this.ResolveDroidInterpolatorFrom(interpolation.Value));
                    }
                }
                return animator;
            });
        }
        protected virtual ITimeInterpolator ResolveDroidInterpolatorFrom(AnimationInterpolation interpolation)
        {
            return base.ExecuteFunction<ITimeInterpolator>("ResolveDroidInterpolatorFrom", delegate()
            {
                switch (interpolation)
                {
                    case AnimationInterpolation.Accelerate:
                        return new AccelerateInterpolator();
                    case AnimationInterpolation.EaseInEaseOut:
                        return new AccelerateDecelerateInterpolator();     
                    case AnimationInterpolation.EaseIn:
                        return new AccelerateInterpolator();
                    case AnimationInterpolation.EaseOut:
                        return new DecelerateInterpolator();
                    default:
                    case AnimationInterpolation.Linear:
                        return new LinearInterpolator();
                }
            });
        }

        #endregion

        #region Event Handlers

        protected void textValueAnimator_Float_Update(object sender, ValueAnimator.AnimatorUpdateEventArgs e)
        {
            base.ExecuteMethod("textValueAnimator_Float_Update", delegate()
            {
                TextView view = this.TargetView as TextView;
                if (view != null)
                {
                    object animatedValue = e.Animation.AnimatedValue as object;
                    float value;
                    if (float.TryParse(animatedValue.ToString(), out value))
                    {
                        view.Text = value.ToString();
                        view.Invalidate();
                    }
                }
            });
        }
        protected void colorValueAnimator_Update(object sender, ValueAnimator.AnimatorUpdateEventArgs e)
        {
            base.ExecuteMethod("colorValueAnimator_Update", delegate()
            {
                View view = this.TargetView;
                if (view != null)
                {
                    object animatedValue = e.Animation.AnimatedValue as object;
                    Decimal decimalValue;
                    if (Decimal.TryParse(animatedValue.ToString(), out decimalValue))
                    {
                        BigInteger value = new BigInteger(decimalValue);
                        view.SetBackgroundColor(value.ToString("X").ConvertHexToColor());
                        view.Invalidate();
                    }
                }
            });
        } 

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    _animators = null;
                    this.TargetView = null;
                }
                catch { }
            }
            base.Dispose(disposing);
        }
    }
}

