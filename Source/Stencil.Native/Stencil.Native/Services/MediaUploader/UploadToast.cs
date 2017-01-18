using System;
using Stencil.Native.Core;

namespace Stencil.Native.Services.MediaUploader
{
    public class UploadToast : BaseClass
    {
        public UploadToast()
            : base("UploadToast")
        {
        }
        public object NativeImagePreview { get; set; }
        public int Attempt { get; set; }
        public string Message { get; set; }
        public bool IsError { get; set; }
        public bool IsComplete { get; set; }
        public int CurrentIndex { get; set; }
        public bool IsProcessing { get; set; }

        // how long to show the toast, if supported
        public int TimeOutSeconds { get; set; }
        public bool Handled { get; set; }

        private int _percentComplete = 0;
        public int PercentComplete
        {
            get
            {
                return _percentComplete;
            }
            set
            {
                if (_percentComplete != value)
                {
                    _percentComplete = value;
                    this.RaisePropertyChanged("PercentComplete");
                }
            }
        }
    }
}

