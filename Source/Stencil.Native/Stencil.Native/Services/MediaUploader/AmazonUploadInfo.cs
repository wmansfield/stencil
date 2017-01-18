using System;

namespace Stencil.Native.Services.MediaUploader
{
    public class AmazonUploadInfo
    {
        public bool Success { get; set; }

        public string AmazonKey { get; set; }
        public string AmazonKeyThumb { get; set; }

        public string AmazonPath { get; set; }
    }
}

