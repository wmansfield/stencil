using System;
using Android.Content;
using Android.OS;
using Stencil.Native.Services.MediaUploader;

namespace Stencil.Native.Droid.Core.Services
{
    public class UploadServiceConnection : Java.Lang.Object, IServiceConnection
    {
        public UploadServiceConnection(UploadServiceBinder binder)
        {
            if (binder != null)
            {
                _binder = binder;
            }
        }

        private UploadServiceBinder _binder;

        public bool IsBound
        {
            get
            {
                return _binder != null && _binder.IsBinderAlive && _binder.IsBound;
            }
        }
        public IMediaUploader MediaUploader
        {
            get
            {
                return _binder.UploadService;
            }
        }
        public event EventHandler<UploadServiceConnectedEventArgs> ServiceConnected;

        private void OnServiceConnected(IBinder binder)
        {
            var handler = this.ServiceConnected;
            if (handler != null)
            {
                handler(this, new UploadServiceConnectedEventArgs() { Binder = binder });
            }
        }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            UploadServiceBinder serviceBinder = service as UploadServiceBinder;

            if (serviceBinder != null)
            {
                _binder = serviceBinder;
                _binder.IsBound = true;

                // raise the service bound event
                OnServiceConnected(_binder);

                // begin updating the location in the Service
                serviceBinder.UploadService.Initialize();
            }
        }

        public void OnServiceDisconnected(ComponentName name) 
        {
            _binder.IsBound = false; 
        }
    }
}

