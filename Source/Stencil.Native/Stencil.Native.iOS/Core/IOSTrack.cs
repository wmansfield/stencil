using Stencil.Native.Core;
using System;

namespace Stencil.Native.iOS.Core
{
    public class IOSTrack : CoreTrack
    {
        public override void LogError(string message, string tag = "")
        {
            base.LogError(message, tag);
            //TODO:COULD: Report exception to some web service or analytics
        }
    }
}

