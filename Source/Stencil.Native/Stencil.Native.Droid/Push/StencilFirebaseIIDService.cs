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
using Firebase.Iid;
using Stencil.Native.Core;

namespace Stencil.Native.Droid.Push
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    public class StencilFirebaseIIDService : FirebaseInstanceIdService
    {
        public override void OnTokenRefresh()
        {
            var refreshedToken = FirebaseInstanceId.Instance.Token;
            SendRegistrationToServer(refreshedToken);
        }
        private void SendRegistrationToServer(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                Container.StencilApp.PersistPushNotificationToken(token);
            }
        }
    }
}