using System;
using System.Collections.Generic;

namespace Stencil.Native.Services.MediaUploader
{
    public interface IMediaUploader
    {
        event EventHandler<ToastChangedEventArgs> ToastChanged;
        UploadToast CurrentToast { get; }
        UploadRequest CurrentToastRequest { get; }
        void EnqueueRequests(IEnumerable<UploadRequest> requests);
        bool DequeueRequest(UploadRequest request);
        void ForceStop();
        void StartIfNeeded();
        List<UploadRequest> GetRequests();

        #if DEBUG

        void DebugSetCurrentToast(UploadToast toast);

        #endif
    }
}

