using System;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using Stencil.Native.Core;

namespace Stencil.Native.Services.MediaUploader
{
    public abstract class BaseMediaUploader : BaseClass, IMediaUploader
    {
        #region Constructor

        public BaseMediaUploader(string trackPrefix)
            : base(trackPrefix)
        {
            this.Requests = new List<UploadRequest>();
            this.FailedRequests = new List<UploadRequest>();
            this.UploadingFormat = "Uploading {0} ({1} of {2})";
            this.UploadingSingleFormat = "Uploading {0}";
            this.RetryingFormat = "Retrying {0} ({1} of {2})";
            this.UploadErrorCaption = "Error while uploading.";
            this.UploadCompleteCaption = "Upload Complete";
            this.PhotoCaption = "Photo";
            this.VideoCaption = "Video";
            this.UploadCancelledCaption = "Upload Cancelled";
            this.MaxUploadAttempts = 3;
        }

        #endregion


        #region Events & Invokers

        public event EventHandler<ToastChangedEventArgs> ToastChanged;
        protected virtual void OnToastChanged(UploadToast toast)
        {
            base.ExecuteMethod("OnToastChanged", delegate()
            {
                var evnt = this.ToastChanged;
                if(evnt != null)
                {
                    evnt(this, new ToastChangedEventArgs(toast));
                }
            });
        }

        #endregion

        #region Protected Properties

        protected static object _RequestSyncRoot = new object();

        protected virtual Task UploadTask { get; set; }
        protected virtual CancellationTokenSource UploadTaskCancellationToken { get; set; }

        protected virtual int CurrentTotal { get; set; }

        protected virtual List<UploadRequest> Requests { get; set; }
        protected virtual List<UploadRequest> FailedRequests { get; set; }


        #endregion

        #region Public Properties

        public virtual int MaxUploadAttempts { get; set; }
        public virtual string UploadingFormat { get; set; }
        public virtual string UploadingSingleFormat { get; set; }
        public virtual string RetryingFormat { get; set; }
        public virtual string UploadCompleteCaption { get; set; }
        public virtual string UploadErrorCaption { get; set; }
        public virtual string UploadCancelledCaption { get; set; }
        public virtual string PhotoCaption { get; set; }
        public virtual string VideoCaption { get; set; }

        private UploadToast _currentUploadToast;
        public virtual UploadToast CurrentToast
        {
            get
            {
                UploadToast result = null;
                lock (_RequestSyncRoot)
                {
                    result = _currentUploadToast;
                }
                return result;
            }
            protected set
            {
                lock (_RequestSyncRoot)
                {
                    _currentUploadToast = value;
                }
                this.OnToastChanged(_currentUploadToast);
                if (value != null && value.TimeOutSeconds > 0)
                {
                    Task.Run(async delegate()
                    {
                        await Task.Delay(value.TimeOutSeconds * 1000);
                        bool changed = false;
                        lock (_RequestSyncRoot)
                        {
                            if (_currentUploadToast == value)
                            {
                                _currentUploadToast = null;
                                changed = true;
                            }
                        }
                        if(changed)
                        {
                            this.OnToastChanged(_currentUploadToast);
                        }
                    });
                }
            }
        }
        public virtual UploadRequest CurrentToastRequest  { get; protected set; }



        #endregion

        #region Public Methods

        #if DEBUG

        public void DebugSetCurrentToast(UploadToast toast)
        {
            this.CurrentToast = toast;
        }

        #endif

        public void ForceStop()
        {
            base.ExecuteMethod("ForceStop", delegate()
            {
                if (this.UploadTask != null && this.UploadTaskCancellationToken != null)
                {
                    this.UploadTaskCancellationToken.Cancel();
                }
                UploadToast finishedToast = new UploadToast()
                {
                    TimeOutSeconds = 2,
                    IsProcessing = false,
                    IsComplete = true,
                    Message = this.UploadCancelledCaption
                };

                this.CurrentToast = finishedToast;
                lock (_RequestSyncRoot)
                {
                    this.Requests.Clear();
                }
            });
        }
        public virtual void StartIfNeeded()
        {
            base.ExecuteMethod("StartIfNeeded", delegate()
            {
                Task taskToStart = null;
                lock (_RequestSyncRoot)
                {
                    if (this.UploadTask == null || this.UploadTask.IsCompleted)
                    {
                        this.UploadTaskCancellationToken = new CancellationTokenSource();
                        taskToStart = new Task(DoUploads, UploadTaskCancellationToken.Token);
                        this.UploadTask = taskToStart;
                    }
                }
                if (taskToStart != null)
                {
                    taskToStart.Start(TaskScheduler.Default);
                }
            });
        }
        public virtual void EnqueueRequests(IEnumerable<UploadRequest> requests)
        {
            base.ExecuteMethod("EnqueueRequest", delegate()
            {
                foreach (var item in requests)
                {
                    EnqueueRequest(item);
                }
            });
        }
        public virtual void EnqueueRequest(UploadRequest request)
        {
            base.ExecuteMethod("EnqueueRequest", delegate()
            {
                Task taskToStart = null;
                UploadToast toastToNotify = null;
                lock (_RequestSyncRoot)
                {
                    this.FailedRequests.Remove(request); //jic
                    this.Requests.Add(request);
                    if (this.UploadTask == null || this.UploadTask.IsCompleted)
                    {
                        this.UploadTaskCancellationToken = new CancellationTokenSource();
                        taskToStart = new Task(DoUploads, UploadTaskCancellationToken.Token);
                        this.UploadTask = taskToStart;
                    }
                    else
                    {
                        this.CurrentTotal++;
                        toastToNotify = this.CurrentToast;
                    }
                }
                if (toastToNotify != null && toastToNotify.IsProcessing)
                {
                    string format = this.UploadingFormat;
                    if(this.CurrentTotal == 1)
                    {
                        format = this.UploadingSingleFormat;
                    }
                    if (toastToNotify.Attempt > 1)
                    {
                        format = this.RetryingFormat;
                    }
                    if (request.IsPhoto)
                    {
                        toastToNotify.Message = string.Format(format, this.PhotoCaption, toastToNotify.CurrentIndex, this.CurrentTotal);
                    }
                    else
                    {
                        toastToNotify.Message = string.Format(format, this.VideoCaption, toastToNotify.CurrentIndex, this.CurrentTotal);
                    }
                    this.CurrentToast = toastToNotify;
                }
                if (taskToStart != null)
                {
                    taskToStart.Start(TaskScheduler.Default);
                }
            });
        }
        public virtual bool DequeueRequest(UploadRequest request)
        {
            return base.ExecuteFunction("DequeueRequest", delegate()
            {
                bool result = false;
                lock (_RequestSyncRoot)
                {
                    if(this.Requests.Remove(request))
                    {
                        result = true;
                    }
                    if(this.FailedRequests.Remove(request))
                    {
                        result = true;
                    }
                }
                return result;
            });
        }

        public List<UploadRequest> GetRequests()
        {
            return base.ExecuteFunction("GetRequests", delegate()
            {
                List<UploadRequest> result = new List<UploadRequest>();
                UploadRequest request = this.CurrentToastRequest;
                if(request != null)
                {
                    result.Add(request);
                }
                if(this.Requests.Count > 0 || this.FailedRequests.Count > 0) // allow for a lil leeway for ui locking performance
                {
                    lock (_RequestSyncRoot)
                    {
                        result.AddRange(this.Requests);
                        result.AddRange(this.FailedRequests);
                    }
                }
                return result;
            });
        }

        #endregion

        #region Protected Methods

        protected virtual void DoUploads()
        {
            try
            {
                base.ExecuteMethod("DoUploads", delegate()
                {
                    // Get First
                    UploadRequest nextRequest = null;
                    CurrentTotal = 0;
                    int current = 0;
                    lock (_RequestSyncRoot)
                    {
                        if (this.Requests.Count > 0)
                        {
                            CurrentTotal = this.Requests.Count;
                            nextRequest = this.Requests[0];

                            this.Requests.RemoveAt(0);
                            current++;
                        }
                    }
                    if (nextRequest == null)
                    {
                        return;
                    }
                    try
                    {
                        this.OnStarted();

                        bool hadError = false;
                        string latestCachePrefix = "";

                        UploadToast lastToast = null;

                        // Iterate
                        while (nextRequest != null)
                        {
                            UploadRequest request = nextRequest;
                            nextRequest = null; // while breaker

                            this.CurrentToastRequest = request;


                            latestCachePrefix = request.DataCachePrefix;

                            // Notify About Process
                            UploadToast uploadToast = new UploadToast()
                            {
                                TimeOutSeconds = 0,
                                CurrentIndex = current,
                                IsProcessing = true,
                                NativeImagePreview = request.NativeImagePreview
                            };
                            request.RelatedToast = uploadToast;
                            string format = this.UploadingFormat;
                            if(this.CurrentTotal == 1)
                            {
                                format = this.UploadingSingleFormat;
                            }
                            if (uploadToast.Attempt > 1)
                            {
                                format = RetryingFormat;
                            }
                            if (request.IsPhoto)
                            {
                                uploadToast.Message = string.Format(format, this.PhotoCaption, current, CurrentTotal);
                            }
                            else
                            {
                                uploadToast.Message = string.Format(format, this.VideoCaption, current, CurrentTotal);
                            }

                            this.CurrentToast = uploadToast;
                            lastToast = uploadToast;

                            // Process
                            Task<bool> uploadTask = this.DoUploadItem(request, uploadToast);
                            Task.WaitAll(new Task[] { uploadTask });
                            bool success = uploadTask.Result;
                            if (!success)
                            {
                                hadError = true;
                            }


                            // Get Next
                            lock (_RequestSyncRoot)
                            {
                                if (!success)
                                {
                                    this.FailedRequests.Add(request);
                                }
                                if (this.Requests.Count > 0)
                                {
                                    nextRequest = this.Requests[0];
                                    this.Requests.RemoveAt(0);
                                    current++;
                                }
                                else
                                {
                                    this.UploadTask = null; // weird, we clear our own reference so that new ones can be spawned
                                }
                            }
                        }
                        this.CurrentToastRequest = null;

                        // Notify complete
                        UploadToast finishedToast = lastToast;
                        if(finishedToast == null)
                        {
                            finishedToast = new UploadToast();
                        }
                        finishedToast.IsProcessing = false;
                        finishedToast.Message = this.UploadCompleteCaption;

                        if (hadError)
                        {
                            finishedToast.IsError = true;
                            finishedToast.Message = this.UploadErrorCaption;
                        }
                        else
                        {
                            finishedToast.IsComplete = true;
                            finishedToast.TimeOutSeconds = 4;
                        }

                        this.CurrentToast = finishedToast;

                        if (!string.IsNullOrEmpty(latestCachePrefix))
                        {
                            this.InvalidateCacheForPrefix(latestCachePrefix);
                        }
                    }
                    finally
                    {
                        this.OnStopped();
                    }
                });

            }
            catch (OperationCanceledException)
            {
                //gulp
            }
        }

        protected virtual Task<bool> DoUploadItem(UploadRequest request, UploadToast uploadToast)
        {
            return base.ExecuteFunctionAsync("DoUploadItem", async delegate()
            {
                bool success = false;
                request.UploadAttempt = 1;
                while (!success && !request.CancelRequested && (request.UploadAttempt < this.MaxUploadAttempts))
                {
                    request.UploadAttempt++;
                    if (uploadToast != null)
                    {
                        uploadToast.Attempt = request.UploadAttempt;
                    }

                    request.InvokeProcessingActions();


                    if (request.IsPhoto)
                    {
                        success = await UploadPhotoItem(request);
                    }
                    else
                    {
                        success = await UploadVideoItem(request);
                    }
                }
                request.Succeeded = success;
                request.InvokeProcessingActions();

                if(request.StrongCallbackAction != null)
                {
                    await request.StrongCallbackAction(request);
                }
                if(request.WeakCallbackAction != null)
                {
                    Func<UploadRequest, Task> action = null;
                    request.WeakCallbackAction.TryGetTarget(out action);
                    if(action != null)
                    {
                        await action(request);
                    }
                }
                request.StrongCallbackAction = null;
                request.WeakCallbackAction = null;
                return success;
            });
        }

        protected virtual Task<bool> UploadVideoItem(UploadRequest request)
        {
            return base.ExecuteFunctionAsync("UploadVideoItem", async delegate()
            {
                try
                {
                    AmazonUploadInfo info = await PerformUploadVideoToAmazon(request);
                    if (info != null && info.Success)
                    {
                        if(request.CancelRequested)
                        {
                            return false;
                        }
                        if(request.IsPreUpload)
                        {
                            return true;
                        }
                        return await PerformAddUploadedVideoToApp(request, info);
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    base.LogError(ex, "UploadVideoItem");
                    return false;
                }
            });
        }
        protected virtual Task<bool> UploadPhotoItem(UploadRequest request)
        {
            return base.ExecuteFunctionAsync("UploadPhotoItem", async delegate()
            {        
                try
                {
                    AmazonUploadInfo info = await PerformUploadPhotoToAmazon(request);
                    if (info != null && info.Success)
                    {
                        if(request.CancelRequested)
                        {
                            return false;
                        }
                        if(request.IsPreUpload)
                        {
                            return true;
                        }
                        return await PerformAddUploadedPhotoToApp(request, info);
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    base.LogError(ex, "UploadPhotoItem");
                    return false;
                }
            });
        }


        /// <summary>
        /// Extension point
        /// </summary>
        protected virtual void OnStarted()
        {
        }

        /// <summary>
        /// Extension point
        /// </summary>
        protected virtual void OnStopped()
        {
        }

        #endregion

        #region Abstract Methods

        protected abstract Task<AmazonUploadInfo> PerformUploadVideoToAmazon(UploadRequest request);
        protected abstract Task<AmazonUploadInfo> PerformUploadPhotoToAmazon(UploadRequest request);
        protected abstract Task<bool> PerformAddUploadedVideoToApp(UploadRequest request, AmazonUploadInfo info);
        protected abstract Task<bool> PerformAddUploadedPhotoToApp(UploadRequest request, AmazonUploadInfo info);
        protected abstract void InvalidateCacheForPrefix(string cachePrefix);

        #endregion
    }
}

