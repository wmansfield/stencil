using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Native.Caching
{
    public interface IDataCacheFilter
    {
        bool RefreshRequired(IDataCache dataCache, string key);

        bool CanReadCached(IDataCache dataCache, string key);
        bool CanSaveToCache(IDataCache dataCache, string key, object data);

        void OnBeforeItemReadFromCache(IDataCache dataCache, string key);
        void OnAfterItemReadFromCache(IDataCache dataCache, string key, object data);

        void OnBeforeItemRetrieved(IDataCache dataCache, string key);
        void OnAfterItemRetrieved(IDataCache dataCache, string key, object data);

        void OnBeforeItemSavedToCache(IDataCache dataCache, string key, object data);
        void OnAfterItemSavedToCache(IDataCache dataCache, string key, object data);

    }
}
