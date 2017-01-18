using Foundation;
using Stencil.Native.App;
using Stencil.Native.Caching;
using Stencil.Native.Core;
using Stencil.Native.iOS.Core;
using Stencil.Native.iOS.Core.Caching;
using Stencil.Native.iOS.Core.Services;
using Stencil.Native.Services.MediaUploader;
using System;
using System.Collections.Generic;
using UIKit;

namespace Stencil.Native.iOS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
	[Register ("StencilAppDelegate")]
	public class StencilAppDelegate : UIApplicationDelegate
	{
        #region Constructor

        public StencilAppDelegate()
        {
        }

        #endregion

        #region Static Properties

        public static StencilAppDelegate Current
        {
            get
            {
                return UIApplication.SharedApplication.Delegate as StencilAppDelegate;
            }
        }

        #endregion

        #region Public Properties

        public override UIWindow Window
        {
            get;
            set;
        }

        public virtual UIStoryboard MainStoryboard { get; set; }

        public virtual object DeviceToken { get; set; }

        #endregion

        #region App Delegate Methods

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            #if ENABLE_TEST_CLOUD
			Xamarin.Calabash.Start();
            #endif

            // <Fake ioc>
            Container.Track = new IOSTrack();
            IFileStore fileStore = new IOSFileStore();
            ICacheHost cacheHost = new CacheHost(NativeAssumptions.INTERNAL_APP_NAME);
            IViewPlatform viewPlatform = new IOSViewPlatform();
            IDataCache dataCache = new DataCache(cacheHost);
            IStencilApp stencilApp = new StencilApp(viewPlatform, cacheHost, dataCache, NativeAssumptions.BASE_API_URL);
            IMediaUploader mediaUploader = new IOSMediaUploader();
            Container.RegisterDependencies(fileStore, cacheHost, dataCache, stencilApp, viewPlatform, mediaUploader);
            // </Fake ioc>


            Container.StencilApp.Initialize();

            NSObject notification = null;
            if (launchOptions != null)
            {
                notification = launchOptions.ObjectForKey(UIApplication.LaunchOptionsRemoteNotificationKey);
            }

            if (notification != null)
            {
                //TODO:COULD: Track notification source
            }


            this.InitializeThemes();
            this.InitializeEnvironment();

            this.MainStoryboard = UIStoryboard.FromName("MainStoryboard", null);

            this.Window = new UIWindow(UIScreen.MainScreen.Bounds);


            if (stencilApp.CurrentAccount == null)
            {
                this.LaunchLogin();
            }
            else
            {
                this.LaunchLogin(); //TODO:MUST: Support forked launching
                // make your own launch____ (ie: Launchdashboard)
            }

            this.Window.MakeKeyAndVisible();

            return true;
        }


        public override void OnActivated(UIApplication application)
        {
            CoreUtility.ExecuteMethod("OnActivated", delegate ()
            {
                // Restart any tasks that were paused (or not yet started) while the application was inactive. 
                // If the application was previously in the background, optionally refresh the user interface.

                Container.Track.LogTrace("OnActivated");
                Container.StencilApp.OnAppActivated();
            });
        }
        public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            return CoreUtility.ExecuteFunction("OpenUrl", delegate ()
            {
                //TODO:COULD: Handle url opening
                return false;
            });
        }

        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            CoreUtility.ExecuteMethod("RegisteredForRemoteNotifications", delegate ()
            {
                this.DeviceToken = deviceToken;

                Container.StencilApp.PersistPushNotificationToken(deviceToken.ToString());
            });
        }
        public override void DidRegisterUserNotificationSettings(UIApplication application, UIUserNotificationSettings notificationSettings)
        {
            CoreUtility.ExecuteMethod("DidRegisterUserNotificationSettings", delegate ()
            {
                application.RegisterForRemoteNotifications(); // second part, first from viewplatform
            });
        }
        public override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
        {
            this.ProcessNotification(application, userInfo, null);
        }
        public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        {
            this.ProcessNotification(application, userInfo, completionHandler);
        }

        public override void ReceiveMemoryWarning(UIApplication application)
        {
            CoreUtility.ExecuteMethod("ReceiveMemoryWarning", delegate ()
            {
                Container.Track.LogWarning("ReceivedMemoryWarning");
                Container.ViewPlatform.OnMemoryWarning();
            });
        }



        public override void OnResignActivation(UIApplication application)
        {
            // Invoked when the application is about to move from active to inactive state.
            // This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
            // or when the user quits the application and it begins the transition to the background state.
            // Games should use this method to pause the game.
        }

        public override void DidEnterBackground(UIApplication application)
        {
            // Use this method to release shared resources, save user data, invalidate timers and store the application state.
            // If your application supports background exection this method is called instead of WillTerminate when the user quits.
        }
        public override void WillEnterForeground(UIApplication application)
        {
            // Called as part of the transiton from background to active state.
            // Here you can undo many of the changes made on entering the background.
        }

        public override void WillTerminate(UIApplication application)
        {
            // Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
        }

        #endregion

        #region Public Methods

        public virtual void LaunchLogin(UIViewAnimationOptions transition = UIViewAnimationOptions.TransitionFlipFromLeft)
        {
            CoreUtility.ExecuteMethod("LaunchLogin", delegate ()
            {
                UIViewController firstController = this.MainStoryboard.InstantiateViewController(LoginController.IDENTIFIER);
                UINavigationController navController = new UINavigationController(firstController);
                navController.NavigationBarHidden = true;
                this.ChangeRootViewController(navController, transition);
            });
        }


        #endregion

        #region Protected Methods



        protected void ProcessNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        {
            CoreUtility.ExecuteMethod("ProcessNotification", delegate ()
            {
                bool fromBackground = (application.ApplicationState == UIApplicationState.Inactive || application.ApplicationState == UIApplicationState.Background);
                PushNotificationProcessor processor = new PushNotificationProcessor(this);
                processor.ProcessRemotePushNotification(userInfo, fromBackground, completionHandler);
            });
        }

        protected virtual void ChangeRootViewController(UIViewController newController, UIViewAnimationOptions transition)
        {
            CoreUtility.ExecuteMethod("ChangeRootViewController", delegate ()
            {
                UIViewController previousRoot = this.Window.RootViewController;
                if (this.Window.RootViewController == null)
                {
                    this.Window.RootViewController = newController;
                }
                else
                {
                    UIView.Transition(this.Window, 0.5, transition, delegate ()
                    {
                        this.Window.RootViewController = newController;
                    }, null);
                }
                if (previousRoot != null)
                {
                    previousRoot.Dispose();
                }
            });
        }

        protected virtual void InitializeEnvironment()
        {
            CoreUtility.ExecuteMethod("InitializeEnvironment", delegate ()
            {
                //TODO:COULD: Any 3rd party bootstrap-like settings
            });
        }

        protected virtual void InitializeThemes()
        {
            CoreUtility.ExecuteMethod("InitializeThemes", delegate ()
            {
                //TODO:COULD: Any default fonts and appearances
                /* SAMPLES: 
                UINavigationBar.Appearance.SetTitleTextAttributes(new UITextAttributes()
                {
                    TextColor = UIColor.White,
                    Font = UIFont.FromName(NativeAssumptions.FONT_MEDIUM, 17f),
                });

                UINavigationBar.Appearance.TintColor = UIColor.Black; // careful changing these, its global!
                UINavigationBar.Appearance.BarTintColor = UIColor.White; // careful changing these, its global!


                UIBarButtonItem.Appearance.SetTitleTextAttributes(new UITextAttributes()
                {
                    TextColor = UIColor.White,
                    Font = UIFont.FromName(NativeAssumptions.FONT_MEDIUM, 17f),
                },
                UIControlState.Normal);

                UIBarButtonItem.Appearance.SetTitleTextAttributes(new UITextAttributes()
                {
                    TextColor = UIColor.Gray,
                    Font = UIFont.FromName(NativeAssumptions.FONT_MEDIUM, 17f),
                },
                UIControlState.Disabled);

                var style = UIBarButtonItem.AppearanceWhenContainedIn(typeof(UIImagePickerController));
                style.SetTitleTextAttributes(new UITextAttributes()
                {
                    Font = UIFont.FromName(NativeAssumptions.FONT_MEDIUM, 17f),
                    TextColor = UIColor.Black
                },
                UIControlState.Normal);


                style = UIBarButtonItem.AppearanceWhenContainedIn(typeof(EventKitUI.EKEventEditViewController));
                style.SetTitleTextAttributes(new UITextAttributes()
                {
                    Font = UIFont.FromName(NativeAssumptions.FONT_MEDIUM, 17f),
                    TextColor = UIColor.Black
                },
                UIControlState.Normal);

                */

            });
        }

        #endregion


        #region Event Handlers

        public void OnStatusBarTouchesBegan(NSSet touches, UIEvent evt)
        {
            CoreUtility.ExecuteMethod("OnStatusBarTouchesBegan", delegate ()
            {
                UIViewController topController = UIApplication.SharedApplication.GetRootVisibleController();
                BaseUIViewController baseController = topController as BaseUIViewController;
                if (baseController != null)
                {
                    baseController.ScrollToTop();
                }
            });
        }

        #endregion
    }
}


