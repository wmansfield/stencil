using System;
using UIKit;
using System.Collections.Generic;
using Foundation;
using System.Collections;
using CoreGraphics;
using CoreAnimation;
using Stencil.Native.Core;

namespace Stencil.Native.iOS.Core.Data
{
    [Obsolete("this is really old code, built before understanding, needs refactoring", false)]
    public class CoreCollectionSource : UICollectionViewDataSource
    {
        public CoreCollectionSource()
        {

        }

        public virtual List<object> Items { get; set; }

        public virtual bool DisableItemDeselection { get; set; }
        public const string DRAG_ANIMATION_NAME = "DragHideBars";

        /// <summary>
        /// If set, during scrolling, it will auto hide.
        /// Keep protected since we need event handlers with it
        /// </summary>
        protected virtual UITabBarController AutoHideTabBarController { get; set; }
        protected virtual BaseUIViewController AutoHideControllerInstance { get; set; }
        public virtual bool AutoHideTabBarShouldBeHidden { get; set; }
        protected virtual bool SuspendTabBarTracking { get; set; }
        public virtual bool DisableTabBarTracking { get; set; }

        protected virtual CGRect? OriginalTabBarFrame { get; set; }
        protected virtual nfloat TabBarLastOffsetY { get; set; }
        protected virtual Func<UICollectionView, int> CustomNumberOfSections { get; set; }
        protected virtual Func<UICollectionView, object, NSIndexPath, UICollectionViewCell> CustomCellCreator { get; set; }
        protected virtual Func<UICollectionView, NSString, object, NSIndexPath, UICollectionReusableView> CustomSupplementaryCreator { get; set; }
        protected virtual string CellIdentifier { get; set; }
        protected virtual Action<UICollectionViewCell, object, NSIndexPath> CustomCellBinding { get; set; }
        protected virtual Func<CoreCollectionSource, UICollectionView, int, UIView> CustomHeaderCreator { get; set; }
        protected virtual Func<CoreCollectionSource, UICollectionView, int, UIView> CustomFooterCreator { get; set; }
        protected virtual Action<object, NSIndexPath, UICollectionViewCell> OnSelectedMethod { get; set; }
        public virtual IScrollListener ScrollListener { get; set; }

        public override nint NumberOfSections(UICollectionView collectionView)
        {
            if (CustomNumberOfSections != null)
            {
                return CustomNumberOfSections(collectionView);
            }
            return 1;
        }
        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            if (this.Items != null)
            {
                return this.Items.Count;
            }
            return 0;
        }


        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            return CoreUtility.ExecuteFunction<UICollectionViewCell>("GetCell", delegate() 
            {
                object item = this.Items[indexPath.Row];
                UICollectionViewCell cell = null;

                if(this.CustomCellCreator != null)
                {
                    cell = this.CustomCellCreator(collectionView, item, indexPath);
                }
                if(cell == null)
                {
                    cell = collectionView.DequeueReusableCell(new NSString(this.CellIdentifier), indexPath) as UICollectionViewCell;
                }
                if(cell == null)
                {
                    cell = new UICollectionViewCell();
                }
                if(this.CustomCellBinding != null)
                {
                    this.CustomCellBinding(cell, item, indexPath);
                }
                return cell;
            });
        }

        public override UICollectionReusableView GetViewForSupplementaryElement(UICollectionView collectionView, NSString elementKind, NSIndexPath indexPath)
        {
            return CoreUtility.ExecuteFunction<UICollectionReusableView>("GetViewForSupplementaryElement", delegate() 
            {
                object item = null;
                if(this.Items.Count > 0)
                {
                    item = this.Items[indexPath.Row];
                }
                UICollectionReusableView view = null;

                if(this.CustomSupplementaryCreator != null)
                {
                    view = this.CustomSupplementaryCreator(collectionView, elementKind, item, indexPath);
                }

                return view;
            });
        }



        public T GetItem<T>(int id)
        {
            return CoreUtility.ExecuteFunction<T>("GetItem", delegate()
            {
                if (this.Items.Count >= id)
                {
                    return (T)this.Items[id];
                }
                return default(T);
            });
        }
        public void AppendItems(IEnumerable items)
        {
            CoreUtility.ExecuteMethod("AppendItems", delegate()
            {
                if(items != null)
                {
                    foreach (var item in items)
                    {
                        this.Items.Add(item);
                    }
                }
            });
        }
        public void ClearItems()
        {
            CoreUtility.ExecuteMethod("ClearItems", delegate()
            {
                this.Items.Clear();
            });
        }

        protected virtual void Scrolled_HandleTabBarHiding(UIScrollView scrollView)
        {
            CoreUtility.ExecuteMethod("Scrolled_HandleTabBarHiding", delegate()
            {
                if(this.AutoHideTabBarController == null)
                {
                    return; // ------- ShortCircuit
                }
                if(this.AutoHideControllerInstance != null)
                {
                    if(!this.AutoHideControllerInstance.IsControllerVisible)
                    {
                        return;
                    }
                }

                bool contentGoingUp = false;
                bool contentGoingDown = false;
                if (this.TabBarLastOffsetY < scrollView.ContentOffset.Y)
                {
                    contentGoingUp = true;
                }
                else if (this.TabBarLastOffsetY > scrollView.ContentOffset.Y) 
                {
                    contentGoingDown = true;
                }

                if(!contentGoingUp && !contentGoingDown) 
                {
                    return; // ------- ShortCircuit
                }
                if(contentGoingUp && AutoHideTabBarShouldBeHidden)
                {
                    return; // ------- ShortCircuit
                }
                if(contentGoingDown && !AutoHideTabBarShouldBeHidden)
                {
                    return; // ------- ShortCircuit
                }
                if(contentGoingUp)
                {
                    AnimateTabBar(false);
                }
                else if(contentGoingDown)
                {
                    AnimateTabBar(true);
                }
                else
                {
                    AnimateTabBar(null);
                }

            }, null, true);
        }

        /// <summary>
        /// Cloned in CoreTableSource
        /// </summary>
        public virtual void AnimateTabBar(bool? show, bool suspendTrackingDuringAnimation = false, bool force = false)
        {
            CoreUtility.ExecuteMethod("AnimateTabBar", delegate()
            {
                if(this.AutoHideControllerInstance != null)
                {
                    if(!this.AutoHideControllerInstance.IsControllerVisible)
                    {
                        return;
                    }
                }
                if(!force && (this.SuspendTabBarTracking || this.DisableTabBarTracking))
                {
                    return;
                }
                UITabBar tabBar = this.AutoHideTabBarController.TabBar;

                if(!OriginalTabBarFrame.HasValue)
                {
                    OriginalTabBarFrame = tabBar.Frame;
                }

                tabBar.Layer.RemoveAnimation(DRAG_ANIMATION_NAME);

                if(show.HasValue)
                {
                    if(suspendTrackingDuringAnimation)
                    {
                        this.SuspendTabBarTracking = true;
                    }
                    CATransaction.Begin();
                    CATransaction.CompletionBlock = new Action(delegate()
                    {
                        if(suspendTrackingDuringAnimation) // only ours
                        {
                            this.SuspendTabBarTracking = false;
                        }
                        this.BeginInvokeOnMainThread(delegate()
                        {
                            CoreUtility.ExecuteMethod("DraggingStarted.CompletionBlock", delegate() 
                            {
                                if(AutoHideTabBarShouldBeHidden)
                                {
                                    tabBar.Frame = new CGRect(tabBar.Frame.X, this.AutoHideTabBarController.View.Frame.Height, tabBar.Frame.Width, tabBar.Frame.Height);
                                }
                                else
                                {
                                    tabBar.Frame = OriginalTabBarFrame.GetValueOrDefault();
                                }
                                tabBar.Layer.RemoveAnimation(DRAG_ANIMATION_NAME);
                            });
                        });
                    });


                    if (!show.Value)
                    {
                        // content going up
                        AutoHideTabBarShouldBeHidden = true;

                        AnimationBuilder
                            .Begin(tabBar.Layer, DRAG_ANIMATION_NAME)
                            .MoveTo(new CGPoint(tabBar.Layer.Frame.X, this.AutoHideTabBarController.View.Frame.Height), 0f, 0.3f)
                            .Commit();
                    }
                    else if (show.Value) 
                    {
                        // content going down
                        AutoHideTabBarShouldBeHidden = false;

                        AnimationBuilder
                            .Begin(tabBar.Layer, DRAG_ANIMATION_NAME)
                            .MoveTo(OriginalTabBarFrame.Value.Location, 0f, 0.3f)
                            .Commit();
                    }

                    CATransaction.Commit();
                }
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ScrollListener = null;
                this.Items = null;
                this.OnSelectedMethod = null;
                this.CustomCellBinding = null;
                this.CustomCellCreator = null;
                this.CustomHeaderCreator = null;
                this.CustomFooterCreator = null;
                this.CustomNumberOfSections = null;

                this.AutoHideTabBarController = null;
            }
            base.Dispose(disposing);
        }


        #region Fluent Interface

        public CoreCollectionSource For(IEnumerable data)
        {
            this.Items = new List<object>();
            foreach (var item in data)
            {
                this.Items.Add(item);
            }
            return this;
        }

        public CoreCollectionSource WhenCreatingCell<T>(Func<UICollectionView, T, NSIndexPath, UICollectionViewCell> creator)
            where T : class
        {
            this.CustomCellCreator = delegate(UICollectionView arg1, object arg2, NSIndexPath arg3)
            {
                return creator(arg1, (T)arg2, arg3);
            };
            return this;
        }
        public CoreCollectionSource WhenCreatingSupplementary<T>(Func<UICollectionView, NSString, T, NSIndexPath, UICollectionReusableView> creator)
            where T : class
        {
            this.CustomSupplementaryCreator = delegate(UICollectionView arg1, NSString arg2, object arg3, NSIndexPath arg4)
            {
                return creator(arg1, arg2, (T)arg3, arg4);
            };
            return this;
        }
        public CoreCollectionSource WhenCreatingHeader(Func<CoreCollectionSource, UICollectionView, int, UIView> creator)
        {
            this.CustomHeaderCreator = creator;
            return this;
        }
        public CoreCollectionSource WhenCreatingFooter(Func<CoreCollectionSource, UICollectionView, int, UIView> creator)
        {
            this.CustomFooterCreator = creator;
            return this;
        }
        public CoreCollectionSource WhenItemSelected<T, K>(Action<T, K> selectedMethod)
            where K : UICollectionViewCell
        {
            this.OnSelectedMethod = delegate(object item, NSIndexPath index, UICollectionViewCell cell) 
            {
                selectedMethod((T)item,(K)cell);
            };
            return this;
        }
        public CoreCollectionSource HideTabsWhenScrollingDown(BaseUIViewController host, UITabBarController controller, CGRect? originalTabBarFrame = null)
        {
            if (controller == null)
            {
                return this;
            }
            this.AutoHideControllerInstance = host;
            this.AutoHideTabBarController = controller;
            CGRect frame = this.AutoHideTabBarController.TabBar.Frame;
            this.AutoHideTabBarController.TabBar.Layer.AnchorPoint = new CGPoint(0, 0);
            this.AutoHideTabBarController.TabBar.Frame = frame;
            if (originalTabBarFrame.HasValue)
            {
                this.OriginalTabBarFrame = originalTabBarFrame;
            }
            else
            {
                this.OriginalTabBarFrame = this.AutoHideTabBarController.TabBar.Frame;
            }
            return this;
        }
        public CoreCollectionSource WhenScrolling(IScrollListener scrollListener)
        {
            this.ScrollListener = scrollListener;
            return this;
        }
        #endregion
    }
}

