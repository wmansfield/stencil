using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Native.Caching
{
    public class TimedDataCacheFilter : IDataCacheFilter
    {
        #region Constructor
        public TimedDataCacheFilter()
        {
            this.TimingData = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
            this.ClearPrefixes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        #endregion

        #region Protected Statics
        protected static object _TimingWriteLock = new object();

        #endregion

        #region Public Properties

        public Dictionary<string, DateTime> TimingData { get; set; }
        public virtual int DefaultMaxiumumStaleSeconds { get; set; }
        public Dictionary<string, string> ClearPrefixes { get; set; }

        #endregion

        #region Public Methods

        public void ClearWithPrefix(string prefix)
        {
            lock (_TimingWriteLock)
            {
                string[] keys = this.TimingData.Keys.ToArray();
                foreach (string key in keys)
                {
                    if (key.StartsWith(prefix))
                    {
                        this.TimingData.Remove(key);
                    }
                }
            }
        }
        public void ClearAll()
        {
            lock (_TimingWriteLock)
            {
                this.TimingData.Clear();
            }
        }
        public void AddClearBeforeSave(string localKey, string prefixKey)
        {
            this.ClearPrefixes[localKey] = prefixKey;
        }

        public virtual bool RefreshRequired(IDataCache dataCache, string key)
        {
            return RefreshRequired(key, this.DefaultMaxiumumStaleSeconds);
        }
        public virtual bool RefreshRequired(string key, int maximumStaleSeconds)
        {
            bool refreshRequired = false;
            if (maximumStaleSeconds > 0)
            {
                if (this.TimingData.ContainsKey(key))
                {
                    DateTime stamp = TimingData[key];
                    refreshRequired = (maximumStaleSeconds < (DateTime.UtcNow - stamp).TotalSeconds);
                }
                else
                {
                    refreshRequired = true;
                }
            }
            else
            {
                // don't set required here, we use our wrapper to do it, this is embedded, dont want the datacache to use directly
            }
            return refreshRequired;
        }

        public bool CanReadCached(IDataCache dataCache, string key)
        {
            return true;
        }
        public bool CanSaveToCache(IDataCache dataCache, string key, object data)
        {
            return true;
        }

        public void OnBeforeItemReadFromCache(IDataCache dataCache, string key)
        {

        }
        public void OnAfterItemReadFromCache(IDataCache dataCache, string key, object data)
        {

        }

        public void OnBeforeItemRetrieved(IDataCache dataCache, string key)
        {

        }
        public void OnAfterItemRetrieved(IDataCache dataCache, string key, object data)
        {
        }

        public void OnAfterItemSavedToCache(IDataCache dataCache, string key, object data)
        {
            lock (_TimingWriteLock)
            {
                TimingData[key] = DateTime.UtcNow;
            }
        }
        public void OnBeforeItemSavedToCache(IDataCache dataCache, string key, object data)
        {
            if (this.ClearPrefixes.ContainsKey(key))
            {
                string prefix = this.ClearPrefixes[key];
                this.ClearPrefixes.Remove(key);
                dataCache.ClearWithPrefix(prefix);
            }
        }
        #endregion


       
    }
}
