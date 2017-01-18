using System;
using Android.App;
using Android.Content;
using Newtonsoft.Json;
using Android.OS;
using Android.Support.V4.Content;
using Stencil.Native.Core;
using Stencil.Native.Droid.Core;
using Stencil.Native.Droid.Activities;
using v7 = Android.Support.V7.App;

namespace Stencil.Native.Droid.Push
{
    public class PushNotificationProcessor
    {
        public static PushNotification ExtractPushNotification(Intent messageIntent)
        {
            return CoreUtility.ExecuteFunction("ExtractPushNotification", delegate()
            {
                if(messageIntent == null)
                {
                    return null;
                }
                return ExtractPushNotification(messageIntent.Extras);
            });
        }
        public static PushNotification ExtractPushNotification(Bundle extras)
        {
            return CoreUtility.ExecuteFunction("ExtractPushNotification", delegate()
            {
                if(extras == null)
                {
                    return null;
                }
                PushNotification push = new PushNotification();
                push.LocalTimeUTC = DateTime.UtcNow;
                push.Alert = extras.GetString("alert");
                push.Type = extras.GetString("type");
                push.TypeArgument = extras.GetString("type-arg");
                push.LocaleKey = extras.GetString("loc-key");
                push.ExtraData = extras.GetString("x");
                string localeArgs = extras.GetString("loc-args");
                if (!string.IsNullOrEmpty(localeArgs))
                {
                    push.LocaleArgs = JsonConvert.DeserializeObject<string[]>(localeArgs);
                }

                if(string.IsNullOrEmpty(push.Alert) && string.IsNullOrEmpty(push.Type))
                {
                    return null;
                }
                return push;
            });
        }
        public static v7.NotificationCompat.Builder GenerateNotification(Context context, PushNotification push, Bundle messageIntent)
        {
            return CoreUtility.ExecuteFunction("GenerateNotification", delegate()
            {
                if (string.IsNullOrEmpty(push.Type)) 
                { 
                    return null; 
                }

                Type activityType = typeof(SplashScreenActivity);

                Intent intent = new Intent(context, activityType);
                intent.PutExtra(BaseActivity.INIT_BUNDLE_KEY, messageIntent);
                PendingIntent pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.OneShot);

                string title = context.Resources.GetString(Resource.String.app_name);
                string text = push.Alert;

                string[] args = push.LocaleArgs;
                int count = 0;
                if(args != null && args.Length > 0)
                {
                    int.TryParse(args[0], out count);
                }
                Guid? route_id = null;
                if(!string.IsNullOrEmpty(push.TypeArgument))
                {
                    Guid parsed = Guid.Empty;
                    if(Guid.TryParse(push.TypeArgument, out parsed))
                    {
                        route_id = parsed;
                    }
                }
                switch (push.Type) 
                {
                    case "snl_sample":
                        text = string.Format(Container.StencilApp.GetLocalizedText(I18NToken.ALERT_SAMPLE, NativeAssumptions.ALERT_SAMPLE), args[0], args[1]);
                        break;
                    default:
                        break;
                }
                if(string.IsNullOrEmpty(text))
                {
                    return null;
                }
                v7.NotificationCompat.Builder builder = new v7.NotificationCompat.Builder(context);
                builder
                    .SetSmallIcon(Resource.Drawable.Icon)
                    .SetContentTitle(title)
                    .SetContentText(text)
                    .SetStyle(new v7.NotificationCompat.BigTextStyle().BigText(text))
                    .SetAutoCancel(true)
                    .SetContentIntent(pendingIntent)
                    .SetVisibility(v7.NotificationCompat.VisibilityPublic)
                    .SetDefaults(v7.NotificationCompat.DefaultLights | v7.NotificationCompat.DefaultSound);

                return builder;

            });
        }


        //TODO:SHOULD:Push: Add the following to any BaseActivity that will want to process a push based invocation
        /*
        override OnCreate(...){
            ...
            new Handler().PostDelayed(delegate() 
            {
                PushNotificationProcessor.ProcessPushNotification(this, this.GetInitialBundle());
            }, 100); 

        }
        */

        public static void ProcessPushNotification(BaseActivity activity, Bundle bundle)
        {
            CoreUtility.ExecuteMethod("ProcessPushNotification", delegate()
            {
                PushNotification notification = ExtractPushNotification(bundle);
                if(notification == null) { return; }

                Guid? route_id = null;
                if(!string.IsNullOrEmpty(notification.TypeArgument))
                {
                    Guid parsed = Guid.Empty;
                    if(Guid.TryParse(notification.TypeArgument, out parsed))
                    {
                        route_id = parsed;
                    }
                }

                string extraParameter = notification.ExtraData;

                if (route_id.HasValue)
                {
                    switch (notification.Type) 
                    {
                        case "snl_sample":
                            // maybe jump to the proper page?
                            break;
                        default:
                            break;
                    }
                }
            });
        }
    }
}

