using System;
using Stencil.Native.App;
using Foundation;
using UIKit;
using SDWebImage;
using Stencil.SDK.Models;
using Stencil.Native.Core;

namespace Stencil.Native.iOS.Core
{
    public class IOSViewPlatform : BaseClass, IViewPlatform
    {
        public IOSViewPlatform()
            : base("IOSViewPlatform")
        {
            NSString version = new NSString("CFBundleShortVersionString");
            if(NSBundle.MainBundle.InfoDictionary.ContainsKey(version))
            {
                this.VersionNumber = NSBundle.MainBundle.InfoDictionary[version].ToString();
            }
            else
            {
                this.VersionNumber = "0.0";
            }
        }
        public virtual string VersionNumber { get; set; }
        public bool CanRegisterForPush { get; set; }

        public virtual string ShortName
        {
            get { return "ios"; }
        }
        public void OnMemoryWarning()
        {
            base.ExecuteMethod("OnMemoryWarning", delegate()
            {
                SDWebImageManager.SharedManager.ImageCache.ClearMemory();
            });
        }
        public void NavigateToFirstScreen()
        {
            this.ExecuteMethodOnMainThread("NavigateToFirstScreen", delegate()
            {
                StencilAppDelegate.Current.LaunchLogin();
            });
        }
        public void OnLoggedOn()
        {
            base.ExecuteMethod("OnLoggedOn", delegate()
            {
            });
        }
        public void OnLoggedOff()
        {
            this.ExecuteMethodOnMainThread("OnLoggedOff", delegate()
            {
                UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
            });
        }
        public void OnAccountRefreshed()
        {
            this.ExecuteMethodOnMainThread("OnAccountRefreshed", delegate()
            {
                
            });
        }


        public void DisplayNotification(string title, string message)
        {
            this.ExecuteMethodOnMainThread("DisplayNotification", delegate()
            {
                new UIAlertView(title, message, null, Container.StencilApp.GetLocalizedText(I18NToken.General_OK, "OK")).Show();
            });
        }

        private bool _showingOutdated;

        #pragma warning disable 0414 // <reference keeper>
        private UIAlertView _recentAlertView;
        #pragma warning restore 0414 // </reference keeper>

        public void OnOutDated(string message)
        {
            this.ExecuteMethodOnMainThread("OnOutDated", delegate()
            {
                if(_showingOutdated) { return; }
                _showingOutdated = true;

                if(string.IsNullOrEmpty(message))
                {
                    message = Container.StencilApp.GetLocalizedText(I18NToken.VersionNotSupported, "This version of the app is no longer supported.");
                }

                var view = new UIAlertView(Container.StencilApp.GetLocalizedText(I18NToken.AppExpired, "App Expired"), message + "\n" + Container.StencilApp.GetLocalizedText(I18NToken.LatestVersionIOS, "Please download the latest version from the App Store."), null, Container.StencilApp.GetLocalizedText(I18NToken.General_NoThanks, "No Thanks"), Container.StencilApp.GetLocalizedText(I18NToken.General_OK, "OK"));
                view.Dismissed += AlertViewOutDated_Dismissed;
                view.Show();
                _recentAlertView = view;
            });
        }
        private void AlertViewOutDated_Dismissed (object sender, UIButtonEventArgs e)
        {
            this.ExecuteMethodOnMainThread("AlertViewOutDated_Dismissed", delegate()
            {
                _showingOutdated = false;
                UIAlertView alert = sender as UIAlertView;
                if(alert != null)
                {
                    alert.Dismissed -= AlertViewOutDated_Dismissed;
                }
                if(e.ButtonIndex == 1)
                {
                    UIApplication.SharedApplication.OpenUrl(new NSUrl(NativeAssumptions.IOS_APPSTORE_URL));
                }
            });
        }
        public void UpdatePushNotificationToken()
        {
            this.ExecuteMethodOnMainThread("RegisterForPushNotifications", delegate ()
            {

                UIUserNotificationSettings currentSettings = UIApplication.SharedApplication.CurrentUserNotificationSettings;
                if(currentSettings != null && currentSettings.Types != UIUserNotificationType.None)
                {
                    this.RegisterForPushNotifications(); // already permitted, just updating token.
                }
            });
        }
        public void RegisterForPushNotificationsWithPrePromptIfNeeded(string faction_name)
        {
            this.ExecuteMethodOnMainThread("RegisterForPushNotificationsWithPrePromptIfNeeded", delegate ()
            {
                bool isAppetize = NSUserDefaults.StandardUserDefaults.BoolForKey("isAppetize");
                if(isAppetize)
                {
                    return;
                }

                UIUserNotificationSettings currentSettings = UIApplication.SharedApplication.CurrentUserNotificationSettings;
                if(currentSettings == null || currentSettings.Types == UIUserNotificationType.None)
                {
                    //TODO:SHOULD:Permissions: Prompt user with a pre-warning before actually calling:
                    this.RegisterForPushNotifications();
                }

            });
        }
       
        public void RegisterForPushNotifications()
        {
            this.ExecuteMethodOnMainThread("RegisterForPushNotifications", delegate()
            {
                bool isAppetize = NSUserDefaults.StandardUserDefaults.BoolForKey("isAppetize");
                if(isAppetize)
                {
                    return;
                }

                UIUserNotificationType types = UIUserNotificationType.Sound | UIUserNotificationType.Badge | UIUserNotificationType.Alert;
                UIUserNotificationSettings settings = UIUserNotificationSettings.GetSettingsForTypes(types, null);
                UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
            });
        }


        public void UnRegisterForPushNotifications()
        {
            this.ExecuteMethodOnMainThread("RegisterForPushNotifications", delegate()
            {

                this.CanRegisterForPush = false;
                UIApplication.SharedApplication.UnregisterForRemoteNotifications();
            });
        }

        public void ShowToast(string message)
        {
            this.ExecuteMethodOnMainThread("ShowToast", delegate()
            {
                HUD.ShowToast(message, false);
            });
        }

        public void SetBadge(int badge)
        {
            this.ExecuteMethodOnMainThreadBegin("SetBadge", delegate()
            {
                UIApplication.SharedApplication.ApplicationIconBadgeNumber = badge;
            });
        }

        public void SyncBadge()
        {
            this.ExecuteMethodOnMainThread("SyncBadge", delegate()
            {
                
            });
        }
       
        public UIViewController GetRootViewController()
        {
            return base.ExecuteFunction("GetRootViewController", delegate() 
            {
                return StencilAppDelegate.Current.Window.RootViewController;
            });
        }
        public UINavigationController GetRootNavigationController()
        {
            return base.ExecuteFunction("GetRootNavigationController", delegate() 
            {
                UINavigationController navController = StencilAppDelegate.Current.Window.RootViewController as UINavigationController;
                if(navController == null)
                {
                    navController = StencilAppDelegate.Current.Window.RootViewController.NavigationController;
                }
                return navController;
            });
        }

        public void ExecuteMethodOnMainThread(string name, Action method)
        {
            if(NSThread.IsMain)
            {
                this.ExecuteMethod(name, method);
            }
            else
            {
                UIApplication.SharedApplication.InvokeOnMainThread(new Action(delegate ()
                {
                    base.ExecuteMethod(name, method);
                }));
            }
        }
        public void ExecuteMethodOnMainThreadBegin(string name, Action method)
        {
            UIApplication.SharedApplication.BeginInvokeOnMainThread(delegate ()
            {
                base.ExecuteMethod(name, method);
            });
        }

        /// <summary>
        /// Not always assigned, only when controller provides public entrypoint
        /// </summary>
        protected BaseUIViewController RecentController { get; set; }
        public void RecentClearIfMatch(BaseUIViewController controller)
        {
            CoreUtility.ExecuteMethod("RecentClearIfMatch", delegate ()
            {
                if(RecentController == controller)
                {
                    RecentController = null;
                }
            });
        }
        public void RecentSet(BaseUIViewController controller)
        {
            CoreUtility.ExecuteMethod("RecentClearIfMatch", delegate ()
            {
                RecentController = controller;
            });
        }
        public BaseUIViewController RecentGet()
        {
            return RecentController;
        }
    }
}

