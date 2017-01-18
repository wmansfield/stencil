using System;
using Android.OS;

namespace Stencil.Native.Droid.Core.Services
{
    public class UploadServiceBinder : Binder
    {
        public UploadServiceBinder(UploadService service) 
        { 
            this.UploadService = service; 
        }

        public UploadService UploadService { get; protected set; }

        public bool IsBound { get; set; }
    }
}

