using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Provider;
using Environment = Android.OS.Environment;
using Path = System.IO.Path;
using Uri = Android.Net.Uri;

namespace Stencil.Native.Droid.Core
{
    public static class _XamarinExtensions
    {

        #region Xamarin Media Fixes

        internal const string ExtraName = "MediaFile";

        public static Task<MediaFileFix> GetMediaFileExtraAsync_Fix(this Intent self, Context context)
        {
            if (self == null)
                throw new ArgumentNullException("self");
            if (context == null)
                throw new ArgumentNullException("context");

            string action = self.GetStringExtra("action");
            if (action == null)
                throw new ArgumentException("Intent was not results from MediaPicker", "self");

            var uri = (Android.Net.Uri)self.GetParcelableExtra(_XamarinExtensions.ExtraName);
            bool isPhoto = self.GetBooleanExtra("isPhoto", false);
            var path = (Android.Net.Uri)self.GetParcelableExtra("path");

            return _XamarinExtensions.GetMediaFileAsync(context, 0, action, isPhoto, ref path, uri)
                .ContinueWith(t => t.Result.ToTask()).Unwrap();
        }

        internal static Task<MediaPickedEventArgs> GetMediaFileAsync(Context context, int requestCode, string action, bool isPhoto, ref Android.Net.Uri path, Android.Net.Uri data)
        {
            Task<Tuple<string, bool>> pathFuture;

            string originalPath = null;

            if (action != Intent.ActionPick)
            {
                originalPath = path.Path;

                // Not all camera apps respect EXTRA_OUTPUT, some will instead
                // return a content or file uri from data.
                if (data != null && data.Path != originalPath && !File.Exists(originalPath))
                {
                    originalPath = data.ToString();
                    string currentPath = path.Path;
                    pathFuture = TryMoveFileAsync(context, data, path, isPhoto).ContinueWith(t =>
                      new Tuple<string, bool>(t.Result ? currentPath : null, false));
                }
                else
                    pathFuture = TaskFromResult(new Tuple<string, bool>(path.Path, false));
            }
            else if (data != null)
            {
                originalPath = data.ToString();
                path = data;
                pathFuture = GetFileForUriAsync(context, path, isPhoto);
            }
            else
                pathFuture = TaskFromResult<Tuple<string, bool>>(null);

            return pathFuture.ContinueWith(t => {

                string resultPath = t.Result.Item1;
                if (!string.IsNullOrEmpty(resultPath) && File.Exists(resultPath))
                {
                    var mf = new MediaFileFix(resultPath, deletePathOnDispose: t.Result.Item2);
                    return new MediaPickedEventArgs(requestCode, false, mf);
                }
                else
                    return new MediaPickedEventArgs(requestCode, new Xamarin.Media.MediaFileNotFoundException(originalPath));
            });
        }
        private static Task<bool> TryMoveFileAsync(Context context, Uri url, Uri path, bool isPhoto)
        {
            string moveTo = GetLocalPath(path);
            return GetFileForUriAsync(context, url, isPhoto).ContinueWith(t => {
                if (t.Result.Item1 == null)
                    return false;

                File.Delete(moveTo);
                File.Move(t.Result.Item1, moveTo);

                if (url.Scheme == "content")
                    context.ContentResolver.Delete(url, null, null);

                return true;
            }, TaskScheduler.Default);
        }
        internal static Task<Tuple<string, bool>> GetFileForUriAsync(Context context, Uri uri, bool isPhoto)
        {
            var tcs = new TaskCompletionSource<Tuple<string, bool>>();

            if (uri.Scheme == "file")
                tcs.SetResult(new Tuple<string, bool>(new System.Uri(uri.ToString()).LocalPath, false));
            else if (uri.Scheme == "content")
            {
                Task.Factory.StartNew(() =>
                {
                    ICursor cursor = null;
                    try
                    {
                        string contentPath = null;
                        try
                        {
                            string[] proj = null;
                            //Android 5.1.1 requires projection
                            if ((int)Build.VERSION.SdkInt >= 22)
                                proj = new[] { MediaStore.MediaColumns.Data };
                            cursor = context.ContentResolver.Query(uri, proj, null, null, null);
                        }
                        catch (Exception)
                        {
                        }
                        if (cursor != null && cursor.MoveToNext())
                        {
                            int column = cursor.GetColumnIndex(MediaStore.MediaColumns.Data);
                            if (column != -1)
                                contentPath = cursor.GetString(column);
                        }

                        bool copied = false;

                        // If they don't follow the "rules", try to copy the file locally
                        if (contentPath == null || !contentPath.StartsWith("file"))
                        {
                            copied = true;
                            Uri outputPath = GetOutputMediaFile(context, "temp", null, isPhoto);

                            try
                            {
                                
                                using (Stream input = context.ContentResolver.OpenInputStream(uri))
                                using (Stream output = File.Create(outputPath.Path))
                                    input.CopyTo(output);
                                contentPath = outputPath.Path;
                            }
                            catch (Exception)
                            {
                                // If there's no data associated with the uri, we don't know
                                // how to open this. contentPath will be null which will trigger
                                // MediaFileNotFoundException.
                            }
                        }
                        tcs.SetResult(new Tuple<string, bool>(contentPath, copied));
                    }
                    finally
                    {
                        if (cursor != null)
                        {
                            cursor.Close();
                            cursor.Dispose();
                        }
                    }
                }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
            }
            else
                tcs.SetResult(new Tuple<string, bool>(null, false));

            return tcs.Task;
        }
        private static Uri GetOutputMediaFile(Context context, string subdir, string name, bool isPhoto)
        {
            subdir = subdir ?? String.Empty;

            if (String.IsNullOrWhiteSpace(name))
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                if (isPhoto)
                    name = "IMG_" + timestamp + ".jpg";
                else
                    name = "VID_" + timestamp + ".mp4";
            }

            string mediaType = (isPhoto) ? Environment.DirectoryPictures : Environment.DirectoryMovies;
            using (Java.IO.File mediaStorageDir = new Java.IO.File(context.GetExternalFilesDir(mediaType), subdir))
            {
                if (!mediaStorageDir.Exists())
                {
                    if (!mediaStorageDir.Mkdirs())
                        throw new IOException("Couldn't create directory, have you added the WRITE_EXTERNAL_STORAGE permission?");

                    // Ensure this media doesn't show up in gallery apps
                    using (Java.IO.File nomedia = new Java.IO.File(mediaStorageDir, ".nomedia"))
                        nomedia.CreateNewFile();
                }

                return Uri.FromFile(new Java.IO.File(GetUniquePath(mediaStorageDir.Path, name, isPhoto)));
            }
        }
        private static string GetUniquePath(string folder, string name, bool isPhoto)
        {
            string ext = Path.GetExtension(name);
            if (ext == String.Empty)
                ext = ((isPhoto) ? ".jpg" : ".mp4");

            name = Path.GetFileNameWithoutExtension(name);

            string nname = name + ext;
            int i = 1;
            while (File.Exists(Path.Combine(folder, nname)))
                nname = name + "_" + (i++) + ext;

            return Path.Combine(folder, nname);
        }
        private static string GetLocalPath(Uri uri)
        {
            return new System.Uri(uri.ToString()).LocalPath;
        }


        private static Task<T> TaskFromResult<T>(T result)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetResult(result);
            return tcs.Task;
        }

        
        internal class MediaPickedEventArgs
        : EventArgs
        {
            public MediaPickedEventArgs(int id, Exception error)
            {
                if (error == null)
                    throw new ArgumentNullException("error");

                RequestId = id;
                Error = error;
            }

            public MediaPickedEventArgs(int id, bool isCanceled, MediaFileFix media = null)
            {
                RequestId = id;
                IsCanceled = isCanceled;
                if (!IsCanceled && media == null)
                    throw new ArgumentNullException("media");

                Media = media;
            }

            public int RequestId
            {
                get;
                private set;
            }

            public bool IsCanceled
            {
                get;
                private set;
            }

            public Exception Error
            {
                get;
                private set;
            }

            public MediaFileFix Media
            {
                get;
                private set;
            }

            public Task<MediaFileFix> ToTask()
            {
                var tcs = new TaskCompletionSource<MediaFileFix>();

                if (IsCanceled)
                    tcs.SetCanceled();
                else if (Error != null)
                    tcs.SetException(Error);
                else
                    tcs.SetResult(Media);

                return tcs.Task;
            }
        }
        #endregion
    }
    public sealed class MediaFileFix : IDisposable
    {
        internal MediaFileFix(string path, bool deletePathOnDispose)
        {
            this.deletePathOnDispose = deletePathOnDispose;
            this.path = path;
        }

        public string Path
        {
            get
            {
                if (this.isDisposed)
                    throw new ObjectDisposedException(null);

                return this.path;
            }
        }

        public Stream GetStream()
        {
            if (this.isDisposed)
                throw new ObjectDisposedException(null);

            return File.OpenRead(this.path);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool isDisposed;
        private readonly bool deletePathOnDispose;
        private readonly string path;

        internal const string ExtraName = "MediaFile";

        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
                return;

            this.isDisposed = true;
            if (this.deletePathOnDispose)
            {
                try
                {
                    File.Delete(this.path);
                    // We don't really care if this explodes for a normal IO reason.
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (DirectoryNotFoundException)
                {
                }
                catch (IOException)
                {
                }
            }
        }

        ~MediaFileFix()
        {
            Dispose(false);
        }
    }
}