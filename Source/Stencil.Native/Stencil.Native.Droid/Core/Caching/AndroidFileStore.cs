using System;
using Stencil.Native.Caching;
using Android.Content;
using System.IO;

namespace Stencil.Native.Droid.Core.Caching
{
    public class AndroidFileStore : BaseFileStore
    {
        public AndroidFileStore(Context context)
        {
            this.Context = context;
        }

        private Context Context { get; set; }

        public override string NativePath(string path)
        {
            return Path.Combine(Context.FilesDir.Path, path);
        }
    }
}

