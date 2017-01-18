using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Native.Caching
{
    public interface ICacheHost
    {
        T CachedUserGet<T>() where T : class;
        void CachedUserSet<T>(T user) where T : class;
        void CachedUserClear();

        void CachedDataClear();
        bool CachedDataSet<T>(bool secure, string fileName, T item);
        T CachedDataGet<T>(bool secured, string fileName);

        /// <summary>
        /// Stores data that is not typically removed [app configs, network configs, serial #s, etc]
        /// </summary>
        bool PersistentDataSet<T>(bool secure, string fileName, T item);
        /// <summary>
        /// Gets data that is not typically changed/removed [app configs, network configs, serial #s, etc]
        /// </summary>
        T PersistentDataGet<T>(bool secured, string fileName);
    }
}
