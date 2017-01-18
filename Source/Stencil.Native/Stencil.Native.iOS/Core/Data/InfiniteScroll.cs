using System;
using UIKit;
using System.Threading.Tasks;
using System.Threading;
using Stencil.Native.Core;

namespace Stencil.Native.iOS.Core.Data
{
    public class InfiniteScroll : BaseClass, IScrollListener
    {
        public InfiniteScroll(UITableView tableView, Func<IScrollListener, Task> getNextMethod, int refreshThreshold)
            : base("InfiniteScroll")
        {
            this.RefreshThreshold = refreshThreshold;
            this.TableView = tableView;
            this.GetNextMethod = getNextMethod;
            this.DelayMinimumMilliseconds = DEFAULT_DELAY_MILLISECONDS;
        }

        public InfiniteScroll(UICollectionView collectionView, Func<IScrollListener, Task> getNextMethod, int refreshThreshold)
            : base("InfiniteScroll")
        {
            this.RefreshThreshold = refreshThreshold;
            this.CollectionView = collectionView;
            this.GetNextMethod = getNextMethod;
            this.DelayMinimumMilliseconds = DEFAULT_DELAY_MILLISECONDS;
        }


        public const int DEFAULT_FETCH_THRESHOLD = 400;
        /// <summary>
        /// The min amount of time between loading the next element
        /// </summary>
        public const int DEFAULT_DELAY_MILLISECONDS = 400;

        /// <summary>
        /// The min amount of time between loading the next element
        /// </summary>
        public int DelayMinimumMilliseconds { get; set; }
        /// <summary>
        /// The amount of points near the end before refreshing
        /// </summary>
        public int RefreshThreshold { get; set; }
        public UITableView TableView { get; set; }
        public UICollectionView CollectionView { get; set; }
        /// <summary>
        /// Returns true if there is more data to get
        /// </summary>
        /// <value>The get next method.</value>
        public Func<IScrollListener, Task> GetNextMethod { get; set; }

        private int _tracker;
        private bool _isRetrieving;
        private static object _root = new object();
        private DateTime? _lastAttempt = null;
        public bool ListeningDisabled { get; set; }

        public void OnScrolled(UIScrollView scrollView)
        {
            base.ExecuteMethod("OnScrolled", delegate()
            {
                if(!ListeningDisabled)
                {
                    bool triggerCallBack = false;
                    nfloat frameHeight;
                    if(this.TableView != null)
                    {
                        frameHeight = TableView.Frame.Height;
                    }
                    else
                    {
                        frameHeight = CollectionView.Frame.Height;
                    }

                    if(scrollView.ContentSize.Height < frameHeight)
                    {
                        triggerCallBack = false; 
                    }
                    else
                    {
                        triggerCallBack = (scrollView.ContentOffset.Y > ((scrollView.ContentSize.Height - frameHeight) - RefreshThreshold));
                    }
                    if (triggerCallBack)
                    {
                        if (!_isRetrieving)
                        {
                            Task.Run(new Action(DoGetNextItem)).ConfigureAwait(false);
                        }
                    }
                }
            }, null, true);
        }

        private void DoGetNextItem()
        {
            base.ExecuteMethod("DoGetNextItem", delegate()
            {
                if (this._isRetrieving) { return; }
                if(_lastAttempt.HasValue)
                {
                    TimeSpan span = DateTime.UtcNow - _lastAttempt.Value;
                    if(this.DelayMinimumMilliseconds > span.TotalMilliseconds) 
                    { 
                        base.LogTrace("DoNext Skipped, too fast");
                        return; 
                    }
                }
                bool shouldGetNext = false;
                int target = _tracker;
                lock (_root)
                {
                    if (!this._isRetrieving && (target == _tracker))
                    {
                        this._lastAttempt = DateTime.UtcNow;
                        this._isRetrieving = true;
                        _tracker++;
                        shouldGetNext = true;
                    }
                }
                if(shouldGetNext)
                {
                    new Thread(() => 
                    {
                        this.GetNextMethod(this).ContinueWith((task) => {
                            lock (_root)
                            {
                                this._isRetrieving = false;
                            }
                        });
                    }).Start();
                }
            });
        }
    }
}

