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
using Stencil.Native.Droid.Core.Services;
using Com.Nostra13.Universalimageloader.Core;
using Android.Graphics;
using Com.Nostra13.Universalimageloader.Core.Assist;
using Stencil.Native.Droid;

namespace Stencil.Native.Core
{
    public static partial class Container
    {
        public static ImageLoader ImageLoader { get; set; }

        private static DisplayImageOptions _imageLoaderDefaultOptions;
        public static DisplayImageOptions ImageLoaderDefaultOptions
        {
            get
            {
                if (_imageLoaderDefaultOptions == null)
                {
                    DisplayImageOptions options = new DisplayImageOptions.Builder()
                        .BitmapConfig(Bitmap.Config.Rgb565)
                        .ImageScaleType(ImageScaleType.Exactly)
                        .CacheOnDisk(true)
                        .CacheInMemory(true)
                        .ShowImageOnFail(Resource.Drawable.empty)
                        .Build();
                    _imageLoaderDefaultOptions = options;
                    return options;
                }
                return _imageLoaderDefaultOptions;
            }
        }


        private static MediaUploaderWrapper _wrapper;
        public static MediaUploaderWrapper MediaUploaderWrapper
        {
            get
            {
                return _wrapper;
            }
            set
            {
                _wrapper = value;
                MediaUploader = delegate()
                {
                    return _wrapper.Instance;
                };
            }
        }

    }
}