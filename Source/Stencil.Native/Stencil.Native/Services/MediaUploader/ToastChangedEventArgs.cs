using System;

namespace Stencil.Native.Services.MediaUploader
{
    public class ToastChangedEventArgs : EventArgs
    {
        public ToastChangedEventArgs(UploadToast toast)
        {
            Toast = toast;
        }

        public UploadToast Toast { get; set; }
    }
}

