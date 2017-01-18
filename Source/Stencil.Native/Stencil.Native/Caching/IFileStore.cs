using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Stencil.Native.Caching
{
    /// <summary>
    /// Redirect file storage to cache locations
    /// </summary>
    public interface IFileStore
    {
        bool Exists(string filePath);
        bool FolderExists(string folderPath);
        bool TryReadTextFile(string filePath, out string contents);
        void EnsureFolderExists(string folderPath);
        void WriteFile(string filePath, string contents);
        void DeleteFile(string filePath);
        void DeleteFolder(string folderPath, bool recursive);
        Stream OpenRead(string path);
        Stream OpenWrite(string path);
    }
}
