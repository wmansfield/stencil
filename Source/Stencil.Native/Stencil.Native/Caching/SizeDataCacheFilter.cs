using System;
using System.Collections.Generic;
using System.Linq;

namespace Stencil.Native.Caching
{
    public class SizeDataCacheFilter : IDataCacheFilter
    {
        public SizeDataCacheFilter(int maxDataLength, int maxKeyCount, int removeKeyBufferCount)
        {
            this.MaximumDataLength = maxDataLength;
            this.MaximumKeyCount = maxKeyCount;
            this.RemoveKeyBufferCount = removeKeyBufferCount;
        }

        public const string NAME = "SizeDataCacheFilter";

        public virtual int MaximumDataLength { get; set; }
        public virtual int MaximumKeyCount { get; set; }
        public virtual int RemoveKeyBufferCount { get; set; }
        private bool _direction;

        public virtual bool RefreshRequired(IDataCache dataCache, string key)
        {
            return false;
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

        public void OnBeforeItemSavedToCache(IDataCache dataCache, string key, object data)
        {
            // EXPECTS PROPERLY FORMATED CACHE KEYS
            try
            {
                List<string> orderedKeys = dataCache.CacheKeys.OrderBy(x => x).ToList();

#if DEBUG
                /*
                Container.Track.LogTrace(string.Format("<Cache Details: {0} - {1}>", dataCache.CacheKeys.Length, dataCache.CacheCount));
                foreach(var item in orderedKeys)
                {
                    Container.Track.LogTrace(string.Format("     {0}", item));
                }
                Container.Track.LogTrace(string.Format("</Cache Details>"));
                */
#endif

                if(dataCache.CacheCount > this.MaximumKeyCount)
                {
                    // simple strategy to try to trim from both directions
                    _direction = !_direction;
                    if(!_direction)
                    {
                        orderedKeys.Reverse();
                    }

                    string[] keys = dataCache.CacheKeys;
                    int keyIndex = -1;
                    for(int i = 0; i < this.RemoveKeyBufferCount; i++)
                    {
                        if(orderedKeys.Count > 0)
                        {
                            string itemToRemove = string.Empty;

                            while(keyIndex < orderedKeys.Count)
                            {
                                keyIndex++;
                                string item = orderedKeys[keyIndex];
                                if(!item.Contains("?") && !key.Contains(item) && key != item)
                                {
                                    itemToRemove = item;
                                    break;
                                }
                            }

                            if(!string.IsNullOrEmpty(itemToRemove))
                            {
                                //Container.Track.LogTrace(string.Format(" ----> Removed From Cache: {0}", itemToRemove));
                                dataCache.ClearWithPrefix(itemToRemove);
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }
        public void OnAfterItemSavedToCache(IDataCache dataCache, string key, object data)
        {

        }

    }
}
