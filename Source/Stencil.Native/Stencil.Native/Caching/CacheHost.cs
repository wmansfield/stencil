using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Stencil.Native.Core;

namespace Stencil.Native.Caching
{
    public class CacheHost : BaseClass, ICacheHost
    {
        public CacheHost(string appFolderName)
            : base("CacheHost")
        {
            this.AppFolderName = appFolderName;
        }

        private const string USER_CACHE_FILENAME = "user.cache";
        private const string DATA_CACHE_FILENAME = "data.cache";

        private static object _persistLock = new object();

        public virtual string AppFolderName { get; set; }


        protected virtual string GenerateUserLocation(string fileName)
        {
            return Path.Combine(this.AppFolderName, fileName);
        }
        protected virtual string GenerateDataCacheLocation(string fileName)
        {
            return Path.Combine(this.AppFolderName, "data", fileName);
        }
        protected virtual string GeneratePersistentCacheLocation(string fileName)
        {
            return base.ExecuteFunction<string>("GetUserCacheLocation", delegate()
            {
                return Path.Combine(this.AppFolderName, "config", fileName);
            });
        }
        
        protected virtual IFileStore GetFileStore()
        {
            return Container.FileStore;
        }


        public virtual T CachedUserGet<T>()
            where T : class
        {
            return base.ExecuteFunction("CachedUserGet", delegate()
            {
                try
                {
                    IFileStore fileStore = this.GetFileStore();
                    string path = GenerateUserLocation(USER_CACHE_FILENAME);
                    if (fileStore.Exists(path))
                    {
                        string protectedString = string.Empty;
                        fileStore.TryReadTextFile(path, out protectedString);
                        if (!string.IsNullOrEmpty(protectedString))
                        {
                            string unProtectedString = protectedString;
                            return JsonConvert.DeserializeObject<T>(unProtectedString, new JsonSerializerSettings
                            {
                                TypeNameHandling = TypeNameHandling.All,
                            });
                        }
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    base.LogError(ex, "CachedUserGet");
                    return null;
                }
            });
        }
        public virtual void CachedUserSet<T>(T user)
            where T : class
        {
            base.ExecuteMethod("CachedUserSet", delegate()
            {
                try
                {
                    IFileStore fileStore = this.GetFileStore();
                    string path = GenerateUserLocation(USER_CACHE_FILENAME);

                    string unProtectedString = JsonConvert.SerializeObject(user);
                    string protectedString = string.Empty;
                    if (!string.IsNullOrEmpty(unProtectedString))
                    {
                        protectedString = unProtectedString;
                    }
                    fileStore.EnsureFolderExists(Path.GetDirectoryName(path));
                    fileStore.WriteFile(path, protectedString);
                }
                catch (Exception ex)
                {
                    base.LogError(ex, "CachedUserSet");
                }
            });
        }
        public virtual void CachedUserClear()
        {
            base.ExecuteMethod("CachedUserClear", delegate()
            {
                IFileStore fileStore = this.GetFileStore();
                string path = GenerateUserLocation(USER_CACHE_FILENAME);
                if (fileStore.Exists(path))
                {
                    fileStore.DeleteFile(path);
                }
            });
        }


        public virtual void CachedDataClear()
        {
            base.ExecuteMethod("CachedDataClear", delegate()
            {
                try
                {
                    IFileStore fileStore = this.GetFileStore();
                    string path = GenerateDataCacheLocation("ignore");
                    lock (_persistLock)
                    {
                        if(fileStore.FolderExists(Path.GetDirectoryName(path)))
                        {
                            fileStore.DeleteFolder(Path.GetDirectoryName(path), true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.LogError(ex, "CachedDataClear");
                }
            });
        }
        public virtual bool CachedDataSet<T>(bool secure, string fileName, T item)
        {
            return base.ExecuteFunction<bool>("CachedDataSet", delegate()
            {
                try
                {
                    IFileStore fileStore = this.GetFileStore();
                    string path = GenerateDataCacheLocation(fileName);

                    string unProtectedString = JsonConvert.SerializeObject(item);
                    string protectedString = unProtectedString;
                    if (secure)
                    {
                        if (!string.IsNullOrEmpty(unProtectedString))
                        {
                            protectedString = unProtectedString;
                        }
                    }
                    lock (_persistLock)
                    {
                        fileStore.EnsureFolderExists(Path.GetDirectoryName(path));
                        fileStore.WriteFile(path, protectedString);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    this.LogError(ex, "CachedDataSet");
                    return false;
                }
            });
        }
        public virtual T CachedDataGet<T>(bool secured, string fileName)
        {
            return base.ExecuteFunction<T>("CachedDataGet", delegate()
            {
                try
                {
                    IFileStore fileStore = this.GetFileStore();
                    string path = GenerateDataCacheLocation(fileName);
                    string protectedString = string.Empty;
                    lock (_persistLock)
                    {
                        if (fileStore.Exists(path))
                        {
                            fileStore.TryReadTextFile(path, out protectedString);
                        }
                    }
                    if (!string.IsNullOrEmpty(protectedString))
                    {
                        string unProtectedString = protectedString;
                        if (secured)
                        {
                            unProtectedString = protectedString;
                        }
                        if (!string.IsNullOrEmpty(unProtectedString))
                        {
                            return JsonConvert.DeserializeObject<T>(unProtectedString, new JsonSerializerSettings
                            {
                                TypeNameHandling = TypeNameHandling.All,
                            });
                        }
                    }
                    return default(T);
                }
                catch (Exception ex)
                {
                    this.LogError(ex, "CachedDataGet");
                    return default(T);
                }
            });
        }


        public virtual bool PersistentDataSet<T>(bool secure, string fileName, T item)
        {
            return base.ExecuteFunction<bool>("PersistentDataSet", delegate()
            {
                try
                {
                    IFileStore fileStore = this.GetFileStore();
                    string path = GeneratePersistentCacheLocation(fileName);

                    string unProtectedString = JsonConvert.SerializeObject(item);
                    string protectedString = unProtectedString;
                    if (secure)
                    {
                        if (!string.IsNullOrEmpty(unProtectedString))
                        {
                            protectedString = unProtectedString;
                        }
                    }
                    lock (_persistLock)
                    {
                        fileStore.EnsureFolderExists(Path.GetDirectoryName(path));
                        fileStore.WriteFile(path, protectedString);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    this.LogError(ex, "PersistentDataSet");
                    return false;
                }
            });

        }
        public virtual T PersistentDataGet<T>(bool secured, string fileName)
        {
            return base.ExecuteFunction<T>("PersistentDataGet", delegate()
            {
                try
                {
                    IFileStore fileStore = this.GetFileStore();
                    string path = GeneratePersistentCacheLocation(fileName);

                    string protectedString = string.Empty;
                    lock (_persistLock)
                    {
                        if (fileStore.Exists(path))
                        {
                            fileStore.TryReadTextFile(path, out protectedString);
                        }
                    }
                    if (!string.IsNullOrEmpty(protectedString))
                    {
                        string unProtectedString = protectedString;
                        if (secured)
                        {
                            unProtectedString = protectedString;
                        }
                        if (!string.IsNullOrEmpty(unProtectedString))
                        {
                            return JsonConvert.DeserializeObject<T>(unProtectedString, new JsonSerializerSettings
                            {
                                TypeNameHandling = TypeNameHandling.All,
                            });
                        }
                    }
                    return default(T);
                }
                catch (Exception ex)
                {
                    this.LogError(ex, "PersistentDataGet");
                    return default(T);
                }
            });
        }
    }
}
