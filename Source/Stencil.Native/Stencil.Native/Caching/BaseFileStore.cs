using System;
using System.IO;

namespace Stencil.Native.Caching
{
#if !WINDOWS_PHONE_APP
    public abstract class BaseFileStore : IFileStore
    {
        public BaseFileStore()
        {
        }

        public abstract string NativePath(string filePath);

        #region ICacheFileStore implementation

        public Stream OpenRead(string path)
        {
            var fullPath = NativePath(path);
            if (!System.IO.File.Exists(fullPath))
            {
                return null;
            }
            return System.IO.File.OpenRead(fullPath);
        }
        public Stream OpenWrite(string path)
        {
            var fullPath = NativePath(path);
            if (!System.IO.File.Exists(fullPath))
            {
                return System.IO.File.Create(fullPath);
            }
            return System.IO.File.OpenWrite(fullPath);
        }
        public virtual bool Exists(string filePath)
        {
            string fullPath = NativePath(filePath);
            return System.IO.File.Exists(fullPath);
        }

        public virtual bool FolderExists(string filePath)
        {
            string fullPath = NativePath(filePath);
            return System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(fullPath));
        }

        public virtual bool TryReadTextFile(string path, out string contents)
        {
            string result = null;
            var toReturn = TryReadFileCommon(path, (stream) =>
            {
                using (var streamReader = new StreamReader(stream))
                {
                    result = streamReader.ReadToEnd();
                }
                return true;
            });
            contents = result;
            return toReturn;
        }


        public virtual void EnsureFolderExists(string folderPath)
        {
            string fullPath = NativePath(folderPath);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }

        public virtual void WriteFile(string path, string contents)
        {
            WriteFileCommon(path, (stream) =>
            {
                using (var sw = new StreamWriter(stream))
                {
                    sw.Write(contents);
                    sw.Flush();
                }
            });
        }

        public virtual void DeleteFile(string filePath)
        {
            var fullPath = NativePath(filePath);
            System.IO.File.Delete(fullPath);
        }

        public virtual void DeleteFolder(string folderPath, bool recursive)
        {
            var fullPath = NativePath(folderPath);
            if (FolderExists(fullPath))
            {
                Directory.Delete(fullPath, recursive);
            }
        }

        #endregion


        protected virtual bool TryReadFileCommon(string path, Func<Stream, bool> streamAction)
        {
            string fullPath = NativePath(path);
            if (!System.IO.File.Exists(fullPath))
            {
                return false;
            }
            using (var fileStream = System.IO.File.OpenRead(fullPath))
            {
                return streamAction(fileStream);
            }
        }

        protected virtual void WriteFileCommon(string path, Action<Stream> streamAction)
        {
            string fullPath = NativePath(path);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
            using (var fileStream = System.IO.File.OpenWrite(fullPath))
            {
                streamAction(fileStream);
            }
        }
    }
#endif

}

