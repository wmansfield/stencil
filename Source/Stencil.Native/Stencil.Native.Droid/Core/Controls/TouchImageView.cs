using System;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Java.Lang;
using Math = System.Math;

namespace Stencil.Native.Droid
{
    public partial class TouchImageView : Android.Support.V7.Widget.AppCompatImageView
    {

        private static string DEBUG = "DEBUG";

        //
        // SuperMin and SuperMax multipliers. Determine how much the image can be
        // zoomed below or above the zoom boundaries, before animating back to the
        // min/max zoom boundary.
        //
        private static float SUPER_MIN_MULTIPLIER = .75f;
        private static float SUPER_MAX_MULTIPLIER = 1.25f;

        //
        // Scale of image ranges from minScale to maxScale, where minScale == 1
        // when the image is stretched to fit view.
        //
        private float normalizedScale;

        //
        // Matrix applied to image. MSCALE_X and MSCALE_Y should always be equal.
        // MTRANS_X and MTRANS_Y are the other values used. prevMatrix is the matrix
        // saved prior to the screen rotating.
        //
        private Matrix matrix, prevMatrix;

        public enum TouchState { NONE, DRAG, ZOOM, FLING, ANIMATE_ZOOM };
        private TouchState state;

        private float minScale;
        private float maxScale;
        private float superMinScale;
        private float superMaxScale;
        private float[] m;

        private Context context;
        private Fling fling;

        private ScaleType mScaleType;

        private bool imageRenderedAtLeastOnce;
        private bool onDrawReady;

        private ZoomVariables delayedZoomVariables;

        //
        // Size of view and previous view size (ie before rotation)
        //
        protected int viewWidth, viewHeight, prevViewWidth, prevViewHeight;

        //
        // Size of image when it is stretched to fit view. Before and After rotation.
        //
        private float matchViewWidth, matchViewHeight, prevMatchViewWidth, prevMatchViewHeight;

        //
        // After setting image, a value of true means the new image should maintain
        // the zoom of the previous image. False means it should be resized within the view.
        //
        private bool maintainZoomAfterSetImage;

        //
        // True when maintainZoomAfterSetImage has been set to true and setImage has been called.
        //
        //private bool setImageCalledRecenterImage;

        private ScaleGestureDetector mScaleDetector;
        private GestureDetector mGestureDetector;
        private GestureDetector.IOnDoubleTapListener doubleTapListener = null;
        private IOnTouchListener touchListener = null;

        [Register(".ctor", "(Landroid/content/Context;)V", "")]
        public TouchImageView(Context context)
            : base(context)
        {
            SharedConstructing(context);
        }
        [Register(".ctor", "(Landroid/content/Context;Landroid/util/AttributeSet;)V", "")]
        public TouchImageView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            SharedConstructing(context);
        }

        [Register(".ctor", "(Landroid/content/Context;Landroid/util/AttributeSet;I)V", "")]
        public TouchImageView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            SharedConstructing(context);
        }

        protected TouchImageView(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {

        }



        public void SharedConstructing(Context context)
        {
            this.Clickable = true;
            this.context = context;
            mScaleDetector = new ScaleGestureDetector(context, new ScaleListener(this));
            mGestureDetector = new GestureDetector(context, new GestureListener(this));
            matrix = new Matrix();
            prevMatrix = new Matrix();
            m = new float[9];
            normalizedScale = 1;
            if(mScaleType == null)
            {
                mScaleType = ScaleType.FitCenter;
            }
            minScale = 1;
            maxScale = 3;
            superMinScale = SUPER_MIN_MULTIPLIER * minScale;
            superMaxScale = SUPER_MAX_MULTIPLIER * maxScale;
            this.ImageMatrix = matrix;
            SetScaleType(ScaleType.Matrix);
            SetState(TouchState.NONE);
            base.SetOnTouchListener(new TouchImageViewListener(this));
        }

        public bool CanScrollHorizontallyFroyo(int direction)
        {
            return CanScrollHorizontally(direction);
        }

        public override void SetOnTouchListener(View.IOnTouchListener l)
        {
            touchListener = l;
        }

        public void SetOnDoubleTapListener(GestureDetector.IOnDoubleTapListener l)
        {
            doubleTapListener = l;
        }

        public override void SetImageResource(int resId)
        {
            base.SetImageResource(resId);
            SavePreviousImageValues();
            FitImageToView();
        }

        public override void SetImageBitmap(Bitmap bm)
        {
            base.SetImageBitmap(bm);
            SavePreviousImageValues();
            FitImageToView();
        }

        public override void SetImageDrawable(Drawable drawable)
        {
            base.SetImageDrawable(drawable);
            SavePreviousImageValues();
            FitImageToView();
        }

        public override void SetImageURI(Android.Net.Uri uri)
        {
            base.SetImageURI(uri);
            SavePreviousImageValues();
            FitImageToView();
        }

        public override void SetScaleType(ScaleType type)
        {
            if(type == ScaleType.FitStart || type == ScaleType.FitEnd)
            {
                throw new UnsupportedOperationException("TouchImageView does not support FitStart or FitEnd");
            }
            if(type == ScaleType.Matrix)
            {
                base.SetScaleType(ScaleType.Matrix);

            }
            else {
                mScaleType = type;
            }
        }


        public override ScaleType GetScaleType()
        {
            return mScaleType;
        }

        /*private void SetImageCalled() {
            if (!maintainZoomAfterSetImage) {
                setImageCalledRecenterImage = true;
            }
        }*/

        /**
        * Returns false if image is in initial, unzoomed state. False, otherwise.
            * @return true if image is zoomed
        */
        public bool IsZoomed()
        {
            return normalizedScale != 1;
        }

        /**
     * Return a Rect representing the zoomed image.
     * @return rect representing zoomed image
     */
        public RectF GetZoomedRect()
        {
            if(mScaleType == ScaleType.FitXy)
            {
                throw new UnsupportedOperationException("getZoomedRect() not supported with FitXy");
            }
            PointF topLeft = TransformCoordTouchToBitmap(0, 0, true);
            PointF bottomRight = TransformCoordTouchToBitmap(viewWidth, viewHeight, true);

            float w = this.Drawable.IntrinsicWidth;
            float h = this.Drawable.IntrinsicHeight;
            return new RectF(topLeft.X / w, topLeft.Y / h, bottomRight.X / w, bottomRight.Y / h);
        }

        /**
     * Save the current matrix and view dimensions
     * in the prevMatrix and prevView variables.
     */
        private void SavePreviousImageValues()
        {
            if(matrix != null && viewHeight != 0 && viewWidth != 0)
            {
                matrix.GetValues(m);
                prevMatrix.SetValues(m);
                prevMatchViewHeight = matchViewHeight;
                prevMatchViewWidth = matchViewWidth;
                prevViewHeight = viewHeight;
                prevViewWidth = viewWidth;
            }
        }

        protected override IParcelable OnSaveInstanceState()
        {
            Bundle bundle = new Bundle();
            bundle.PutParcelable("instanceState", base.OnSaveInstanceState());
            bundle.PutFloat("saveScale", normalizedScale);
            bundle.PutFloat("matchViewHeight", matchViewHeight);
            bundle.PutFloat("matchViewWidth", matchViewWidth);
            bundle.PutInt("viewWidth", viewWidth);
            bundle.PutInt("viewHeight", viewHeight);
            matrix.GetValues(m);
            bundle.PutFloatArray("matrix", m);
            bundle.PutBoolean("imageRendered", imageRenderedAtLeastOnce);
            return bundle;
        }

        protected override void OnRestoreInstanceState(IParcelable state)
        {
            if(state is Bundle)
            {
                Bundle bundle = (Bundle)state;
                normalizedScale = bundle.GetFloat("saveScale");
                m = bundle.GetFloatArray("matrix");
                prevMatrix.SetValues(m);
                prevMatchViewHeight = bundle.GetFloat("matchViewHeight");
                prevMatchViewWidth = bundle.GetFloat("matchViewWidth");
                prevViewHeight = bundle.GetInt("viewHeight");
                prevViewWidth = bundle.GetInt("viewWidth");
                imageRenderedAtLeastOnce = bundle.GetBoolean("imageRendered");
                base.OnRestoreInstanceState((IParcelable)bundle.GetParcelable("instanceState"));
                return;
            }

            base.OnRestoreInstanceState(state);
        }
        protected override void OnDraw(Canvas canvas)
        {
            onDrawReady = true;
            imageRenderedAtLeastOnce = true;
            if(delayedZoomVariables != null)
            {
                SetZoom(delayedZoomVariables.Scale, delayedZoomVariables.FocusX, delayedZoomVariables.FocusY, delayedZoomVariables.ScaleType);
                delayedZoomVariables = null;
            }
            base.OnDraw(canvas);
        }

        protected override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            SavePreviousImageValues();
        }

        /**
     * Get the max zoom multiplier.
     * @return max zoom multiplier.
     */
        public float GetMaxZoom()
        {
            return maxScale;
        }

        /**
     * Set the max zoom multiplier. Default value: 3.
     * @param max max zoom multiplier.
     */
        public void SetMaxZoom(float max)
        {
            maxScale = max;
            superMaxScale = SUPER_MAX_MULTIPLIER * maxScale;
        }

        /**
     * Get the min zoom multiplier.
     * @return min zoom multiplier.
     */
        public float GetMinZoom()
        {
            return minScale;
        }

        /**
     * Get the current zoom. This is the zoom relative to the initial
     * scale, not the original resource.
     * @return current zoom multiplier.
     */
        public float GetCurrentZoom()
        {
            return normalizedScale;
        }

        /**
     * Set the min zoom multiplier. Default value: 1.
     * @param min min zoom multiplier.
     */
        public void SetMinZoom(float min)
        {
            minScale = min;
            superMinScale = SUPER_MIN_MULTIPLIER * minScale;
        }

        /**
         * Reset zoom and translation to initial state.
         */
        public void ResetZoom()
        {
            normalizedScale = 1;
            FitImageToView();
        }

        /**
         * Set zoom to the specified scale. Image will be centered by default.
         * @param scale
         */
        public void SetZoom(float scale)
        {
            SetZoom(scale, 0.5f, 0.5f);
        }

        /**
         * Set zoom to the specified scale. Image will be centered around the point
         * (focusX, focusY). These floats range from 0 to 1 and denote the focus point
         * as a fraction from the left and top of the view. For example, the top left 
         * corner of the image would be (0, 0). And the bottom right corner would be (1, 1).
         * @param scale
         * @param focusX
         * @param focusY
         */
        public void SetZoom(float scale, float focusX, float focusY)
        {
            SetZoom(scale, focusX, focusY, mScaleType);
        }

        /**
         * Set zoom to the specified scale. Image will be centered around the point
         * (focusX, focusY). These floats range from 0 to 1 and denote the focus point
         * as a fraction from the left and top of the view. For example, the top left 
         * corner of the image would be (0, 0). And the bottom right corner would be (1, 1).
         * @param scale
         * @param focusX
         * @param focusY
         * @param scaleType
         */
        public void SetZoom(float scale, float focusX, float focusY, ScaleType scaleType)
        {
            //
            // setZoom can be called before the image is on the screen, but at this point, 
            // image and view sizes have not yet been calculated in onMeasure. Thus, we should
            // delay calling setZoom until the view has been measured.
            //
            if(!onDrawReady)
            {
                delayedZoomVariables = new ZoomVariables(scale, focusX, focusY, scaleType);
                return;
            }

            SetScaleType(scaleType);
            ResetZoom();
            ScaleImage(scale, viewWidth / 2, viewHeight / 2, false);
            matrix.GetValues(m);
            m[Matrix.MtransX] = -((focusX * GetImageWidth()) - (viewWidth * 0.5f));
            m[Matrix.MtransY] = -((focusY * GetImageHeight()) - (viewHeight * 0.5f));
            matrix.SetValues(m);
            FixTrans();
            this.ImageMatrix = matrix;
        }

        /**
         * Set zoom parameters equal to another TouchImageView. Including scale, position,
         * and ScaleType.
         * @param TouchImageView
         */
        public void SetZoom(TouchImageView img)
        {
            PointF center = img.GetScrollPosition();
            SetZoom(img.GetCurrentZoom(), center.X, center.Y, img.GetScaleType());
        }

        /**
         * Return the point at the center of the zoomed image. The PointF coordinates range
         * in value between 0 and 1 and the focus point is denoted as a fraction from the left 
         * and top of the view. For example, the top left corner of the image would be (0, 0). 
         * And the bottom right corner would be (1, 1).
         * @return PointF representing the scroll position of the zoomed image.
         */
        public PointF GetScrollPosition()
        {
            Drawable drawable = this.Drawable;
            if(drawable == null)
            {
                return null;
            }
            int drawableWidth = drawable.IntrinsicWidth;
            int drawableHeight = drawable.IntrinsicHeight;

            PointF point = TransformCoordTouchToBitmap(viewWidth / 2, viewHeight / 2, true);
            point.X /= drawableWidth;
            point.Y /= drawableHeight;
            return point;
        }

        /**
         * Set the focus point of the zoomed image. The focus points are denoted as a fraction from the
         * left and top of the view. The focus points can range in value between 0 and 1. 
         * @param focusX
         * @param focusY
         */
        public void SetScrollPosition(float focusX, float focusY)
        {
            SetZoom(normalizedScale, focusX, focusY);
        }

        /**
     * Performs boundary checking and fixes the image matrix if it 
     * is out of bounds.
     */
        private void FixTrans()
        {
            matrix.GetValues(m);
            float transX = m[Matrix.MtransX];
            float transY = m[Matrix.MtransY];

            float fixTransX = GetFixTrans(transX, viewWidth, GetImageWidth());
            float fixTransY = GetFixTrans(transY, viewHeight, GetImageHeight());

            if(fixTransX != 0 || fixTransY != 0)
            {
                matrix.PostTranslate(fixTransX, fixTransY);
            }
        }

        /**
     * When transitioning from zooming from focus to zoom from center (or vice versa)
     * the image can become unaligned within the view. This is apparent when zooming
     * quickly. When the content size is less than the view size, the content will often
     * be centered incorrectly within the view. fixScaleTrans first calls fixTrans() and 
     * then makes sure the image is centered correctly within the view.
     */
        private void FixScaleTrans()
        {
            FixTrans();
            matrix.GetValues(m);
            if(GetImageWidth() < viewWidth)
            {
                m[Matrix.MtransX] = (viewWidth - GetImageWidth()) / 2;
            }

            if(GetImageHeight() < viewHeight)
            {
                m[Matrix.MtransY] = (viewHeight - GetImageHeight()) / 2;
            }
            matrix.SetValues(m);
        }

        protected virtual float GetFixTrans(float trans, float viewSize, float contentSize)
        {
            float minTrans, maxTrans;

            if(contentSize <= viewSize)
            {
                minTrans = 0;
                maxTrans = viewSize - contentSize;

            }
            else {
                minTrans = viewSize - contentSize;
                maxTrans = 0;
            }

            if(trans < minTrans)
                return -trans + minTrans;
            if(trans > maxTrans)
                return -trans + maxTrans;
            return 0;
        }

        protected virtual float GetFixDragTrans(float delta, float viewSize, float contentSize)
        {
            if(contentSize <= viewSize)
            {
                return 0;
            }
            return delta;
        }

        private float GetImageWidth()
        {
            return matchViewWidth * normalizedScale;
        }

        private float GetImageHeight()
        {
            return matchViewHeight * normalizedScale;
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            Drawable drawable = this.Drawable;
            if(drawable == null || drawable.IntrinsicWidth == 0 || drawable.IntrinsicHeight == 0)
            {
                SetMeasuredDimension(0, 0);
                return;
            }

            int drawableWidth = drawable.IntrinsicWidth;
            int drawableHeight = drawable.IntrinsicHeight;
            int widthSize = MeasureSpec.GetSize(widthMeasureSpec);
            int widthMode = (int)MeasureSpec.GetMode(widthMeasureSpec);
            int heightSize = MeasureSpec.GetSize(heightMeasureSpec);
            int heightMode = (int)MeasureSpec.GetMode(heightMeasureSpec);
            viewWidth = SetViewSize(widthMode, widthSize, drawableWidth);
            viewHeight = SetViewSize(heightMode, heightSize, drawableHeight);

            //
            // Set view dimensions
            //
            SetMeasuredDimension(viewWidth, viewHeight);

            //
            // Fit content within view
            //
            FitImageToView();
        }

        /**
     * If the normalizedScale is equal to 1, then the image is made to fit the screen. Otherwise,
     * it is made to fit the screen according to the dimensions of the previous image matrix. This
     * allows the image to maintain its zoom after rotation.
     */
        protected virtual void FitImageToView()
        {
            Drawable drawable = this.Drawable;
            if(drawable == null || drawable.IntrinsicWidth == 0 || drawable.IntrinsicHeight == 0)
            {
                return;
            }
            if(matrix == null || prevMatrix == null)
            {
                return;
            }

            int drawableWidth = drawable.IntrinsicWidth;
            int drawableHeight = drawable.IntrinsicHeight;

            //
            // Scale image for view
            //
            float scaleX = (float)viewWidth / drawableWidth;
            float scaleY = (float)viewHeight / drawableHeight;

            if(mScaleType == ScaleType.Center)
            {
                scaleX = scaleY = 1;
            }
            else if(mScaleType == ScaleType.CenterCrop)
            {
                scaleX = scaleY = Math.Max(scaleX, scaleY);
            }
            else if(mScaleType == ScaleType.CenterInside)
            {
                scaleX = scaleY = Math.Min(1, Math.Min(scaleX, scaleY));
            }
            else if(mScaleType == ScaleType.FitCenter)
            {
                scaleX = scaleY = Math.Min(scaleX, scaleY);
            }
            else if(mScaleType == ScaleType.FitXy)
            {
            }
            else {
                throw new UnsupportedOperationException("TouchImageView does not support FitStart or FitEnd");
            }

            //
            // Center the image
            //
            float redundantYSpace = viewHeight - (scaleX * drawableHeight);
            float redundantXSpace = viewWidth - (scaleY * drawableWidth);
            matchViewWidth = viewWidth - redundantXSpace;
            matchViewHeight = viewHeight - redundantYSpace;
            if(!IsZoomed() && !imageRenderedAtLeastOnce)
            {
                //
                // Stretch and center image to fit view
                //
                matrix.SetScale(scaleX, scaleY);
                matrix.PostTranslate(redundantXSpace / 2, redundantYSpace / 2);
                normalizedScale = 1;

            }
            else {
                if(prevMatchViewWidth == 0 || prevMatchViewHeight == 0)
                {
                    SavePreviousImageValues();
                }

                prevMatrix.GetValues(m);

                //
                // Rescale Matrix after rotation
                //
                m[Matrix.MscaleX] = matchViewWidth / drawableWidth * normalizedScale;
                m[Matrix.MscaleY] = matchViewHeight / drawableHeight * normalizedScale;

                //
                // TransX and TransY from previous matrix
                //
                float transX = m[Matrix.MtransX];
                float transY = m[Matrix.MtransY];

                //
                // Width
                //
                float prevActualWidth = prevMatchViewWidth * normalizedScale;
                float actualWidth = GetImageWidth();
                TranslateMatrixAfterRotate(Matrix.MtransX, transX, prevActualWidth, actualWidth, prevViewWidth, viewWidth, drawableWidth);

                //
                // Height
                //
                float prevActualHeight = prevMatchViewHeight * normalizedScale;
                float actualHeight = GetImageHeight();
                TranslateMatrixAfterRotate(Matrix.MtransY, transY, prevActualHeight, actualHeight, prevViewHeight, viewHeight, drawableHeight);

                //
                // Set the matrix to the adjusted scale and translate values.
                //
                matrix.SetValues(m);
            }
            FixTrans();
            this.ImageMatrix = matrix;
        }

        /**
     * Set view dimensions based on layout params
     * 
     * @param mode 
     * @param size
     * @param drawableWidth
     * @return
     */
        private int SetViewSize(int mode, int size, int drawableWidth)
        {
            int viewSize;
            switch((MeasureSpecMode)mode)
            {
                case MeasureSpecMode.Exactly:
                    viewSize = size;
                    break;
                case MeasureSpecMode.AtMost:
                    viewSize = Math.Min(drawableWidth, size);
                    break;

                case MeasureSpecMode.Unspecified:
                    viewSize = drawableWidth;
                    break;

                default:
                    viewSize = size;
                    break;
            }
            return viewSize;
        }

        /**
     * After rotating, the matrix needs to be translated. This function finds the area of image 
     * which was previously centered and adjusts translations so that is again the center, post-rotation.
     * 
     * @param axis Matrix.MTRANS_X or Matrix.MTRANS_Y
     * @param trans the value of trans in that axis before the rotation
     * @param prevImageSize the width/height of the image before the rotation
     * @param imageSize width/height of the image after rotation
     * @param prevViewSize width/height of view before rotation
     * @param viewSize width/height of view after rotation
     * @param drawableSize width/height of drawable
     */
        private void TranslateMatrixAfterRotate(int axis, float trans, float prevImageSize, float imageSize, int prevViewSize, int viewSize, int drawableSize)
        {
            if(imageSize < viewSize)
            {
                //
                // The width/height of image is less than the view's width/height. Center it.
                //
                m[axis] = (viewSize - (drawableSize * m[Matrix.MscaleX])) * 0.5f;

            }
            else if(trans > 0)
            {
                //
                // The image is larger than the view, but was not before rotation. Center it.
                //
                m[axis] = -((imageSize - viewSize) * 0.5f);

            }
            else {
                //
                // Find the area of the image which was previously centered in the view. Determine its distance
                // from the left/top side of the view as a fraction of the entire image's width/height. Use that percentage
                // to calculate the trans in the new view width/height.
                //
                float percentage = (Math.Abs(trans) + (0.5f * prevViewSize)) / prevImageSize;
                m[axis] = -((percentage * imageSize) - (viewSize * 0.5f));
            }
        }

        private void SetState(TouchState state)
        {
            this.state = state;
        }

        /**
     * Gesture Listener detects a single click or long click and passes that on
     * to the view's listener.
     * @author Ortiz
     *
     */
        private class GestureListener : GestureDetector.SimpleOnGestureListener
        {
            private TouchImageView view;

            public GestureListener(TouchImageView view)
            {
                this.view = view;
            }

            public override bool OnSingleTapConfirmed(MotionEvent e)
            {
                if(view.doubleTapListener != null)
                {
                    return view.doubleTapListener.OnSingleTapConfirmed(e);
                }
                return view.PerformClick();
            }

            public override void OnLongPress(MotionEvent e)
            {
                view.PerformLongClick();
            }

            public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
            {
                if(view.fling != null)
                {
                    //
                    // If a previous fling is still active, it should be cancelled so that two flings
                    // are not run simultaenously.
                    //
                    view.fling.CancelFling();
                }
                view.fling = new Fling(view, (int)velocityX, (int)velocityY);
                view.CompatPostOnAnimation(view.fling);
                return base.OnFling(e1, e2, velocityX, velocityY);
            }

            public override bool OnDoubleTap(MotionEvent e)
            {
                bool consumed = false;
                if(view.doubleTapListener != null)
                {
                    consumed = view.doubleTapListener.OnDoubleTap(e);
                }
                if(view.state == TouchState.NONE)
                {
                    float targetZoom = (view.normalizedScale == view.minScale) ? view.maxScale : view.minScale;
                    DoubleTapZoom doubleTap = new DoubleTapZoom(view, targetZoom, e.GetX(), e.GetY(), false);
                    view.CompatPostOnAnimation(doubleTap);
                    consumed = true;
                }
                return consumed;
            }

            public override bool OnDoubleTapEvent(MotionEvent e)
            {
                if(view.doubleTapListener != null)
                {
                    return view.doubleTapListener.OnDoubleTapEvent(e);
                }
                return false;
            }
        }

        /**
     * Responsible for all touch events. Handles the heavy lifting of drag and also sends
     * touch events to Scale Detector and Gesture Detector.
     * @author Ortiz
     *
     */
        private class TouchImageViewListener : Java.Lang.Object, View.IOnTouchListener
        {
            private TouchImageView view;

            //
            // Remember last point position for dragging
            //
            private PointF last = new PointF();

            public TouchImageViewListener(TouchImageView view)
            {
                this.view = view;
            }

            public bool OnTouch(View v, MotionEvent evt)
            {
                if(view.touchListener != null) view.touchListener.OnTouch(v, evt); // User-defined handler, maybe
                view.mScaleDetector.OnTouchEvent(evt);
                view.mGestureDetector.OnTouchEvent(evt);
                PointF curr = new PointF(evt.GetX(), evt.GetY());

                if(view.state == TouchState.NONE || view.state == TouchState.DRAG || view.state == TouchState.FLING)
                {
                    switch(evt.Action)
                    {
                        case MotionEventActions.Down:
                            last.Set(curr);
                            if(view.fling != null)
                                view.fling.CancelFling();
                            view.SetState(TouchState.DRAG);
                            break;

                        case MotionEventActions.Move:
                            if(view.state == TouchState.DRAG)
                            {
                                float deltaX = curr.X - last.X;
                                float deltaY = curr.Y - last.Y;
                                float fixTransX = view.GetFixDragTrans(deltaX, view.viewWidth, view.GetImageWidth());
                                float fixTransY = view.GetFixDragTrans(deltaY, view.viewHeight, view.GetImageHeight());
                                view.matrix.PostTranslate(fixTransX, fixTransY);
                                view.FixTrans();
                                last.Set(curr.X, curr.Y);
                            }
                            break;

                        case MotionEventActions.Up:
                        case MotionEventActions.PointerUp:
                            view.SetState(TouchState.NONE);
                            break;
                    }
                }

                view.ImageMatrix = view.matrix;
                //
                // indicate event was handled
                //
                return true;
            }
        }

        /**
     * ScaleListener detects user two finger scaling and scales image.
     * @author Ortiz
     *
     */
        private class ScaleListener : ScaleGestureDetector.SimpleOnScaleGestureListener
        {
            private TouchImageView view;

            public ScaleListener(TouchImageView view)
            {
                this.view = view;
            }

            public override bool OnScaleBegin(ScaleGestureDetector detector)
            {
                view.SetState(TouchState.ZOOM);
                return true;
            }

            public override bool OnScale(ScaleGestureDetector detector)
            {
                view.ScaleImage(detector.ScaleFactor, detector.FocusX, detector.FocusY, true);
                return true;
            }

            public override void OnScaleEnd(ScaleGestureDetector detector)
            {
                base.OnScaleEnd(detector);
                view.SetState(TouchState.NONE);
                bool animateToZoomBoundary = false;
                float targetZoom = view.normalizedScale;
                if(view.normalizedScale > view.maxScale)
                {
                    targetZoom = view.maxScale;
                    animateToZoomBoundary = true;

                }
                else if(view.normalizedScale < view.minScale)
                {
                    targetZoom = view.minScale;
                    animateToZoomBoundary = true;
                }

                if(animateToZoomBoundary)
                {
                    DoubleTapZoom doubleTap = new DoubleTapZoom(view, targetZoom, view.viewWidth / 2, view.viewHeight / 2, true);
                    view.CompatPostOnAnimation(doubleTap);
                }
            }
        }

        private void ScaleImage(float deltaScale, float focusX, float focusY, bool stretchImageToSuper)
        {

            float lowerScale, upperScale;
            if(stretchImageToSuper)
            {
                lowerScale = superMinScale;
                upperScale = superMaxScale;

            }
            else {
                lowerScale = minScale;
                upperScale = maxScale;
            }

            float origScale = normalizedScale;
            normalizedScale *= deltaScale;
            if(normalizedScale > upperScale)
            {
                normalizedScale = upperScale;
                deltaScale = upperScale / origScale;
            }
            else if(normalizedScale < lowerScale)
            {
                normalizedScale = lowerScale;
                deltaScale = lowerScale / origScale;
            }

            matrix.PostScale(deltaScale, deltaScale, focusX, focusY);
            FixScaleTrans();
        }

        /**
     * DoubleTapZoom calls a series of runnables which apply
     * an animated zoom in/out graphic to the image.
     * @author Ortiz
     *
     */
        protected class DoubleTapZoom : Java.Lang.Object, IRunnable
        {

            private long startTime;
            private static float ZOOM_TIME = 500;
            private float startZoom, targetZoom;
            private float bitmapX, bitmapY;
            private bool stretchImageToSuper;
            private AccelerateDecelerateInterpolator interpolator = new AccelerateDecelerateInterpolator();
            private PointF startTouch;
            private PointF endTouch;

            private TouchImageView view;

            public DoubleTapZoom(TouchImageView view, float targetZoom, float focusX, float focusY, bool stretchImageToSuper)
            {
                this.view = view;
                view.SetState(TouchState.ANIMATE_ZOOM);
                startTime = DateTime.Now.Ticks;
                this.startZoom = view.normalizedScale;
                this.targetZoom = targetZoom;
                this.stretchImageToSuper = stretchImageToSuper;
                PointF bitmapPoint = view.TransformCoordTouchToBitmap(focusX, focusY, false);
                this.bitmapX = bitmapPoint.X;
                this.bitmapY = bitmapPoint.Y;

                //
                // Used for translating image during scaling
                //
                startTouch = view.TransformCoordBitmapToTouch(bitmapX, bitmapY);
                endTouch = new PointF(view.viewWidth / 2, view.viewHeight / 2);
            }

            public void Run()
            {
                float t = Interpolate();
                float deltaScale = CalculateDeltaScale(t);
                view.ScaleImage(deltaScale, bitmapX, bitmapY, stretchImageToSuper);
                TranslateImageToCenterTouchPosition(t);
                view.FixScaleTrans();
                view.ImageMatrix = view.matrix;

                if(t < 1f)
                {
                    //
                    // We haven't finished zooming
                    //
                    view.CompatPostOnAnimation(this);

                }
                else {
                    //
                    // Finished zooming
                    //
                    view.SetState(TouchState.NONE);
                }
            }

            /**
         * Interpolate between where the image should start and end in order to translate
         * the image so that the point that is touched is what ends up centered at the end
         * of the zoom.
         * @param t
         */
            private void TranslateImageToCenterTouchPosition(float t)
            {
                float targetX = startTouch.X + t * (endTouch.X - startTouch.X);
                float targetY = startTouch.Y + t * (endTouch.Y - startTouch.Y);
                PointF curr = view.TransformCoordBitmapToTouch(bitmapX, bitmapY);
                view.matrix.PostTranslate(targetX - curr.X, targetY - curr.Y);
            }

            /**
         * Use interpolator to get t
         * @return
         */
            private float Interpolate()
            {
                long currTime = DateTime.Now.Ticks;
                float elapsed = (currTime - startTime) / ZOOM_TIME;
                elapsed = Math.Min(1f, elapsed);
                return interpolator.GetInterpolation(elapsed);
            }

            /**
         * Interpolate the current targeted zoom and get the delta
         * from the current zoom.
         * @param t
         * @return
         */
            private float CalculateDeltaScale(float t)
            {
                float zoom = startZoom + t * (targetZoom - startZoom);
                return zoom / view.normalizedScale;
            }
        }

        /**
     * This function will transform the coordinates in the touch event to the coordinate 
     * system of the drawable that the imageview contain
     * @param x x-coordinate of touch event
     * @param y y-coordinate of touch event
     * @param clipToBitmap Touch event may occur within view, but outside image content. True, to clip return value
     *          to the bounds of the bitmap size.
     * @return Coordinates of the point touched, in the coordinate system of the original drawable.
     */
        protected PointF TransformCoordTouchToBitmap(float x, float y, bool clipToBitmap)
        {
            matrix.GetValues(m);
            float origW = this.Drawable.IntrinsicWidth;
            float origH = this.Drawable.IntrinsicHeight;
            float transX = m[Matrix.MtransX];
            float transY = m[Matrix.MtransY];
            float finalX = ((x - transX) * origW) / GetImageWidth();
            float finalY = ((y - transY) * origH) / GetImageHeight();

            if(clipToBitmap)
            {
                finalX = Math.Min(Math.Max(x, 0), origW);
                finalY = Math.Min(Math.Max(y, 0), origH);
            }

            return new PointF(finalX, finalY);
        }

        /**
     * Inverse of transformCoordTouchToBitmap. This function will transform the coordinates in the
     * drawable's coordinate system to the view's coordinate system.
     * @param bx x-coordinate in original bitmap coordinate system
     * @param by y-coordinate in original bitmap coordinate system
     * @return Coordinates of the point in the view's coordinate system.
     */
        private PointF TransformCoordBitmapToTouch(float bx, float by)
        {
            matrix.GetValues(m);
            float origW = this.Drawable.IntrinsicWidth;
            float origH = this.Drawable.IntrinsicHeight;
            float px = bx / origW;
            float py = by / origH;
            float finalX = m[Matrix.MtransX] + GetImageWidth() * px;
            float finalY = m[Matrix.MtransY] + GetImageHeight() * py;
            return new PointF(finalX, finalY);
        }

        /**
     * Fling launches sequential runnables which apply
     * the fling graphic to the image. The values for the translation
     * are interpolated by the Scroller.
     * @author Ortiz
     *
     */
        protected class Fling : Java.Lang.Object, IRunnable
        {

            private TouchImageView view;
            Scroller scroller;
            int currX, currY;

            public Fling(TouchImageView view, int velocityX, int velocityY)
            {
                this.view = view;
                view.SetState(TouchState.FLING);
                scroller = new Scroller(view.context);
                view.matrix.GetValues(view.m);

                int startX = (int)view.m[Matrix.MtransX];
                int startY = (int)view.m[Matrix.MtransY];
                int minX, maxX, minY, maxY;

                if(view.GetImageWidth() > view.viewWidth)
                {
                    minX = view.viewWidth - (int)view.GetImageWidth();
                    maxX = 0;
                }
                else
                {
                    minX = maxX = startX;
                }

                if(view.GetImageHeight() > view.viewHeight)
                {
                    minY = view.viewHeight - (int)view.GetImageHeight();
                    maxY = 0;
                }
                else
                {
                    minY = maxY = startY;
                }

                scroller.Fling(startX, startY, (int)velocityX, (int)velocityY, minX, maxX, minY, maxY);
                currX = startX;
                currY = startY;
            }

            public void CancelFling()
            {
                if(scroller != null)
                {
                    view.SetState(TouchState.NONE);
                    scroller.ForceFinished(true);
                }
            }

            public void Run()
            {
                if(scroller.IsFinished)
                {
                    scroller = null;
                    return;
                }

                if(scroller.ComputeScrollOffset())
                {
                    int newX = scroller.CurrX;
                    int newY = scroller.CurrY;
                    int transX = newX - currX;
                    int transY = newY - currY;
                    currX = newX;
                    currY = newY;
                    view.matrix.PostTranslate(transX, transY);
                    view.FixTrans();
                    view.ImageMatrix = view.matrix;
                    view.CompatPostOnAnimation(this);
                }
            }
        }

        //@TargetApi(Build.VERSION_CODES.GINGERBREAD)
        private class CompatScroller
        {
            Scroller scroller;
            OverScroller overScroller;
            bool isPreGingerbread;

            public CompatScroller(Context context)
            {
                if(Build.VERSION.SdkInt < BuildVersionCodes.Gingerbread)
                {
                    isPreGingerbread = true;
                    scroller = new Scroller(context);

                }
                else {
                    isPreGingerbread = false;
                    overScroller = new OverScroller(context);
                }
            }

            public void Fling(int startX, int startY, int velocityX, int velocityY, int minX, int maxX, int minY, int maxY)
            {
                if(isPreGingerbread)
                {
                    scroller.Fling(startX, startY, velocityX, velocityY, minX, maxX, minY, maxY);
                }
                else {
                    overScroller.Fling(startX, startY, velocityX, velocityY, minX, maxX, minY, maxY);
                }
            }

            public void ForceFinished(bool finished)
            {
                if(isPreGingerbread)
                {
                    scroller.ForceFinished(finished);
                }
                else {
                    overScroller.ForceFinished(finished);
                }
            }

            public bool isFinished()
            {
                if(isPreGingerbread)
                {
                    return scroller.IsFinished;
                }
                else {
                    return overScroller.IsFinished;
                }
            }

            public bool computeScrollOffset()
            {
                if(isPreGingerbread)
                {
                    return scroller.ComputeScrollOffset();
                }
                else {
                    return overScroller.ComputeScrollOffset();
                }
            }

            public int getCurrX()
            {
                if(isPreGingerbread)
                {
                    return scroller.CurrX;
                }
                else {
                    return overScroller.CurrX;
                }
            }

            public int getCurrY()
            {
                if(isPreGingerbread)
                {
                    return scroller.CurrY;
                }
                else {
                    return overScroller.CurrY;
                }
            }
        }

        //@TargetApi(Build.VERSION_CODES.JELLY_BEAN)
        private void CompatPostOnAnimation(IRunnable runnable)
        {
            if(Build.VERSION.SdkInt >= BuildVersionCodes.JellyBean)
            {
                PostOnAnimation(runnable);

            }
            else {
                PostDelayed(runnable, 1000 / 60);
            }
        }

        private class ZoomVariables
        {
            public float Scale;
            public float FocusX;
            public float FocusY;
            public ScaleType ScaleType;

            public ZoomVariables(float scale, float focusX, float focusY, ScaleType scaleType)
            {
                this.Scale = scale;
                this.FocusX = focusX;
                this.FocusY = focusY;
                this.ScaleType = scaleType;
            }
        }

        private void PrintMatrixInfo()
        {
            matrix.GetValues(m);
            Log.Debug(DEBUG, "Scale: " + m[Matrix.MscaleX] + " TransX: " + m[Matrix.MtransX] + " TransY: " + m[Matrix.MtransY]);
        }
    }
}
