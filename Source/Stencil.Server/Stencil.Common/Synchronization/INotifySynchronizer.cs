using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Common.Synchronization
{
    public interface INotifySynchronizer
    {
        /// <summary>
        /// When an inline sync fails
        /// </summary>
        void OnSyncFailed(string type, string argument);

        /// <summary>
        /// When an inline sync times out or invalidates peacefully
        /// </summary>
        void SyncEntity(string entityType);

        /// <summary>
        /// Agitates specific daemon
        /// </summary>
        void AgitateDaemon(string name);

        /// <summary>
        /// Agitates all daemons
        /// </summary>
        void AgitateSyncDaemon();
    }
}
