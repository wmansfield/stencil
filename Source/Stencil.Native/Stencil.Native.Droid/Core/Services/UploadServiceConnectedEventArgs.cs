using System;
using Android.OS;

namespace Stencil.Native.Droid.Core.Services
{
    public class UploadServiceConnectedEventArgs : EventArgs
    {
        public IBinder Binder { get; set; }
    }
}

