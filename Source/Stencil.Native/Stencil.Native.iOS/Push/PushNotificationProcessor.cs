using Foundation;
using Stencil.Native.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;

namespace Stencil.Native.iOS
{
    public class PushNotificationProcessor : BaseClass
    {
        public PushNotificationProcessor(StencilAppDelegate appDelegate)
            : base("PushNotificationProcessor")
        {
            this.AppDelegate = appDelegate;
        }

        protected StencilAppDelegate AppDelegate { get; set; }
        private NSString nsAps = new NSString("aps");
        private NSString nsBadge = new NSString("badge");
        private NSString nsAlert = new NSString("alert");
        private NSString nsArgs = new NSString("loc-args");
        private NSString nsExtraType = new NSString("x");
        private NSString nsSample = new NSString("snl_sample");


        public void ProcessRemotePushNotification(NSDictionary data, bool fromBackground, Action<UIBackgroundFetchResult> completionHandler)
        {
            base.ExecuteMethod("ProcessRemotePushNotification", delegate ()
            {
                if (data == null)
                {
                    if (completionHandler != null)
                    {
                        completionHandler(UIBackgroundFetchResult.NoData);
                    }
                    return; //---------- Short Circuit
                }

                // Process each notification type
                this.ProcessNotifications(data, fromBackground);


                // notify complete
                if (completionHandler != null)
                {
                    completionHandler(UIBackgroundFetchResult.NoData);
                }
            });
        }

        protected void ProcessNotifications(NSDictionary data, bool fromBackground)
        {
            base.ExecuteMethod("ProcessNotifications", delegate ()
            {
                if (data.ContainsKey(nsAps))
                {
                    NSDictionary apsDictionary = data[nsAps] as NSDictionary;

                    // process badge
                    this.ProcessNotification_Badge(fromBackground, apsDictionary);

                    // Process Others
                    NSDictionary alertDictionary = apsDictionary[nsAlert] as NSDictionary;

                    if (data.ContainsKey(nsSample))
                    {
                        this.ProcessNotification_Sample(fromBackground, data, apsDictionary, alertDictionary);
                    }
                    else
                    {
                        // try generic alert
                        string alertString = string.Empty;
                        NSString alertNSString = apsDictionary[nsAlert] as NSString;
                        if (alertNSString != null)
                        {
                            alertString = alertNSString.ToString();
                        }
                        if (!string.IsNullOrEmpty(alertString))
                        {
                            ProcessNotification_Generic(fromBackground, data, alertString);
                        }
                    }
                }
            });
        }

        protected void ProcessNotification_Badge(bool fromBackground, NSDictionary rawData)
        {
            base.ExecuteMethod("ProcessNotification_Badge", delegate ()
            {
                if (!rawData.ContainsKey(nsBadge)) { return; }

                NSNumber badgeCount = rawData[nsBadge] as NSNumber;

                Container.ViewPlatform.SetBadge((int)badgeCount.NIntValue);

            });
        }
        protected void ProcessNotification_Generic(bool fromBackground, NSDictionary rawData, string alertString)
        {
            base.ExecuteMethod("ProcessNotification_Generic", delegate ()
            {
                new UIAlertView(Container.StencilApp.GetLocalizedText(I18NToken.Notification, "Notification"), alertString, null, Container.StencilApp.GetLocalizedText(I18NToken.General_OK, "OK")).Show();
            });
        }
        protected void ProcessNotification_Sample(bool fromBackground, NSDictionary rawData, NSDictionary apsDictionary, NSDictionary alertDictionary)
        {
            base.ExecuteMethod("ProcessNotification_Sample", delegate ()
            {
                string route_id = (rawData[nsSample] as NSString).ToString();
                string extra_parameter = (rawData[nsExtraType] as NSString).ToString();

                if (fromBackground)
                {
                    // maybe jump to the proper page?
                }
            });
        }
       
    }
}
