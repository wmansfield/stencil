using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Stencil.Native.App;
using Stencil.Native.Core;

namespace Stencil.Native.Caching
{
    /// <remarks>v2014.9.17</remarks>
    public class DataCache : BaseClass, IDataCache
    {
        #region Constructor

        public DataCache(ICacheHost cacheHost)
            : base("DataCache")
        {
            this.CacheHost = cacheHost;
            this.CachedData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            this.Executing = new HashSet<string>();
            this.Filters = new Dictionary<string, IDataCacheFilter>(StringComparer.OrdinalIgnoreCase);
            this.Filters.Add(SizeDataCacheFilter.NAME, new SizeDataCacheFilter(100000, 20, 5));
        }

        #endregion

        #region Protected Statics

        protected static string _cacheKey = "datacache.cache";
        protected static object _CacheWriteLock = new object();
        protected static object _ExecutingLock = new object();
        protected static object _PersistLock = new object();

        #endregion

        #region Protected Properties

        protected virtual ICacheHost CacheHost { get; set; }

        protected virtual Dictionary<string, object> CachedData { get; set; }
        protected virtual HashSet<string> Executing { get; set; }
        protected virtual bool Initialized { get; set; }
        protected virtual bool IsPersisting { get; set; }
        protected virtual bool PersistRequested { get; set; }

        #endregion

        #region Public Properties

        public virtual Dictionary<string, IDataCacheFilter> Filters { get; set; }

        public virtual int CacheCount
        {
            get
            {
                return this.CachedData.Count;
            }
        }
        public virtual string[] CacheKeys
        {
            get
            {
                return this.CachedData.Keys.ToArray();
            }
        }

        #endregion

        #region Public Methods

        public void Clear()
        {
            base.ExecuteMethod("Clear", delegate()
            {
                lock (_CacheWriteLock)
                {
                    this.CachedData.Clear();
                }
                this.DelayPersist();
            });
        }
        public void ClearWithPrefix(string prefix)
        {
            base.ExecuteMethod("ClearWithPrefix", delegate()
            {
                lock (_CacheWriteLock)
                {
                    string[] keys = this.CachedData.Keys.ToArray();
                    foreach (string key in keys)
                    {
                        if (key.StartsWith(prefix))
                        {
                            this.CachedData.Remove(key);
                        }
                    }
                }
                this.DelayPersist();
            });
        }

        public bool ContainsKey(string key)
        {
            return base.ExecuteFunction<bool>("ContainsKey", delegate()
            {
                return this.CachedData.ContainsKey(key);
            });
        }

        public virtual void AddToCache(string key, object data)
        {
            base.ExecuteMethod("AddToCache", delegate()
            {
                lock (_CacheWriteLock)
                {
                    this.CachedData[key] = data;
                }
                this.DelayPersist();
            });
        }
        public virtual void RemoveFromCache(string key)
        {
            base.ExecuteMethod("RemoveFromCache", delegate()
            {
                lock (_CacheWriteLock)
                {
                    this.CachedData.Remove(key);
                }
                this.DelayPersist();
            });
        }
        public virtual bool TryGetFromCache<T>(string key, out T value)
        {
            value = default(T);
            try
            {
                lock (_CacheWriteLock)
                {
                    if (this.CachedData.ContainsKey(key))
                    {
                        value = (T)this.CachedData[key];
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                this.LogError(ex, "TryGetFromCache");
                return false;
            }
        }
        public Task<bool> WithRefreshAsync<T>(string key, bool allowStaleData, bool forceRefresh, FetchedDelegate<T> onRefreshed, Action<bool> onRefreshing, Func<Task<T>> createMethod)
            where T : class
        {
            FetchedRequestDelegate<T> wrapper = null;
            if (onRefreshed != null)
            {
                wrapper = (token, freshData, data) =>
                {
                    onRefreshed(freshData, data);
                };
            }
            return this.WithRefreshAsync<T>(RequestToken.Empty, key, allowStaleData, forceRefresh, wrapper, onRefreshing, createMethod);

        }
        /// <summary>
        /// Returns true if data was retrieved
        /// </summary>
        /// <param name="onRefreshed">true if updated, false if from cache</param>
        public async Task<bool> WithRefreshAsync<T>(RequestToken token, string key, bool allowStaleData, bool forceRefresh, FetchedRequestDelegate<T> onRefreshed, Action<bool> onRefreshing, Func<Task<T>> createMethod)
            where T : class
        {
            this.EnsureInitialized();

            IDataCacheFilter[] filters = this.Filters.Values.ToArray();
            // see if we can use old data
            bool canReadCached = true;
            foreach (var item in filters)
            {
                if (!item.CanReadCached(this, key))
                {
                    canReadCached = false;
                    break;
                }
            }
            T foundData = null;

            // use old data if we can
            if (allowStaleData && canReadCached && CachedData.ContainsKey(key))
            {
                foundData = CachedData[key] as T;
                if (foundData == null)
                {
                    forceRefresh = true; //had it, but it was bad data
                }
            }
            // see if we need to refresh
            if (!forceRefresh)
            {
                foreach (var item in filters)
                {
                    if (item.RefreshRequired(this, key))
                    {
                        forceRefresh = true;
                        break;
                    }
                }
            }

            if (foundData != null)
            {
                if (onRefreshed != null)
                {
                    try
                    {
                        onRefreshed(token, false, foundData); // its not fresh
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex, "onRefreshed:old");
                    }
                }
            }

            // get the data if we need to
            if (foundData == null || forceRefresh || !canReadCached || !CachedData.ContainsKey(key))
            {
                // single request at a time
                lock (_ExecutingLock)
                {
                    if (this.Executing.Contains(key))
                    {
                        return false;
                    }
                    this.Executing.Add(key);
                }
                try
                {
                    // get the data
                    foreach (var item in filters)
                    {
                        item.OnBeforeItemRetrieved(this, key);
                    }
                    if (onRefreshing != null)
                    {
                        try
                        {
                            onRefreshing(true);
                        }
                        catch (Exception ex)
                        {
                            this.LogError(ex, "onRefreshing:true");
                        }
                    }
                    T data = null;
                    if (createMethod != null)
                    {
                        data = await createMethod();
                    }

                    foreach (var item in filters)
                    {
                        item.OnAfterItemRetrieved(this, key, data);
                    }

                    if (data != null)
                    {
                        bool canWriteToCache = true;

                        foreach (var item in filters)
                        {
                            if (!item.CanSaveToCache(this, key, data))
                            {
                                canWriteToCache = false;
                                break;
                            }
                        }

                        if (canWriteToCache)
                        {
                            foreach (var item in filters)
                            {
                                item.OnBeforeItemSavedToCache(this, key, data);
                            }

                            this.AddToCache(key, data);

                            foreach (var item in filters)
                            {
                                item.OnAfterItemSavedToCache(this, key, data);
                            }

                        }
                        if (onRefreshed != null)
                        {
                            try
                            {
                                onRefreshed(token, true, data);
                            }
                            catch (Exception ex)
                            {
                                this.LogError(ex, "onRefreshed:new");
                            }
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                finally
                {
                    lock (_ExecutingLock)
                    {
                        this.Executing.Remove(key);
                    }
                    if (onRefreshing != null)
                    {
                        try
                        {
                            onRefreshing(false);
                        }
                        catch (Exception ex)
                        {
                            this.LogError(ex, "onRefreshing:false");
                        }
                    }
                }
            }
            else
            {
                if (onRefreshing != null)
                {
                    try
                    {
                        onRefreshing(false);
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex, "onRefreshing:false");
                    }
                }
                return false;
            }
        }

        #endregion

        #region Protected Methods

        protected virtual void EnsureInitialized()
        {
            base.ExecuteMethod("EnsureInitialized", delegate()
            {
                if (!this.Initialized)
                {
                    lock (_CacheWriteLock) // don't let anyone write until we're done initializing
                    {
                        if (!this.Initialized)
                        {
                            try
                            {
                                string savedCacheString = this.CacheHost.CachedDataGet<string>(false, _cacheKey);
                                if (!string.IsNullOrEmpty(savedCacheString))
                                {
                                    //Can't use dictionary with generics without a custom constrctor, since its deserialized, we need to fix
                                    List<KeyValuePair<string, object>> items = JsonConvert.DeserializeObject<List<KeyValuePair<string, object>>>(savedCacheString, new JsonSerializerSettings
                                    {
                                        TypeNameHandling = TypeNameHandling.All,
                                    });
                                    if (items != null)
                                    {
                                        Dictionary<string, object> savedCache = new Dictionary<string, object>(StringComparer.Ordinal);
                                        foreach (var item in items)
                                        {
                                            savedCache[item.Key] = item.Value;
                                        }
                                        this.CachedData = savedCache;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                this.LogError(ex, "EnsureInitialized");
                                this.CacheHost.CachedDataSet<string>(false, _cacheKey, string.Empty); // bad data, discard
                            }

                            this.Initialized = true;
                        }
                    }
                }
            });
        }

        protected virtual void DelayPersist()
        {
            base.ExecuteMethod("DelayPersist", delegate()
            {
                if (!PersistRequested)
                {
                    DateTime now = DateTime.Now;
                    Task.Run(async delegate()
                    {
                        await Task.Delay(5000);
                        if(this.PersistRequested)
                        {
                            this.PersistRequested = false;
                            this.PerformPersist();
                        }
                    });
                    this.PersistRequested = true;
                }
            });
        }
        protected virtual void PerformPersist()
        {
            base.ExecuteMethod("PerformPersist", delegate()
            {
                // this also enforces locking on retrieval (since only initialize calls it)
                if (!this.Initialized) { return; }
                if (this.IsPersisting) { return; }

                lock (_PersistLock)
                {
                    if (!IsPersisting)
                    {
                        this.IsPersisting = true;
                        try
                        {
                            string serialized = string.Empty;
                            lock (_CacheWriteLock)
                            {
                                List<KeyValuePair<string, object>> items = new List<KeyValuePair<string, object>>(); //Can't use dictionary with generics without a custom constrctor, since its deserialized, we need to fix
                                foreach (var item in this.CachedData)
                                {
                                    items.Add(item);
                                }
                                serialized = JsonConvert.SerializeObject(items, Formatting.Indented, new JsonSerializerSettings
                                {
                                    TypeNameHandling = TypeNameHandling.All,
                                });
                                #if DEBUG
                                //Container.Track.LogTrace("Cache Size:" + serialized.Length);
                                #endif
                            }
                            this.CacheHost.CachedDataSet(false, _cacheKey, serialized);
                        }
                        catch (Exception ex)
                        {
                            this.LogError(ex, "PerformPersist");
                        }
                        finally
                        {
                            this.IsPersisting = false;
                        }
                    }
                }
            });
        }

        #endregion

    }
}
