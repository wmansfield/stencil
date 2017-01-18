using System;
using Stencil.Native.Services.MediaUploader;

namespace Stencil.Native.Droid.Core.Services
{
    public class MediaUploaderWrapper
    {
        public MediaUploaderWrapper(UploadServiceConnection connection)
        {
            this.Connection = connection;
        }
        public UploadServiceConnection Connection { get; set; }
        public bool IsAvailable
        {
            get { return this.Connection != null && this.Connection.IsBound; }
        }

        public IMediaUploader Instance
        {
            get 
            {  
                if(this.IsAvailable)
                {
                    return Connection.MediaUploader;
                }
                return null;
            }
        }
    }
}

