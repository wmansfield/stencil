using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Native.Caching
{
    public interface IDataCache
    {
        Dictionary<string, IDataCacheFilter> Filters { get; set; }
        int CacheCount { get; }
        string[] CacheKeys { get; }

        void Clear();
        void ClearWithPrefix(string prefix);
        bool ContainsKey(string key);

        void AddToCache(string key, object data);
        void RemoveFromCache(string key);
        bool TryGetFromCache<T>(string key, out T value);


        /// <summary>
        /// Returns true if data was retrieved
        /// </summary>
        /// <param name="onRefreshed">true if updated, false if from cache</param>
        Task<bool> WithRefreshAsync<T>(string key, bool allowStaleData, bool forceRefresh, FetchedDelegate<T> onRefreshed, Action<bool> onRefreshing, Func<Task<T>> createMethod) where T : class;
        /// <summary>
        /// Returns true if data was retrieved
        /// </summary>
        /// <param name="onRefreshed">true if updated, false if from cache</param>
        Task<bool> WithRefreshAsync<T>(RequestToken requestToken, string key, bool allowStaleData, bool forceRefresh, FetchedRequestDelegate<T> onRefreshed, Action<bool> onRefreshing, Func<Task<T>> createMethod) where T : class;

    }
}
