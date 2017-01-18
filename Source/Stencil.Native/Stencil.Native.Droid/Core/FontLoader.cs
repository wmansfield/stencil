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
using Android.Graphics;
using Android.Content.Res;
using Stencil.Native.Core;

namespace Stencil.Native.Droid.Core
{
    public static class FontLoader
    {
        private static Dictionary<string, Typeface> _FontCache = new Dictionary<string, Typeface>(StringComparer.OrdinalIgnoreCase);
        private static object _FontSyncRoot = new object();

        public static Typeface GetFont(AssetManager assets, string assetPath)
        {
            try
            {
                Typeface result = null;
                if (!_FontCache.TryGetValue(assetPath, out result))
                {
                    lock (_FontSyncRoot)
                    {
                        if (!_FontCache.TryGetValue(assetPath, out result))
                        {
                            result = Typeface.CreateFromAsset(assets, assetPath);
                            _FontCache[assetPath] = result;
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                Container.Track.LogWarning(ex.Message, "FontLoader.GetFont: " + assetPath);
                return null;
            }
        }
    }
}