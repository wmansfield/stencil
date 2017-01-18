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
using Stencil.Native.Caching;
using Stencil.Native.Core;
using Stencil.Native.Droid.Core.Caching;
using Stencil.Native.App;
using Stencil.Native.Droid.Core;
using Com.Nostra13.Universalimageloader.Core;
using Com.Nostra13.Universalimageloader.Core.Display;
using Com.Nostra13.Universalimageloader.Core.Assist;
using System.Threading.Tasks;
using Stencil.Native.Droid.Core.Services;
using Stencil.Native.Droid.Push;
using Android.Graphics;

namespace Stencil.Native.Droid
{
#if DEBUG
    [Application(Debuggable = true)]
#else
    [Application(Debuggable=false)]
#endif
    public class StencilApplication : Application
    {
        public StencilApplication(IntPtr handle, JniHandleOwnership transfer)
            : base(handle, transfer)
        {
            // <Fake ioc>
            IFileStore fileStore = new AndroidFileStore(this);
            ICacheHost cacheHost = new CacheHost(NativeAssumptions.INTERNAL_APP_NAME);
            IViewPlatform viewPlatform = new AndroidViewPlatform(this);
            IDataCache dataCache = new DataCache(cacheHost);
            IStencilApp stencilApp = new StencilApp(viewPlatform, cacheHost, dataCache, NativeAssumptions.BASE_API_URL);
            Container.RegisterDependencies(fileStore, cacheHost, dataCache, stencilApp, viewPlatform, null);
            // </Fake ioc>

            Container.StencilApp.Initialize();

            this.StartServices();
        }

        public override void OnCreate()
        {
            CoreUtility.ExecuteMethod("StencilApplication.OnCreate", delegate ()
            {
                base.OnCreate();

                this.InitializeEnvironment();

                Container.StencilApp.OnAppActivated();
            });
        }

        protected void InitializeEnvironment()
        {
            CoreUtility.ExecuteMethod("InitializeEnvironment", delegate ()
            {
                DisplayImageOptions defaultOptions = new DisplayImageOptions.Builder()
                    .BitmapConfig(Bitmap.Config.Rgb565)
                    .ImageScaleType(ImageScaleType.Exactly)
                    .CacheOnDisc(true)
                    .CacheInMemory(true)
                    .ShowImageOnFail(Resource.Drawable.empty)
                    .Displayer(new FadeInBitmapDisplayer(200))
                    .Build();
                ImageLoaderConfiguration config = new ImageLoaderConfiguration.Builder(BaseContext)
                    .DefaultDisplayImageOptions(defaultOptions)
                    .Build();

                ImageLoader.Instance.Init(config);

                Container.ImageLoader = ImageLoader.Instance;
            });
        }
        protected void StartServices()
        {
            Task.Run(delegate ()
            {
                CoreUtility.ExecuteMethod("StartServices", delegate ()
                {
                    this.StartService(new Intent(this, typeof(UploadService)));

                    UploadServiceConnection connection = new UploadServiceConnection(null);

                    Intent uploadServiceIntent = new Intent(this.ApplicationContext, typeof(UploadService));
                    this.ApplicationContext.BindService(uploadServiceIntent, connection, Bind.AutoCreate);

                    Container.MediaUploaderWrapper = new MediaUploaderWrapper(connection);
                });
            });
        }

       
    }
}