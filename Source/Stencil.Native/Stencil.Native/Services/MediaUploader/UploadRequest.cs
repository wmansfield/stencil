using System;
using System.Linq;
using System.Collections.Generic;
using Stencil.Native.Core;
using System.Threading.Tasks;


namespace Stencil.Native.Services.MediaUploader
{
    public class UploadRequest
    {
        public UploadRequest()
        {
            this.OnProcessingActions = new HashSet<Action<UploadRequest>>();
        }
        private static object _ProcessingLock = new object();

        public bool IsPreUpload { get; set; }
        public AmazonUploadInfo UploadInfo { get; set; }
        public virtual string ReferenceID { get; set; }
        public virtual string GroupID { get; set; }
        public virtual bool IsPhoto { get; set; }
        public virtual object NativeData { get; set; }
        public virtual string NativePath { get; set; }
        public virtual object NativeImagePreview { get; set; }
        public virtual Guid UploadID { get; set; }
        public virtual string FileName { get; set; }
        public virtual string MimeType { get; set; }
        public virtual WeakReference<Func<UploadRequest, Task>> WeakCallbackAction  { get; set; }
        public virtual Func<UploadRequest, Task> StrongCallbackAction  { get; set; }
        public virtual object CallbackArgument { get; set; }
        public virtual bool? Succeeded { get; set; }
        /// <summary>
        /// fires when toast is assigned or on toast property changed
        /// </summary>
        protected virtual HashSet<Action<UploadRequest>> OnProcessingActions { get; set; }
        public virtual bool CancelRequested { get; set; }

        public object PlatformParameters { get; set; }

        /// <summary>
        /// The prefix to invalidate after an upload completes
        /// </summary>
        public virtual string DataCachePrefix { get; set; }

        internal virtual int UploadAttempt { get; set; }

        /// <summary>
        /// Data used by the uploader
        /// </summary>
        public virtual object AuthData { get; set; }

        public void AddOnProcessingAction(Action<UploadRequest> action)
        {
            lock (_ProcessingLock)
            {
                this.OnProcessingActions.Add(action);
            }
        }
        public void RemoveOnProcessingAction(Action<UploadRequest> action)
        {
            lock (_ProcessingLock)
            {
                this.OnProcessingActions.Remove(action);
            }
        }
        public void InvokeProcessingActions()
        {
            if (OnProcessingActions != null)
            {
                Action<UploadRequest>[] actions = null;
                lock (_ProcessingLock)
                {
                    actions = this.OnProcessingActions.ToArray();
                }
                foreach (var action in actions) 
                {
                    action(this);
                }
            }
        }

        private UploadToast _uploadToast;
        public virtual UploadToast RelatedToast
        {
            get
            {
                return _uploadToast;
            }
            set
            {
                _uploadToast = value;
                this.InvokeProcessingActions();
            }
        }

        public void OnUploadProgressChanged(object sender, UploadProgressArgs args)
        {
            try
            {
                if(this.RelatedToast != null)
                {
                    this.RelatedToast.PercentComplete = args.PercentDone;
                }

                this.InvokeProcessingActions();
            }
            catch (Exception ex)
            {
                Container.Track.LogError(ex, "OnUploadProgressChanged");
            }
        }
    }
}

