using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Firebase.Messaging;
using Stencil.Native.Core;
using v7 = Android.Support.V7.App;

namespace Stencil.Native.Droid.Push
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class StencilFirebaseMessagingService : FirebaseMessagingService
    {
        public override void OnMessageReceived(RemoteMessage message)
        {
            CoreUtility.ExecuteMethod("OnMessageReceived", delegate ()
            {
                // Shape like previous version of GCM for ease
                Bundle messageIntent = new Bundle();
                if (message.Data != null)
                {
                    foreach (string key in message.Data.Keys)
                    {
                        messageIntent.PutString(key, message.Data[key]);
                    }
                }

                PushNotification notification = PushNotificationProcessor.ExtractPushNotification(messageIntent);
                if (notification != null)
                {
                    notification.Alert += "!";
                    v7.NotificationCompat.Builder builder = PushNotificationProcessor.GenerateNotification(this, notification, messageIntent);
                    if (builder != null)
                    {
                        int uniqueID = (int)DateTime.UtcNow.ToUnixSeconds(); // will work until 2038
                        NotificationManager notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);
                        notificationManager.Notify(uniqueID, builder.Build());
                    }
                }
            });
        }
    }
}