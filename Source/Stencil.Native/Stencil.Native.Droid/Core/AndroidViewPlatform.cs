using System;
using System.Linq;

using Android.App;
using Android.Content.PM;
using Android.Content;
using Android.Widget;
using Android.Net;
using Android.OS;
using Stencil.Native.Core;
using Stencil.Native.Droid.Core.UI;
using Stencil.Native.Droid.Activities;

namespace Stencil.Native.Droid.Core
{
    public class AndroidViewPlatform : BaseClass, IViewPlatform
    {
        public AndroidViewPlatform(Context context)
            : this("AndroidViewPlatform", context)
        {
        }

        public AndroidViewPlatform(string trackPrefix, Context context)
            : base(trackPrefix)
        {
            this.Context = context;

            PackageInfo info = context.PackageManager.GetPackageInfo(context.PackageName, PackageInfoFlags.MetaData);
            if (info != null)
            {
                this.VersionNumber = info.VersionName;
            }
            else
            {
                this.VersionNumber = "0.0";
            }
        }

        private bool _shouldShowOutdated;
        private string _outDateMessage;

        public virtual Context Context { get; set; }
        public virtual object RecentView { get; set; }
        public virtual IMultiViewHost RecentMultiViewHost { get; set; }

        public virtual string VersionNumber { get; set; }

        public virtual string ShortName
        {
            get { return "android"; }
        }
        public void NavigateToFirstScreen()
        {
            base.ExecuteMethod("NavigateToFirstScreen", delegate()
            {
                Intent intent = new Intent(this.Context, typeof(LoginActivity));
                intent.AddFlags(ActivityFlags.ClearTop);
                intent.AddFlags(ActivityFlags.NewTask);
                intent.AddFlags(ActivityFlags.ClearTask);
                this.Context.StartActivity(intent);
            });
        }
        public void OnMemoryWarning()
        {
            Container.ImageLoader.ClearDiskCache();
            Container.ImageLoader.ClearMemoryCache();
        }

        public virtual void OnLoggedOff()
        {
            base.ExecuteMethod("OnLoggedOff", delegate()
            {
                Container.ImageLoader.ClearDiskCache();
                Container.ImageLoader.ClearMemoryCache();
            });
        }

        public virtual void OnLoggedOn()
        {
            base.ExecuteMethod("OnLoggedOn", delegate ()
            {
                
            });
        }

        public void OnOutDated(string message)
        {
            base.ExecuteMethod("OnOutDated", delegate()
            {
                _shouldShowOutdated = true;
                _outDateMessage = message;
            });
        }

        public void ShowOutDatedMessageIfNeeded(Context context)
        {
            base.ExecuteMethod("ShowOutDatedMessageIfNeeded", delegate()
            {
                if(!_shouldShowOutdated) { return; }
                _shouldShowOutdated = false;

                if(string.IsNullOrEmpty(_outDateMessage))
                {
                    _outDateMessage = Container.StencilApp.GetLocalizedText(I18NToken.VersionNotSupported, "This version of the app is no longer supported.");
                }

                HUD.ShowErrorWithStatus(context, _outDateMessage + "\n" + Container.StencilApp.GetLocalizedText(I18NToken.LatestVersionAndroid, "Please download the latest version from the Play Store."), 6000);
            });
        }

        public void UnRegisterForPushNotifications()
        {
            
        }

        public void UpdatePushNotificationToken()
        {
            
        }

        public void RegisterForPushNotificationsWithPrePromptIfNeeded(string factionName)
        {
            
        }
        public void OnAccountRefreshed()
        {
            base.ExecuteMethod("OnAccountRefreshed", delegate()
            {
                
            });
        }
        public void SetBadge(int badge)
        {
            base.ExecuteMethod("OnAccountRefreshed", delegate ()
            {
                //TODO:COULD: Integrate with any launcher that supports badging
               
            });
        }
        public void DisplayNotification(string title, string message)
        {
            Activity currentActivity = RecentView as Activity;
            if(currentActivity == null)
            {
                return;
            }
            currentActivity.RunOnUiThread(delegate ()
            {
                HUD.ShowErrorWithStatus(currentActivity, message, 4000);
            });
        }

        public virtual void ShowToast(string message)
        {
            base.ExecuteMethod("ShowToast", delegate()
            {
                Activity currentActivity = RecentView as Activity;
                if (currentActivity == null)
                {
                    return;
                }
                currentActivity.RunOnUiThread(delegate()
                {
                    Toast toast = Toast.MakeText(currentActivity, message, ToastLength.Long);
                    toast.Show();
                });
            });
        }
        public void SyncBadge()
        {

        }


        public virtual bool VerifyInternetConnection()
        {
            return base.ExecuteFunction("VerifyInternetConnection", delegate()
            {
                try
                {
                    Activity currentActivity = RecentView as Activity;
                    if (currentActivity != null)
                    {
                        ConnectivityManager connectivityManager = (ConnectivityManager)currentActivity.GetSystemService(Context.ConnectivityService);

                        NetworkInfo activeNetwork = connectivityManager.ActiveNetworkInfo;
                        return (activeNetwork != null) && activeNetwork.IsConnectedOrConnecting;
                    }
                    return true; // assume
                }
                catch
                {
                    return true; // assume
                }
            });
        }



        public string GetDeviceInformation()
        {
            return base.ExecuteFunction("GetDeviceInformation", delegate()
            {
                string result = Build.Model;
                string manufacturer = Build.Manufacturer;
                if (!result.StartsWith(manufacturer))
                {
                    result = manufacturer + " " + result;
                }
                result += "," + Build.VERSION.Sdk;
                result += "," + Build.Product;
                return result;
            });
        }

    }
}

