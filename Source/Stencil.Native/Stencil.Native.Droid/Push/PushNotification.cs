using System;

namespace Stencil.Native.Droid.Push
{
    public class PushNotification
    {
        public DateTime LocalTimeUTC { get; set; }

        public string Type { get; set; }
        public string TypeArgument { get; set; }
        public string LocaleKey { get; set; }
        public string[] LocaleArgs { get; set; }
        public string ExtraData { get; set; }
        public string Alert { get; set; }

        public bool HasViewed { get; set; }

        public string UIMessage { get; set; }
    }
}

