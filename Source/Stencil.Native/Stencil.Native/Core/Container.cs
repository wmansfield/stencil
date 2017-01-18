using Stencil.Native.Caching;
using Stencil.Native.Services.MediaUploader;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stencil.Native.Core
{
    public static partial class Container
    {
        static Container()
        {
            Container.Track = new CoreTrack();
        }

        public static IFileStore FileStore;
        public static ITrack Track;
        public static IViewPlatform ViewPlatform;
        public static IDataCache DataCache;
        public static ICacheHost CacheHost;
        public static IStencilApp StencilApp;

        // Function pattern because android requires delayed creation
        public static Func<IMediaUploader> MediaUploader = delegate
        {
            return _mediaUploader;
        };
        private static IMediaUploader _mediaUploader;


        /// <summary>
        /// Not the standard IoC, but it'll work just fine
        /// </summary>
        public static void RegisterDependencies(IFileStore fileStore, ICacheHost cacheHost, IDataCache dataCache, IStencilApp stencilApp, IViewPlatform platform, IMediaUploader mediaUploader)
        {
            Container.ViewPlatform = platform;
            Container.StencilApp = stencilApp;
            Container.FileStore = fileStore;
            Container.DataCache = dataCache;
            Container.CacheHost = cacheHost;
            Container._mediaUploader = mediaUploader;
        }

    }
}
