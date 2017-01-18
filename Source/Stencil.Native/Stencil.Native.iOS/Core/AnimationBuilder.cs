using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using System.Collections.Generic;

namespace Stencil.Native.iOS.Core
{
    public class AnimationBuilder
    {
        public static class Timing
        {
            public static CAMediaTimingFunction sineEaseIn = CAMediaTimingFunction.FromControlPoints(0.47f, 0f, 0.745f, 0.715f);
            public static CAMediaTimingFunction sineEaseOut = CAMediaTimingFunction.FromControlPoints(0.39f, 0.575f, 0.565f, 1f);
            public static CAMediaTimingFunction sineEaseInOut = CAMediaTimingFunction.FromControlPoints(0.445f, 0.05f, 0.55f, 0.95f);

            public static CAMediaTimingFunction quadraticEaseIn = CAMediaTimingFunction.FromControlPoints(0.55f, 0.085f, 0.68f, 0.53f);
            public static CAMediaTimingFunction quadraticEaseOut = CAMediaTimingFunction.FromControlPoints(0.25f, 0.46f, 0.45f, 0.94f);
            public static CAMediaTimingFunction quadraticEaseInOut = CAMediaTimingFunction.FromControlPoints(0.455f, 0.03f, 0.515f, 0.955f);

            public static CAMediaTimingFunction cubicEaseIn = CAMediaTimingFunction.FromControlPoints(0.55f, 0.055f, 0.675f, 0.19f);
            public static CAMediaTimingFunction cubicEaseOut = CAMediaTimingFunction.FromControlPoints(0.215f, 0.61f, 0.355f, 1f);
            public static CAMediaTimingFunction cubicEaseInOut = CAMediaTimingFunction.FromControlPoints(0.645f, 0.045f, 0.355f, 1f);


            public static CAMediaTimingFunction quarticEaseIn = CAMediaTimingFunction.FromControlPoints(0.895f, 0.03f, 0.685f, 0.22f);
            public static CAMediaTimingFunction quarticEaseOut = CAMediaTimingFunction.FromControlPoints(0.165f, 0.84f, 0.44f, 1f);
            public static CAMediaTimingFunction quarticEaseInOut = CAMediaTimingFunction.FromControlPoints(0.77f, 0f, 0.175f, 1f);

            public static CAMediaTimingFunction quinticEaseIn = CAMediaTimingFunction.FromControlPoints(0.755f, 0.05f, 0.855f, 0.06f);
            public static CAMediaTimingFunction quinticEaseOut = CAMediaTimingFunction.FromControlPoints(0.23f, 1f, 0.32f, 1f);
            public static CAMediaTimingFunction quinticEaseInOut = CAMediaTimingFunction.FromControlPoints(0.86f, 0f, 0.07f, 1f);

            public static CAMediaTimingFunction exponentialEaseIn = CAMediaTimingFunction.FromControlPoints(0.95f, 0.05f, 0.795f, 0.035f);
            public static CAMediaTimingFunction exponentialEaseOut = CAMediaTimingFunction.FromControlPoints(0.19f, 1f, 0.22f, 1f);
            public static CAMediaTimingFunction exponentialEaseInOut = CAMediaTimingFunction.FromControlPoints(1f, 0f, 0f, 1f);

            public static CAMediaTimingFunction circularEaseIn = CAMediaTimingFunction.FromControlPoints(0.6f, 0.04f, 0.98f, 0.335f);
            public static CAMediaTimingFunction circularEaseOut = CAMediaTimingFunction.FromControlPoints(0.075f, 0.82f, 0.165f, 1f);
            public static CAMediaTimingFunction circularEaseInOut = CAMediaTimingFunction.FromControlPoints(0.785f, 0.135f, 0.15f, 0.86f);

            public static CAMediaTimingFunction backEaseIn = CAMediaTimingFunction.FromControlPoints(0.6f, -0.28f, 0.735f, 0.045f);
            public static CAMediaTimingFunction backEaseOut = CAMediaTimingFunction.FromControlPoints(0.175f, 0.885f, 0.32f, 1.275f);
            public static CAMediaTimingFunction backEaseInOut = CAMediaTimingFunction.FromControlPoints(0.68f, -0.55f, 0.265f, 1.55f);

        }

        
        public static AnimationBuilder Begin(CALayer targetItem, string animationKey = "")
        {
            CATransaction.Begin();

            if (string.IsNullOrEmpty(animationKey))
            {
                animationKey = "AnimationBuilder";
            }

            return new AnimationBuilder()
            {
                AnimationKey = animationKey,
                TargetItem = targetItem
            };
        }
        private AnimationBuilder()
        {
            this.Animations = new List<CAAnimation>();
            this.EasingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseOut);
        }
        public List<CAAnimation> Animations { get; set; }

        public double DurationSeconds { get; set; }
        public double DelaySeconds { get; set; }
        public CALayer TargetItem { get; set; }
        public CAMediaTimingFunction EasingFunction { get; set; }
        public double TotalDurationSeconds { get; set; }
        public string AnimationKey { get; set; }

        protected void EnsureTotalDuration(double delaySeconds, double durationSeconds)
        {
            double newValue = 0;
            if (delaySeconds > 0)
            {
                newValue += delaySeconds;
            }
            if (durationSeconds > 0)
            {
                newValue += durationSeconds;
            }
            if (newValue > TotalDurationSeconds)
            {
                this.TotalDurationSeconds = newValue;
            }
        }

        public CAAnimationGroup Commit()
        {
            CAAnimationGroup animationGroup = new CAAnimationGroup();
            animationGroup.RemovedOnCompletion = false;
            animationGroup.FillMode = CAFillMode.Forwards;
            animationGroup.Duration = TotalDurationSeconds + .1;
            animationGroup.Animations = this.Animations.ToArray();

            TargetItem.AddAnimation(animationGroup, this.AnimationKey);

            CATransaction.Commit();

            return animationGroup;
        }

        public AnimationBuilder SetDefaultDurationSeconds(double durationSeconds)
        {
            this.DurationSeconds = durationSeconds;
            return this;
        }
        public AnimationBuilder SetDefaultDelaySeconds(double delaySeconds)
        {
            this.DelaySeconds = delaySeconds;
            return this;
        }
        /// <summary>
        /// CAMediaTimingFunction
        /// </summary>
        public AnimationBuilder SetDefaultEasingFunction(string functionName)
        {
            this.EasingFunction = CAMediaTimingFunction.FromName(new NSString(functionName));
            return this;
        }
        public AnimationBuilder SetDefaultEasingFunction(CAMediaTimingFunction function)
        {
            this.EasingFunction = function;
            return this;
        }

        public AnimationBuilder MoveTo(CGPoint position)
        {
            return MoveTo(position, this.DelaySeconds, this.DurationSeconds);
        }
        public AnimationBuilder MoveTo(CGPoint fromPosition, CGPoint toPosition)
        {
            return MoveTo(fromPosition, toPosition, this.DelaySeconds, this.DurationSeconds);
        }
        public AnimationBuilder MoveTo(CGPoint position, double delaySeconds, double durationSeconds)
        {
            //  position
            var positionAnimation = CABasicAnimation.FromKeyPath("position");
            positionAnimation.TimingFunction = this.EasingFunction;
            if (durationSeconds > 0)
            {
                positionAnimation.Duration = durationSeconds;
            }
            if (delaySeconds > 0)
            {
                positionAnimation.BeginTime = delaySeconds;
            }
            positionAnimation.FillMode = CAFillMode.Forwards;
            positionAnimation.RemovedOnCompletion = false;
            positionAnimation.To = NSValue.FromCGPoint(position);

            this.Animations.Add(positionAnimation);
            this.EnsureTotalDuration(delaySeconds, durationSeconds);

            return this;
        }
        public AnimationBuilder MoveTo(CGPoint fromPosition, CGPoint toPosition, double delaySeconds, double durationSeconds)
        {
            //  position
            var positionAnimation = CABasicAnimation.FromKeyPath("position");
            positionAnimation.TimingFunction = this.EasingFunction;
            if (durationSeconds > 0)
            {
                positionAnimation.Duration = durationSeconds;
            }
            if (delaySeconds > 0)
            {
                positionAnimation.BeginTime = delaySeconds;
            }
            positionAnimation.FillMode = CAFillMode.Forwards;
            positionAnimation.RemovedOnCompletion = false;
            positionAnimation.From = NSValue.FromCGPoint(fromPosition);
            positionAnimation.To = NSValue.FromCGPoint(toPosition);

            this.Animations.Add(positionAnimation);
            this.EnsureTotalDuration(delaySeconds, durationSeconds);

            return this;
        }


        public AnimationBuilder MoveBy(CGPoint position)
        {
            return MoveBy(position, this.DelaySeconds, this.DurationSeconds);
        }
        public AnimationBuilder MoveBy(CGPoint fromPosition, CGPoint toPosition)
        {
            return MoveBy(fromPosition, toPosition, this.DelaySeconds, this.DurationSeconds);
        }
        public AnimationBuilder MoveBy(CGPoint position, double delaySeconds, double durationSeconds)
        {
            //  position
            var positionAnimation = CABasicAnimation.FromKeyPath("position");
            positionAnimation.TimingFunction = this.EasingFunction;
            if (durationSeconds > 0)
            {
                positionAnimation.Duration = durationSeconds;
            }
            if (delaySeconds > 0)
            {
                positionAnimation.BeginTime = delaySeconds;
            }
            positionAnimation.FillMode = CAFillMode.Forwards;
            positionAnimation.RemovedOnCompletion = false;
            positionAnimation.By = NSValue.FromCGPoint(position);

            this.Animations.Add(positionAnimation);
            this.EnsureTotalDuration(delaySeconds, durationSeconds);

            return this;
        }
        public AnimationBuilder MoveBy(CGPoint fromPosition, CGPoint toPosition, double delaySeconds, double durationSeconds)
        {
            //  position
            var positionAnimation = CABasicAnimation.FromKeyPath("position");
            positionAnimation.TimingFunction = this.EasingFunction;
            if (durationSeconds > 0)
            {
                positionAnimation.Duration = durationSeconds;
            }
            if (delaySeconds > 0)
            {
                positionAnimation.BeginTime = delaySeconds;
            }
            positionAnimation.FillMode = CAFillMode.Forwards;
            positionAnimation.RemovedOnCompletion = false;
            positionAnimation.From = NSValue.FromCGPoint(fromPosition);
            positionAnimation.By = NSValue.FromCGPoint(toPosition);

            this.Animations.Add(positionAnimation);
            this.EnsureTotalDuration(delaySeconds, durationSeconds);

            return this;
        }

        public AnimationBuilder SizeTo(CGSize size)
        {
            return SizeTo(size, this.DelaySeconds, this.DurationSeconds);
        }
        public AnimationBuilder SizeTo(CGSize fromSize, CGSize toSize)
        {
            return SizeTo(fromSize, toSize, this.DelaySeconds, this.DurationSeconds);
        }
        public AnimationBuilder SizeTo(CGSize size, double delaySeconds, double durationSeconds)
        {
            var sizeAnimation = CABasicAnimation.FromKeyPath("transform");
            sizeAnimation.TimingFunction = this.EasingFunction;
            if (durationSeconds > 0)
            {
                sizeAnimation.Duration = durationSeconds;
            }
            if (delaySeconds > 0)
            {
                sizeAnimation.BeginTime = delaySeconds;
            }
            sizeAnimation.FillMode = CAFillMode.Forwards;
            sizeAnimation.RemovedOnCompletion = false;
            sizeAnimation.To = NSValue.FromCATransform3D(CATransform3D.MakeScale(size.Width / TargetItem.Bounds.Width, size.Height / TargetItem.Bounds.Height, 1));

            this.Animations.Add(sizeAnimation);
            this.EnsureTotalDuration(delaySeconds, durationSeconds);

            return this;
        }
        public AnimationBuilder SizeTo(CGSize fromSize, CGSize toSize, double delaySeconds, double durationSeconds)
        {
            var sizeAnimation = CABasicAnimation.FromKeyPath("transform");
            sizeAnimation.TimingFunction = this.EasingFunction;
            if (durationSeconds > 0)
            {
                sizeAnimation.Duration = durationSeconds;
            }
            if (delaySeconds > 0)
            {
                sizeAnimation.BeginTime = delaySeconds;
            }
            sizeAnimation.FillMode = CAFillMode.Forwards;
            sizeAnimation.RemovedOnCompletion = false;
            sizeAnimation.From = NSValue.FromCATransform3D(CATransform3D.MakeScale(fromSize.Width / TargetItem.Bounds.Width, fromSize.Height / TargetItem.Bounds.Height, 1));
            sizeAnimation.To = NSValue.FromCATransform3D(CATransform3D.MakeScale(toSize.Width / TargetItem.Bounds.Width, toSize.Height / TargetItem.Bounds.Height, 1));

            this.Animations.Add(sizeAnimation);
            this.EnsureTotalDuration(delaySeconds, durationSeconds);

            return this;
        }


        public AnimationBuilder ScaleTo(float scale)
        {
            return ScaleTo(scale, this.DelaySeconds, this.DurationSeconds);
        }
        public AnimationBuilder ScaleTo(float scale, double delaySeconds, double durationSeconds)
        {
            var scaleAnimation = CABasicAnimation.FromKeyPath("transform.scale");
            scaleAnimation.TimingFunction = this.EasingFunction;
            if (durationSeconds > 0)
            {
                scaleAnimation.Duration = durationSeconds;
            }
            if (delaySeconds > 0)
            {
                scaleAnimation.BeginTime = delaySeconds;
            }
            scaleAnimation.FillMode = CAFillMode.Forwards;
            scaleAnimation.RemovedOnCompletion = false;
            scaleAnimation.To = NSNumber.FromFloat(scale);

            this.Animations.Add(scaleAnimation);
            this.EnsureTotalDuration(delaySeconds, durationSeconds);

            return this;
        }

        public AnimationBuilder RotateDegrees(nfloat fromDegrees, nfloat toDegrees)
        {
            return RotateDegrees(fromDegrees, toDegrees, this.DelaySeconds, this.DurationSeconds);
        }
        public AnimationBuilder RotateDegrees(nfloat fromDegrees, nfloat toDegrees, double delaySeconds, double durationSeconds)
        {
            // chip rotation
            var animateRotate = CABasicAnimation.FromKeyPath("transform.rotation.z");
            animateRotate.TimingFunction = this.EasingFunction;
            if (durationSeconds > 0)
            {
                animateRotate.Duration = durationSeconds;
            }
            if (delaySeconds > 0)
            {
                animateRotate.BeginTime = delaySeconds;
            }
            animateRotate.FillMode = CAFillMode.Forwards;
            animateRotate.RemovedOnCompletion = false;
            animateRotate.From = NSNumber.FromNFloat(fromDegrees.ToRadians());
            animateRotate.To = NSNumber.FromNFloat(toDegrees.ToRadians());

            this.Animations.Add(animateRotate);
            this.EnsureTotalDuration(delaySeconds, durationSeconds);

            return this;
        }

        public AnimationBuilder FadeTo(float? fromOpacity, float toOpacity)
        {
            return FadeTo(fromOpacity, toOpacity, this.DelaySeconds, this.DurationSeconds);
        }
        public AnimationBuilder FadeTo(float? fromOpacity, float toOpacity, double delaySeconds, double durationSeconds)
        {
            var alphaAnimation = CABasicAnimation.FromKeyPath("opacity");
            alphaAnimation.TimingFunction = this.EasingFunction;
            if (durationSeconds > 0)
            {
                alphaAnimation.Duration = durationSeconds;
            }
            if (delaySeconds > 0)
            {
                alphaAnimation.BeginTime = delaySeconds;
            }
            alphaAnimation.FillMode = CAFillMode.Forwards;
            alphaAnimation.RemovedOnCompletion = false;
            if (fromOpacity.HasValue)
            {
                alphaAnimation.From = NSNumber.FromFloat(fromOpacity.Value);
            }
            alphaAnimation.To = NSNumber.FromFloat(toOpacity);

            this.Animations.Add(alphaAnimation);
            this.EnsureTotalDuration(delaySeconds, durationSeconds);

            return this;
        }
    }
}

