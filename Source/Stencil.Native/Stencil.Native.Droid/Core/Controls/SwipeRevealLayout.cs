/**
 The MIT License (MIT)

 Copyright (c) 2016 Chau Thai

 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:

 The above copyright notice and this permission notice shall be included in all
 copies or substantial portions of the Software.

 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Support.V4.Widget;
using Android.Support.V4.View;
using Android.Util;
using Android.Content.Res;
using Stencil.Native.Core;

namespace Stencil.Native.Droid
{
    public class SwipeRevealLayout : ViewGroup
    {
        // These states are used only for ViewBindHelper
        internal const int STATE_CLOSE = 0;
        internal const int STATE_CLOSING = 1;
        internal const int STATE_OPEN = 2;
        internal const int STATE_OPENING = 3;
        internal const int STATE_DRAGGING = 4;

        private const int DEFAULT_MIN_FLING_VELOCITY = 300; // dp per second
        private const int DEFAULT_MIN_DIST_REQUEST_DISALLOW_PARENT = 1; // dp

        public const int DRAG_EDGE_LEFT = 0x1;
        public const int DRAG_EDGE_RIGHT = 0x1 << 1;
        public const int DRAG_EDGE_TOP = 0x1 << 2;
        public const int DRAG_EDGE_BOTTOM = 0x1 << 3;

        /**
         * The secondary view will be under the main view.
         */
        public const int MODE_NORMAL = 0;

        /**
         * The secondary view will stick the edge of the main view.
         */
        public const int MODE_SAME_LEVEL = 1;

        /**
         * Main view is the view which is shown when the layout is closed.
         */
        private View mMainView;

        /**
         * Secondary view is the view which is shown when the layout is opened.
         */
        private View mSecondaryView;

        /**
         * The rectangle position of the main view when the layout is closed.
         */
        private Rect mRectMainClose = new Rect();

        /**
         * The rectangle position of the main view when the layout is opened.
         */
        private Rect mRectMainOpen = new Rect();

        /**
         * The rectangle position of the secondary view when the layout is closed.
         */
        private Rect mRectSecClose = new Rect();

        /**
         * The rectangle position of the secondary view when the layout is opened.
         */
        private Rect mRectSecOpen = new Rect();

        /**
         * The minimum distance (px) to the closest drag edge that the SwipeRevealLayout
         * will disallow the parent to intercept touch event.
         */
        private int mMinDistRequestDisallowParent = 0;

        private bool mIsOpenBeforeInit = false;
        private volatile bool mAborted = false;
        private volatile bool mIsScrolling = false;
        private volatile bool mLockDrag = false;

        private int mMinFlingVelocity = DEFAULT_MIN_FLING_VELOCITY;
        private int mState = STATE_CLOSE;
        private int mMode = MODE_NORMAL;

        private int mLastMainLeft = 0;
        private int mLastMainTop = 0;

        private int mDragEdge = DRAG_EDGE_LEFT;

        private ViewDragHelper mDragHelper;
        private GestureDetectorCompat mGestureDetector;

        private DragStateChangeListener mDragStateChangeListener; // only used for ViewBindHelper
        private SwipeListener mSwipeListener;

        private int mOnLayoutCount = 0;

        internal interface DragStateChangeListener
        {
            void onDragStateChanged(int state);
        }

        /**
         * Listener for monitoring events about swipe layout.
         */
        public interface SwipeListener
        {
            /**
             * Called when the main view becomes completely closed.
             */
            void onClosed(SwipeRevealLayout view);

            /**
             * Called when the main view becomes completely opened.
             */
            void onOpened(SwipeRevealLayout view);

            /**
             * Called when the main view's position changes.
             * @param slideOffset The new offset of the main view within its range, from 0-1
             */
            void onSlide(SwipeRevealLayout view, float slideOffset);
        }

        /**
         * No-op stub for {@link SwipeListener}. If you only want ot implement a subset
         * of the listener methods, you can extend this instead of implement the full interface.
         */
        public class SimpleSwipeListener : SwipeListener
        {
            public void onClosed(SwipeRevealLayout view)
            {
            }

            public void onOpened(SwipeRevealLayout view)
            {
            }

            public void onSlide(SwipeRevealLayout view, float slideOffset)
            {
            }
        }

        public SwipeRevealLayout(Context context)
            : base(context)
        {
            init(context, null);
        }

        public SwipeRevealLayout(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            init(context, attrs);
        }

        public SwipeRevealLayout(Context context, IAttributeSet attrs, int defStyleAttr)
            : base(context, attrs, defStyleAttr)
        {
            init(context, attrs);
        }
        public override bool OnTouchEvent(MotionEvent motionEvent)
        {
            return CoreUtility.ExecuteFunction("OnTouchEvent", delegate ()
            {
                mGestureDetector.OnTouchEvent(motionEvent);
                mDragHelper.ProcessTouchEvent(motionEvent);
                return true;
            });
        }
        public override bool OnInterceptTouchEvent(MotionEvent motionEvent)
        {
            return CoreUtility.ExecuteFunction("ExecuteFunction", delegate ()
            {
                mDragHelper.ProcessTouchEvent(motionEvent);
                mGestureDetector.OnTouchEvent(motionEvent);

                bool settling = mDragHelper.ViewDragState == ViewDragHelper.StateSettling;
                bool idleAfterScrolled = mDragHelper.ViewDragState == ViewDragHelper.StateIdle
                        && mIsScrolling;

                return settling || idleAfterScrolled;
            });

        }

        protected override void OnFinishInflate()
        {
            CoreUtility.ExecuteMethod("OnFinishInflate", delegate ()
            {
                base.OnFinishInflate();

                // get views
                if(ChildCount >= 2)
                {
                    mSecondaryView = GetChildAt(0);
                    mMainView = GetChildAt(1);
                }
                else if(ChildCount == 1)
                {
                    mMainView = GetChildAt(0);
                }
            });
           
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            try
            {

                mAborted = false;

                for(int index = 0; index < ChildCount; index++)
                {
                    View child = GetChildAt(index);

                    int left, right, top, bottom;
                    left = right = top = bottom = 0;

                    int minLeft = PaddingLeft;
                    int maxRight = Math.Max(r - PaddingRight - l, 0);
                    int minTop = PaddingTop;
                    int maxBottom = Math.Max(b - PaddingBottom - t, 0);

                    int measuredChildHeight = child.MeasuredHeight;
                    int measuredChildWidth = child.MeasuredWidth;

                    // need to take account if child size is match_parent
                    LayoutParams childParams = child.LayoutParameters;
                    bool matchParentHeight = false;
                    bool matchParentWidth = false;

                    if(childParams != null)
                    {
                        matchParentHeight = (childParams.Height == LayoutParams.MatchParent) ||
                                (childParams.Height == LayoutParams.FillParent);
                        matchParentWidth = (childParams.Width == LayoutParams.MatchParent) ||
                                (childParams.Width == LayoutParams.FillParent);
                    }

                    if(matchParentHeight)
                    {
                        measuredChildHeight = maxBottom - minTop;
                        childParams.Height = measuredChildHeight;
                    }

                    if(matchParentWidth)
                    {
                        measuredChildWidth = maxRight - minLeft;
                        childParams.Width = measuredChildWidth;
                    }

                    switch(mDragEdge)
                    {
                        case DRAG_EDGE_RIGHT:
                            left = Math.Max(r - measuredChildWidth - PaddingRight - l, minLeft);
                            top = Math.Min(PaddingTop, maxBottom);
                            right = Math.Max(r - PaddingRight - l, minLeft);
                            bottom = Math.Min(measuredChildHeight + PaddingTop, maxBottom);
                            break;

                        case DRAG_EDGE_LEFT:
                            left = Math.Min(PaddingLeft, maxRight);
                            top = Math.Min(PaddingTop, maxBottom);
                            right = Math.Min(measuredChildWidth + PaddingLeft, maxRight);
                            bottom = Math.Min(measuredChildHeight + PaddingTop, maxBottom);
                            break;

                        case DRAG_EDGE_TOP:
                            left = Math.Min(PaddingLeft, maxRight);
                            top = Math.Min(PaddingTop, maxBottom);
                            right = Math.Min(measuredChildWidth + PaddingLeft, maxRight);
                            bottom = Math.Min(measuredChildHeight + PaddingTop, maxBottom);
                            break;

                        case DRAG_EDGE_BOTTOM:
                            left = Math.Min(PaddingLeft, maxRight);
                            top = Math.Max(b - measuredChildHeight - PaddingBottom - t, minTop);
                            right = Math.Min(measuredChildWidth + PaddingLeft, maxRight);
                            bottom = Math.Max(b - PaddingBottom - t, minTop);
                            break;
                    }

                    child.Layout(left, top, right, bottom);
                }

                // taking account offset when mode is SAME_LEVEL
                if(mMode == MODE_SAME_LEVEL)
                {
                    switch(mDragEdge)
                    {
                        case DRAG_EDGE_LEFT:
                            mSecondaryView.OffsetLeftAndRight(-mSecondaryView.Width);
                            break;

                        case DRAG_EDGE_RIGHT:
                            mSecondaryView.OffsetLeftAndRight(mSecondaryView.Width);
                            break;

                        case DRAG_EDGE_TOP:
                            mSecondaryView.OffsetTopAndBottom(-mSecondaryView.Height);
                            break;

                        case DRAG_EDGE_BOTTOM:
                            mSecondaryView.OffsetTopAndBottom(mSecondaryView.Height);
                            break;
                    }
                }

                initRects();

                if(mIsOpenBeforeInit)
                {
                    open(false);
                }
                else
                {
                    close(false);
                }

                mLastMainLeft = mMainView.Left;
                mLastMainTop = mMainView.Top;

                mOnLayoutCount++;
            }
            catch(Exception ex)
            {

            }

        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            CoreUtility.ExecuteMethod("OnMeasure", delegate ()
            {

                LayoutParams parameters = LayoutParameters;

                MeasureSpecMode widthMode = MeasureSpec.GetMode(widthMeasureSpec);
                MeasureSpecMode heightMode = MeasureSpec.GetMode(heightMeasureSpec);

                int desiredWidth = 0;
                int desiredHeight = 0;

                // first find the largest child
                for(int i = 0; i < ChildCount; i++)
                {
                    View child = GetChildAt(i);

                    if(i == 0)
                    {
                        LayoutParams childParams = child.LayoutParameters;
                        childParams.Height = LayoutParams.MatchParent;
                        child.LayoutParameters = childParams;
                    }
                    MeasureChild(child, widthMeasureSpec, heightMeasureSpec);

                    if(i != 0)
                    {
                        desiredWidth = Math.Max(child.MeasuredWidth, desiredWidth);
                        desiredHeight = Math.Max(child.MeasuredHeight, desiredHeight);
                    }
                }
                // create new measure spec using the primary child 
                widthMeasureSpec = MeasureSpec.MakeMeasureSpec(desiredWidth, widthMode);
                heightMeasureSpec = MeasureSpec.MakeMeasureSpec(desiredHeight, heightMode);

                int measuredWidth = MeasureSpec.GetSize(widthMeasureSpec);
                int measuredHeight = MeasureSpec.GetSize(heightMeasureSpec);

                for(int i = 0; i < ChildCount; i++)
                {
                    View child = GetChildAt(i);
                    LayoutParams childParams = child.LayoutParameters;

                    if(childParams != null)
                    {
                        if(childParams.Height == LayoutParams.MatchParent)
                        {
                            child.SetMinimumHeight(measuredHeight);
                        }

                        if(childParams.Width == LayoutParams.MatchParent)
                        {
                            child.SetMinimumWidth(measuredWidth);
                        }
                    }

                    MeasureChild(child, widthMeasureSpec, heightMeasureSpec);
                    desiredWidth = Math.Max(child.MeasuredWidth, desiredWidth);
                    desiredHeight = Math.Max(child.MeasuredHeight, desiredHeight);
                }

                // taking accounts of padding
                desiredWidth += PaddingLeft + PaddingRight;
                desiredHeight += PaddingTop + PaddingBottom;

                // adjust desired width
                if(widthMode == MeasureSpecMode.Exactly)
                {
                    desiredWidth = measuredWidth;
                }
                else
                {
                    if(parameters.Width == LayoutParams.MatchParent)
                    {
                        desiredWidth = measuredWidth;
                    }

                    if(widthMode == MeasureSpecMode.AtMost)
                    {
                        desiredWidth = (desiredWidth > measuredWidth) ? measuredWidth : desiredWidth;
                    }
                }

                // adjust desired height
                if(heightMode == MeasureSpecMode.Exactly)
                {
                    desiredHeight = measuredHeight;
                }
                else
                {
                    if(parameters.Height == LayoutParams.MatchParent)
                    {
                        desiredHeight = measuredHeight;
                    }

                    if(heightMode == MeasureSpecMode.AtMost)
                    {
                        desiredHeight = (desiredHeight > measuredHeight) ? measuredHeight : desiredHeight;
                    }
                }

                SetMeasuredDimension(desiredWidth, desiredHeight);
            });

        }

        public override void ComputeScroll()
        {
            try
            {
                if (mDragHelper.ContinueSettling(true))
                {
                    ViewCompat.PostInvalidateOnAnimation(this);
                }
            }
            catch
            {
            }
            
        }


        /**
         * Open the panel to show the secondary view
         * @param animation true to animate the open motion. {@link SwipeListener} won't be
         *                  called if is animation is false.
         */
        public void open(bool animation)
        {
            CoreUtility.ExecuteMethod("open", delegate ()
            {
                mIsOpenBeforeInit = true;
                mAborted = false;

                if(animation)
                {
                    mState = STATE_OPENING;
                    mDragHelper.SmoothSlideViewTo(mMainView, mRectMainOpen.Left, mRectMainOpen.Top);

                    if(mDragStateChangeListener != null)
                    {
                        mDragStateChangeListener.onDragStateChanged(mState);
                    }
                }
                else
                {
                    mState = STATE_OPEN;
                    mDragHelper.Abort();

                    mMainView.Layout(
                            mRectMainOpen.Left,
                            mRectMainOpen.Top,
                            mRectMainOpen.Right,
                            mRectMainOpen.Bottom
                    );

                    mSecondaryView.Layout(
                            mRectSecOpen.Left,
                            mRectSecOpen.Top,
                            mRectSecOpen.Right,
                            mRectSecOpen.Bottom
                    );
                }

                ViewCompat.PostInvalidateOnAnimation(this);
            });

        }

        /**
         * Close the panel to hide the secondary view
         * @param animation true to animate the close motion. {@link SwipeListener} won't be
         *                  called if is animation is false.
         */
        public void close(bool animation)
        {
            CoreUtility.ExecuteMethod("close", delegate ()
            {
                mIsOpenBeforeInit = false;
                mAborted = false;

                if(animation)
                {
                    mState = STATE_CLOSING;
                    mDragHelper.SmoothSlideViewTo(mMainView, mRectMainClose.Left, mRectMainClose.Top);

                    if(mDragStateChangeListener != null)
                    {
                        mDragStateChangeListener.onDragStateChanged(mState);
                    }

                }
                else
                {
                    mState = STATE_CLOSE;
                    mDragHelper.Abort();

                    mMainView.Layout(
                            mRectMainClose.Left,
                            mRectMainClose.Top,
                            mRectMainClose.Right,
                            mRectMainClose.Bottom
                    );

                    mSecondaryView.Layout(
                            mRectSecClose.Left,
                            mRectSecClose.Top,
                            mRectSecClose.Right,
                            mRectSecClose.Bottom
                    );
                }

                ViewCompat.PostInvalidateOnAnimation(this);
            });

        }

        /**
         * Set the minimum fling velocity to cause the layout to open/close.
         * @param velocity dp per second
         */
        public void setMinFlingVelocity(int velocity)
        {
            mMinFlingVelocity = velocity;
        }

        /**
         * Get the minimum fling velocity to cause the layout to open/close.
         * @return dp per second
         */
        public int getMinFlingVelocity()
        {
            return mMinFlingVelocity;
        }

        /**
         * Set the edge where the layout can be dragged from.
         * @param dragEdge Can be one of these
         *                 <ul>
         *                      <li>{@link #DRAG_EDGE_LEFT}</li>
         *                      <li>{@link #DRAG_EDGE_TOP}</li>
         *                      <li>{@link #DRAG_EDGE_RIGHT}</li>
         *                      <li>{@link #DRAG_EDGE_BOTTOM}</li>
         *                 </ul>
         */
        public void setDragEdge(int dragEdge)
        {
            mDragEdge = dragEdge;
        }

        /**
         * Get the edge where the layout can be dragged from.
         * @return Can be one of these
         *                 <ul>
         *                      <li>{@link #DRAG_EDGE_LEFT}</li>
         *                      <li>{@link #DRAG_EDGE_TOP}</li>
         *                      <li>{@link #DRAG_EDGE_RIGHT}</li>
         *                      <li>{@link #DRAG_EDGE_BOTTOM}</li>
         *                 </ul>
         */
        public int getDragEdge()
        {
            return mDragEdge;
        }

        public void setSwipeListener(SwipeListener listener)
        {
            mSwipeListener = listener;
        }

        /**
         * @param lock if set to true, the user cannot drag/swipe the layout.
         */
        public void setLockDrag(bool lockDrag)
        {
            mLockDrag = lockDrag;
        }

        /**
         * @return true if the drag/swipe motion is currently locked.
         */
        public bool isDragLocked()
        {
            return mLockDrag;
        }

        /**
         * @return true if layout is fully opened, false otherwise.
         */
        public bool isOpened()
        {
            return (mState == STATE_OPEN);
        }

        /**
         * @return true if layout is fully closed, false otherwise.
         */
        public bool isClosed()
        {
            return (mState == STATE_CLOSE);
        }

        /** Only used for {@link ViewBinderHelper} */
        internal void setDragStateChangeListener(DragStateChangeListener listener)
        {
            mDragStateChangeListener = listener;
        }

        /** Abort current motion in progress. Only used for {@link ViewBinderHelper} */
        internal void abort()
        {
            mAborted = true;
            mDragHelper.Abort();
        }

        /**
         * In RecyclerView/ListView, onLayout should be called 2 times to display children views correctly.
         * This method check if it've already called onLayout two times.
         * @return true if you should call {@link #requestLayout()}.
         */
        internal bool shouldRequestLayout()
        {
            return mOnLayoutCount < 2;
        }


        private int getMainOpenLeft()
        {
            switch (mDragEdge)
            {
                case DRAG_EDGE_LEFT:
                    return mRectMainClose.Left + mSecondaryView.Width;

                case DRAG_EDGE_RIGHT:
                    return mRectMainClose.Left - mSecondaryView.Width;

                case DRAG_EDGE_TOP:
                    return mRectMainClose.Left;

                case DRAG_EDGE_BOTTOM:
                    return mRectMainClose.Left;

                default:
                    return 0;
            }
        }

        private int getMainOpenTop()
        {
            switch (mDragEdge)
            {
                case DRAG_EDGE_LEFT:
                    return mRectMainClose.Top;

                case DRAG_EDGE_RIGHT:
                    return mRectMainClose.Top;

                case DRAG_EDGE_TOP:
                    return mRectMainClose.Top + mSecondaryView.Height;

                case DRAG_EDGE_BOTTOM:
                    return mRectMainClose.Top - mSecondaryView.Height;

                default:
                    return 0;
            }
        }

        private int getSecOpenLeft()
        {
            if (mMode == MODE_NORMAL || mDragEdge == DRAG_EDGE_BOTTOM || mDragEdge == DRAG_EDGE_TOP)
            {
                return mRectSecClose.Left;
            }

            if (mDragEdge == DRAG_EDGE_LEFT)
            {
                return mRectSecClose.Left + mSecondaryView.Width;
            }
            else
            {
                return mRectSecClose.Left - mSecondaryView.Width;
            }
        }

        private int getSecOpenTop()
        {
            if (mMode == MODE_NORMAL || mDragEdge == DRAG_EDGE_LEFT || mDragEdge == DRAG_EDGE_RIGHT)
            {
                return mRectSecClose.Top;
            }

            if (mDragEdge == DRAG_EDGE_TOP)
            {
                return mRectSecClose.Top + mSecondaryView.Height;
            }
            else
            {
                return mRectSecClose.Top - mSecondaryView.Height;
            }
        }

        private void initRects()
        {
            CoreUtility.ExecuteMethod("initRects", delegate ()
            {
                // close position of main view
                mRectMainClose.Set(
                        mMainView.Left,
                        mMainView.Top,
                        mMainView.Right,
                        mMainView.Bottom
                );

                // close position of secondary view
                mRectSecClose.Set(
                        mSecondaryView.Left,
                        mSecondaryView.Top,
                        mSecondaryView.Right,
                        mSecondaryView.Bottom
                );

                // open position of the main view
                mRectMainOpen.Set(
                        getMainOpenLeft(),
                        getMainOpenTop(),
                        getMainOpenLeft() + mMainView.Width,
                        getMainOpenTop() + mMainView.Height
                );

                // open position of the secondary view
                mRectSecOpen.Set(
                        getSecOpenLeft(),
                        getSecOpenTop(),
                        getSecOpenLeft() + mSecondaryView.Width,
                        getSecOpenTop() + mSecondaryView.Height
                );
            });

        }

        private void init(Context context, IAttributeSet attrs)
        {
            CoreUtility.ExecuteMethod("init", delegate ()
            {
                mGestureListener = new CustomGestureListener(this);
                mDragHelperCallback = new CustomViewDragHelper(this);

                if(attrs != null && context != null)
                {
                    TypedArray a = context.Theme.ObtainStyledAttributes(
                            attrs,
                            Resource.Styleable.SwipeRevealLayout,
                            0, 0
                    );

                    mDragEdge = a.GetInteger(Resource.Styleable.SwipeRevealLayout_dragEdge, DRAG_EDGE_LEFT);
                    mMinFlingVelocity = a.GetInteger(Resource.Styleable.SwipeRevealLayout_flingVelocity, DEFAULT_MIN_FLING_VELOCITY);
                    mMode = a.GetInteger(Resource.Styleable.SwipeRevealLayout_mode, MODE_NORMAL);

                    mMinDistRequestDisallowParent = a.GetDimensionPixelSize(
                            Resource.Styleable.SwipeRevealLayout_minDistRequestDisallowParent,
                            dpToPx(DEFAULT_MIN_DIST_REQUEST_DISALLOW_PARENT)
                    );
                }

                mDragHelper = ViewDragHelper.Create(this, 1.0f, mDragHelperCallback);
                mDragHelper.SetEdgeTrackingEnabled(ViewDragHelper.EdgeAll);

                mGestureDetector = new GestureDetectorCompat(context, mGestureListener);
            });

        }

        private CustomGestureListener mGestureListener;
        private class CustomGestureListener : GestureDetector.SimpleOnGestureListener
        {
            public CustomGestureListener(SwipeRevealLayout layout)
            {
                _layout = layout;
            }
            private bool hasDisallowed;
            private SwipeRevealLayout _layout;

            public override bool OnDown(MotionEvent e)
            {
                _layout.mIsScrolling = false;
                hasDisallowed = false;
                return true;
            }
            public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
            {
                _layout.mIsScrolling = true;
                return false;
            }
            public override bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
            {
                _layout.mIsScrolling = true;

                if (_layout.Parent != null)
                {
                    bool shouldDisallow;

                    if (!hasDisallowed)
                    {
                        shouldDisallow = getDistToClosestEdge() >= _layout.mMinDistRequestDisallowParent;
                        if (shouldDisallow)
                        {
                            hasDisallowed = true;
                        }
                    }
                    else
                    {
                        shouldDisallow = true;
                    }

                    // disallow parent to intercept touch event so that the layout will work
                    // properly on RecyclerView or view that handles scroll gesture.
                    _layout.Parent.RequestDisallowInterceptTouchEvent(shouldDisallow);
                }

                return false;
            }

            private int getDistToClosestEdge()
            {
                switch (_layout.mDragEdge)
                {
                    case DRAG_EDGE_LEFT:
                        int pivotRight = _layout.mRectMainClose.Left + _layout.mSecondaryView.Width;

                        return Math.Min(
                                 _layout.mMainView.Left - _layout.mRectMainClose.Left,
                                pivotRight - _layout.mMainView.Left
                        );

                    case DRAG_EDGE_RIGHT:
                        int pivotLeft = _layout.mRectMainClose.Right - _layout.mSecondaryView.Width;

                        return Math.Min(
                                _layout.mMainView.Right - pivotLeft,
                                _layout.mRectMainClose.Right - _layout.mMainView.Right
                        );

                    case DRAG_EDGE_TOP:
                        int pivotBottom = _layout.mRectMainClose.Top + _layout.mSecondaryView.Height;

                        return Math.Min(
                                 _layout.mMainView.Bottom - pivotBottom,
                                pivotBottom - _layout.mMainView.Top
                        );

                    case DRAG_EDGE_BOTTOM:
                        int pivotTop = _layout.mRectMainClose.Bottom - _layout.mSecondaryView.Height;

                        return Math.Min(
                                 _layout.mRectMainClose.Bottom - _layout.mMainView.Bottom,
                                 _layout.mMainView.Bottom - pivotTop
                        );
                }

                return 0;
            }

        }


        private int getHalfwayPivotHorizontal()
        {
            if (mDragEdge == DRAG_EDGE_LEFT)
            {
                return mRectMainClose.Left + mSecondaryView.Width / 2;
            }
            else
            {
                return mRectMainClose.Right - mSecondaryView.Width / 2;
            }
        }

        private int getHalfwayPivotVertical()
        {
            if (mDragEdge == DRAG_EDGE_TOP)
            {
                return mRectMainClose.Top + mSecondaryView.Height / 2;
            }
            else
            {
                return mRectMainClose.Bottom - mSecondaryView.Height / 2;
            }
        }




        public static String getStateString(int state)
        {
            switch (state)
            {
                case STATE_CLOSE:
                    return "state_close";

                case STATE_CLOSING:
                    return "state_closing";

                case STATE_OPEN:
                    return "state_open";

                case STATE_OPENING:
                    return "state_opening";

                case STATE_DRAGGING:
                    return "state_dragging";

                default:
                    return "undefined";
            }
        }

        private CustomViewDragHelper mDragHelperCallback;

        public class CustomViewDragHelper : ViewDragHelper.Callback
        {
            public CustomViewDragHelper(SwipeRevealLayout layout)
            {
                _layout = layout;
            }
            private SwipeRevealLayout _layout;

            public override bool TryCaptureView(View child, int pointerId)
            {
                _layout.mAborted = false;

                if (_layout.mLockDrag)
                    return false;

                _layout.mDragHelper.CaptureChildView(_layout.mMainView, pointerId);
                return false;
            }

            public override int ClampViewPositionVertical(View child, int top, int dy)
            {
                switch (_layout.mDragEdge)
                {
                    case DRAG_EDGE_TOP:
                        return Math.Max(
                                Math.Min(top, _layout.mRectMainClose.Top + _layout.mSecondaryView.Height),
                                 _layout.mRectMainClose.Top
                        );

                    case DRAG_EDGE_BOTTOM:
                        return Math.Max(
                                Math.Min(top, _layout.mRectMainClose.Top),
                                 _layout.mRectMainClose.Top - _layout.mSecondaryView.Height
                        );

                    default:
                        return child.Top;
                }
            }
            public override int ClampViewPositionHorizontal(View child, int left, int dx)
            {
                switch (_layout.mDragEdge)
                {
                    case DRAG_EDGE_RIGHT:
                        return Math.Max(
                                Math.Min(left, _layout.mRectMainClose.Left),
                                 _layout.mRectMainClose.Left - _layout.mSecondaryView.Width
                        );

                    case DRAG_EDGE_LEFT:
                        return Math.Max(
                                Math.Min(left, _layout.mRectMainClose.Left + _layout.mSecondaryView.Width),
                                 _layout.mRectMainClose.Left
                        );

                    default:
                        return child.Left;
                }
            }

            public override void OnViewReleased(View releasedChild, float xvel, float yvel)
            {
                bool velRightExceeded = _layout.pxToDp((int)xvel) >= _layout.mMinFlingVelocity;
                bool velLeftExceeded = _layout.pxToDp((int)xvel) <= -_layout.mMinFlingVelocity;
                bool velUpExceeded = _layout.pxToDp((int)yvel) <= -_layout.mMinFlingVelocity;
                bool velDownExceeded = _layout.pxToDp((int)yvel) >= _layout.mMinFlingVelocity;

                int pivotHorizontal = _layout.getHalfwayPivotHorizontal();
                int pivotVertical = _layout.getHalfwayPivotVertical();

                switch (_layout.mDragEdge)
                {
                    case DRAG_EDGE_RIGHT:
                        if (velRightExceeded)
                        {
                            _layout.close(true);
                        }
                        else if (velLeftExceeded)
                        {
                            _layout.open(true);
                        }
                        else
                        {
                            if (_layout.mMainView.Right < pivotHorizontal)
                            {
                                _layout.open(true);
                            }
                            else
                            {
                                _layout.close(true);
                            }
                        }
                        break;

                    case DRAG_EDGE_LEFT:
                        if (velRightExceeded)
                        {
                            _layout.open(true);
                        }
                        else if (velLeftExceeded)
                        {
                            _layout.close(true);
                        }
                        else
                        {
                            if (_layout.mMainView.Left < pivotHorizontal)
                            {
                                _layout.close(true);
                            }
                            else
                            {
                                _layout.open(true);
                            }
                        }
                        break;

                    case DRAG_EDGE_TOP:
                        if (velUpExceeded)
                        {
                            _layout.close(true);
                        }
                        else if (velDownExceeded)
                        {
                            _layout.open(true);
                        }
                        else
                        {
                            if (_layout.mMainView.Top < pivotVertical)
                            {
                                _layout.close(true);
                            }
                            else
                            {
                                _layout.open(true);
                            }
                        }
                        break;

                    case DRAG_EDGE_BOTTOM:
                        if (velUpExceeded)
                        {
                            _layout.open(true);
                        }
                        else if (velDownExceeded)
                        {
                            _layout.close(true);
                        }
                        else
                        {
                            if (_layout.mMainView.Bottom < pivotVertical)
                            {
                                _layout.open(true);
                            }
                            else
                            {
                                _layout.close(true);
                            }
                        }
                        break;
                }
            }
            public override void OnEdgeDragStarted(int edgeFlags, int pointerId)
            {
                base.OnEdgeDragStarted(edgeFlags, pointerId);

                if (_layout.mLockDrag)
                {
                    return;
                }

                bool edgeStartLeft = (_layout.mDragEdge == DRAG_EDGE_RIGHT)
                        && edgeFlags == ViewDragHelper.EdgeLeft;

                bool edgeStartRight = (_layout.mDragEdge == DRAG_EDGE_LEFT)
                        && edgeFlags == ViewDragHelper.EdgeRight;

                bool edgeStartTop = (_layout.mDragEdge == DRAG_EDGE_BOTTOM)
                        && edgeFlags == ViewDragHelper.EdgeTop;

                bool edgeStartBottom = (_layout.mDragEdge == DRAG_EDGE_TOP)
                        && edgeFlags == ViewDragHelper.EdgeBottom;

                if (edgeStartLeft || edgeStartRight || edgeStartTop || edgeStartBottom)
                {
                    _layout.mDragHelper.CaptureChildView(_layout.mMainView, pointerId);
                }
            }
            public override void OnViewPositionChanged(View changedView, int left, int top, int dx, int dy)
            {
                base.OnViewPositionChanged(changedView, left, top, dx, dy);
                if (_layout.mMode == MODE_SAME_LEVEL)
                {
                    if (_layout.mDragEdge == DRAG_EDGE_LEFT || _layout.mDragEdge == DRAG_EDGE_RIGHT)
                    {
                        _layout.mSecondaryView.OffsetLeftAndRight(dx);
                    }
                    else
                    {
                        _layout.mSecondaryView.OffsetTopAndBottom(dy);
                    }
                }

                bool isMoved = (_layout.mMainView.Left != _layout.mLastMainLeft) || (_layout.mMainView.Top != _layout.mLastMainTop);
                if (_layout.mSwipeListener != null && isMoved)
                {
                    if (_layout.mMainView.Left == _layout.mRectMainClose.Left && _layout.mMainView.Top == _layout.mRectMainClose.Top)
                    {
                        _layout.mSwipeListener.onClosed(_layout);
                    }
                    else if (_layout.mMainView.Left == _layout.mRectMainOpen.Left && _layout.mMainView.Top == _layout.mRectMainOpen.Top)
                    {
                        _layout.mSwipeListener.onOpened(_layout);
                    }
                    else
                    {
                        _layout.mSwipeListener.onSlide(_layout, getSlideOffset());
                    }
                }

                _layout.mLastMainLeft = _layout.mMainView.Left;
                _layout.mLastMainTop = _layout.mMainView.Top;
                ViewCompat.PostInvalidateOnAnimation(_layout);
            }


            private float getSlideOffset()
            {
                switch (_layout.mDragEdge)
                {
                    case DRAG_EDGE_LEFT:
                        return (float)(_layout.mMainView.Left - _layout.mRectMainClose.Left) / _layout.mSecondaryView.Width;

                    case DRAG_EDGE_RIGHT:
                        return (float)(_layout.mRectMainClose.Left - _layout.mMainView.Left) / _layout.mSecondaryView.Width;

                    case DRAG_EDGE_TOP:
                        return (float)(_layout.mMainView.Top - _layout.mRectMainClose.Top) / _layout.mSecondaryView.Height;

                    case DRAG_EDGE_BOTTOM:
                        return (float)(_layout.mRectMainClose.Top - _layout.mMainView.Top) / _layout.mSecondaryView.Height;

                    default:
                        return 0;
                }
            }

            public override void OnViewDragStateChanged(int state)
            {
                base.OnViewDragStateChanged(state);
                int prevState = _layout.mState;

                switch (state)
                {
                    case ViewDragHelper.StateDragging:
                        _layout.mState = STATE_DRAGGING;
                        break;

                    case ViewDragHelper.StateIdle:

                        // drag edge is left or right
                        if (_layout.mDragEdge == DRAG_EDGE_LEFT || _layout.mDragEdge == DRAG_EDGE_RIGHT)
                        {
                            if (_layout.mMainView.Left == _layout.mRectMainClose.Left)
                            {
                                _layout.mState = STATE_CLOSE;
                            }
                            else
                            {
                                _layout.mState = STATE_OPEN;
                            }
                        }

                        // drag edge is top or bottom
                        else
                        {
                            if (_layout.mMainView.Top == _layout.mRectMainClose.Top)
                            {
                                _layout.mState = STATE_CLOSE;
                            }
                            else
                            {
                                _layout.mState = STATE_OPEN;
                            }
                        }
                        break;
                }

                if (_layout.mDragStateChangeListener != null && !_layout.mAborted && prevState != _layout.mState)
                {
                    _layout.mDragStateChangeListener.onDragStateChanged(_layout.mState);
                }
            }
        }



        private int pxToDp(int px)
        {
            Resources resources = Context.Resources;
            DisplayMetrics metrics = resources.DisplayMetrics;
            return (int)(px / ((float)metrics.DensityDpi / (float)DisplayMetrics.DensityDefault));
        }

        private int dpToPx(int dp)
        {
            Resources resources = Context.Resources;
            DisplayMetrics metrics = resources.DisplayMetrics;
            return (int)(dp * ((float)metrics.DensityDpi / (float)DisplayMetrics.DensityDefault));
        }
    }
}