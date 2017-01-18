using System;
using Android.App;
using Android.OS;
using Android.Content;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Stencil.Native.Services.MediaUploader;

namespace Stencil.Native.Droid.Core.Services
{
    [Service]
    public class UploadService : BaseService, IMediaUploader
    {
        public UploadService()
            : base("UploadService")
        {

        }

        private bool _initialized;
        private IBinder _binder;
        private List<Thread> _threads = new List<Thread>();

       
        public event EventHandler<ToastChangedEventArgs> ToastChanged;
        protected void OnToastChanged(object sender, ToastChangedEventArgs args)
        {
            var changed = this.ToastChanged;
            if (changed != null)
            {
                Thread t = null;
                t = new Thread (() => 
                {
                    changed(sender, args);
                    _threads.Remove(t);// not safe, but good enough
                });
                t.Name = "OnToastChanged";
                _threads.Add(t);

                t.Start();
            }
        }

        public UploadRequest CurrentToastRequest
        {
            get
            {
                if(this.DroidMediaUploader != null)
                {
                    return this.DroidMediaUploader.CurrentToastRequest;
                }
                return null;
            }
        }
        public bool DequeueRequest(UploadRequest request)
        {
            if(this.DroidMediaUploader != null)
            {
                return this.DroidMediaUploader.DequeueRequest(request);
            }
            return false;
        }

        public List<UploadRequest> GetRequests()
        {
            if(this.DroidMediaUploader != null)
            {
                return this.DroidMediaUploader.GetRequests();
            }
            return new List<UploadRequest>();;
        }

        public void DebugSetCurrentToast(UploadToast toast)
        {
            #if DEBUG
            if(this.DroidMediaUploader != null)
            {
                this.DroidMediaUploader.DebugSetCurrentToast(toast);
            }
            #endif
        }
        public UploadToast CurrentToast
        {
            get 
            { 
                if(this.DroidMediaUploader != null)
                {
                    return this.DroidMediaUploader.CurrentToast;
                }
                return null;
            }
        }
        public DroidMediaUploader DroidMediaUploader { get; set; }


        /// <summary>
        /// Called by OS
        /// </summary>
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            return StartCommandResult.Sticky;
        }
        public override IBinder OnBind(Intent intent)
        {
            _binder = new UploadServiceBinder(this);
            return _binder;
        }

        /// <summary>
        /// Called when we have an open connection to the service, we start our work.
        /// </summary>
        public void Initialize()
        {

        }

        #region IMediaUploader Methods


        protected void EnsureInitialized()
        {
            base.ExecuteMethod("EnsureInitialized", delegate()
            {
                if(!_initialized)
                {
                    this.DroidMediaUploader = new DroidMediaUploader();
                    this.DroidMediaUploader.ToastChanged += OnToastChanged;
                    _initialized = true;
                    this.DroidMediaUploader.StartIfNeeded();
                }
            });
        }

        public void EnqueueRequests(IEnumerable<UploadRequest> requests)
        {
            base.ExecuteMethod("EnqueueRequests", delegate()
            {
                Thread t = null;
                t = new Thread (() => 
                {
                    this.EnsureInitialized();
                    this.DroidMediaUploader.EnqueueRequests(requests);
                    _threads.Remove(t);// not safe, but good enough
                });
                t.Name = "EnqueueRequests";
                _threads.Add(t);

                t.Start();
            });
        }
        public void ForceStop()
        {
            base.ExecuteMethod("ForceStop", delegate()
            {
                this.EnsureInitialized();
                this.DroidMediaUploader.ForceStop();
            });
        }
        public void StartIfNeeded()
        {
            base.ExecuteMethod("StartIfNeeded", delegate()
            {
                Thread t = null;
                t = new Thread (() => 
                {
                    this.EnsureInitialized();
                    this.DroidMediaUploader.StartIfNeeded();
                    _threads.Remove(t);// not safe, but good enough
                });
                t.Name = "StartIfNeeded";
                _threads.Add(t);

                t.Start();
            });
        }

        #endregion


    }
}

