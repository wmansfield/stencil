using System;
using System.Collections.Generic;
using System.Text;

namespace Stencil.Native
{
    public partial class NativeAssumptions
    {
        //TODO:MUST: Replace all identifiers (ios and android) to be not "stencil"
        //TODO:SHOULD:Stencil: find/replace socialhaven and replace with proper urls/emails
        //TODO:COULD: Find/replace for "Stencil" and "stencil" and replace with your own phrase [outside of VS recommended -- AND BE CAREFUL]

        #if DEBUG
        //public static string BASE_API_URL = "http://192.168.1.124:4328/api/";
        #endif

        public static string BASE_API_URL = "https://stencil-demo.azurewebsites.net/api/";

        public static string INTERNAL_APP_NAME = "stencil";
        public static string EXTERNAL_APP_NAME = "Stencil";

        public const string ANDROID_APP_PACKAGE = "com.socialhaven.stencil";

        public static string IOS_APPSTORE_URL = "itms-apps://itunes.apple.com/app/id_______"; //TODO:MUST: IOS App Store Url for app store





        public static string ALERT_SAMPLE = "{0} said: {1}";


#if __IOS__
        public static string FONT_AWESOME = "fontawesome";
#endif

#if __ANDROID__
        public static string FONT_AWESOME = "fonts/fontawesome-webfont.ttf";

#endif



    }
}
