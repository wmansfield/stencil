using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Native.Caching
{
    public static class _DataCacheExtensions
    {
        private static object _timedSyncLock = new object();
        private static string _timedCacheKey = "WithTimedRefreshAsync-11BCE693-EB56-4CB7-B866-6505C70241F1";

        public static void InvalidateTimedCache(this IDataCache dataCache)
        {
            TimedDataCacheFilter timeFilter = EnsureTimedLifetimeFilter(dataCache);
            timeFilter.ClearAll();
        }
        public static void InvalidateTimedPrefix(this IDataCache dataCache, string prefix)
        {
            TimedDataCacheFilter timeFilter = EnsureTimedLifetimeFilter(dataCache);
            timeFilter.ClearWithPrefix(prefix);
        }
        public static bool HasTimeExpiredFor(this IDataCache dataCache, string key, int maximumStaleSeconds)
        {
            TimedDataCacheFilter timeFilter = EnsureTimedLifetimeFilter(dataCache);
            return timeFilter.RefreshRequired(key, maximumStaleSeconds);
        }
       
        public static async Task<bool> WithTimedRefreshAsync<T>(this IDataCache dataCache, RequestToken requestToken, string key, int maximumStaleSeconds, FetchedRequestDelegate<T> onRefreshed, Action<bool> onRefreshing, Func<Task<T>> createMethod)
            where T : class
        {
            TimedDataCacheFilter timeFilter = EnsureTimedLifetimeFilter(dataCache);
            bool forceRefresh = (maximumStaleSeconds <= 0);
            bool allowStale = (maximumStaleSeconds != 0);
            if (!forceRefresh)
            {
                // use the passed in time to override the default
                forceRefresh = timeFilter.RefreshRequired(key, maximumStaleSeconds);
            }
            return await dataCache.WithRefreshAsync<T>(requestToken, key, allowStale, forceRefresh, onRefreshed, onRefreshing, createMethod);
        }
        public static async Task<bool> WithTimedRefreshAsync<T>(this IDataCache dataCache, string key, int maximumStaleSeconds, FetchedDelegate<T> onRefreshed, Action<bool> onRefreshing, Func<Task<T>> createMethod)
            where T : class
        {
            TimedDataCacheFilter timeFilter = EnsureTimedLifetimeFilter(dataCache);
            bool forceRefresh = (maximumStaleSeconds <= 0);
            bool allowStale = (maximumStaleSeconds != 0);
            if (!forceRefresh)
            {
                // use the passed in time to override the default
                forceRefresh = timeFilter.RefreshRequired(key, maximumStaleSeconds);
            }
            return await dataCache.WithRefreshAsync<T>(key, allowStale, forceRefresh, onRefreshed, onRefreshing, createMethod);
        }

        /// <summary>
        /// Refreshes if the prefix has timed out and/or the localkey has timed out.
        /// Flushes all items if the time out has occurred
        /// Returns true if data was retrieved
        /// </summary>
        /// </summary>
        public static async Task<bool> WithTimedRefreshForPrefixAsync<T>(this IDataCache dataCache, bool allowStale, string prefixKey, string localKey, int maximumStaleSeconds, FetchedDelegate<T> onRefreshed, Action<bool> onRefreshing, Func<Task<T>> createMethod)
            where T : class
        {
            TimedDataCacheFilter timeFilter = EnsureTimedLifetimeFilter(dataCache);
            bool forceRefresh = (maximumStaleSeconds <= 0);
            if (!forceRefresh)
            {
                // use the passed in time to override the default
                forceRefresh = timeFilter.RefreshRequired(prefixKey, maximumStaleSeconds);
            }

            if (forceRefresh)
            {
                timeFilter.AddClearBeforeSave(localKey, prefixKey);
            }

            FetchedDelegate<T> onRefreshedWrapper = (freshData, data) =>
            {
                if (freshData)
                {
                    dataCache.AddToCache(prefixKey, data);
                    timeFilter.OnAfterItemSavedToCache(dataCache, prefixKey, data);
                }
                if (onRefreshed != null)
                {
                    onRefreshed(freshData, data);
                }
            };

            return await dataCache.WithRefreshAsync<T>(prefixKey + localKey, allowStale, forceRefresh, onRefreshedWrapper, onRefreshing, createMethod);
        }
        /// <summary>
        /// Refreshes if the prefix has timed out and/or the localkey has timed out.
        /// Flushes all items if the time out has occurred
        /// Returns true if data was retrieved
        /// </summary>
        /// </summary>
        public static async Task<bool> WithTimedRefreshForPrefixAsync<T>(this IDataCache dataCache, RequestToken requestToken, bool allowStale, string prefixKey, string localKey, int maximumStaleSeconds, FetchedRequestDelegate<T> onRefreshed, Action<bool> onRefreshing, Func<Task<T>> createMethod)
            where T : class
        {
            TimedDataCacheFilter timeFilter = EnsureTimedLifetimeFilter(dataCache);
            bool forceRefresh = (maximumStaleSeconds <= 0);
            if (!forceRefresh)
            {
                // use the passed in time to override the default
                forceRefresh = timeFilter.RefreshRequired(prefixKey, maximumStaleSeconds);
            }

            if (forceRefresh)
            {
                timeFilter.AddClearBeforeSave(localKey, prefixKey);
            }

            FetchedRequestDelegate<T> onRefreshedWrapper = (token, freshData, data) =>
            {
                if (freshData)
                {
                    dataCache.AddToCache(prefixKey, data);
                    timeFilter.OnAfterItemSavedToCache(dataCache, prefixKey, data);
                }
                if (onRefreshed != null)
                {
                    onRefreshed(token, freshData, data);
                }
            };

            return await dataCache.WithRefreshAsync<T>(requestToken, prefixKey + localKey, allowStale, forceRefresh, onRefreshedWrapper, onRefreshing, createMethod);
        }

        private static TimedDataCacheFilter EnsureTimedLifetimeFilter(IDataCache dataCache)
        {
            // Super Safe Reader/Writer lock
            TimedDataCacheFilter filter = null;
            if (dataCache.Filters.ContainsKey(_timedCacheKey))
            {
                filter = dataCache.Filters[_timedCacheKey] as TimedDataCacheFilter;
            }
            if (filter == null)
            {
                lock (_timedSyncLock)
                {
                    if (dataCache.Filters.ContainsKey(_timedCacheKey))
                    {
                        filter = dataCache.Filters[_timedCacheKey] as TimedDataCacheFilter;
                    }
                    if (filter == null)
                    {
                        filter = new TimedDataCacheFilter();
                        dataCache.Filters[_timedCacheKey] = filter;
                    }
                }
            }
            return filter;
        }
    }
}
