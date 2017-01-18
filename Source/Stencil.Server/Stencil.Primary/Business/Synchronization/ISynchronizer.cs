using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Synchronization
{
    public interface ISynchronizer
    {
        /// <summary>
        /// Used to notify health system which entity this synchronizer references
        /// </summary>
        string EntityName { get; }

        void RequestSynchronization();

        /// <summary>
        /// Executes a synchronization of items that are out of date
        /// </summary>
        /// <returns>The amount of records that were out of date</returns>
        [Obsolete("Only daemons should use this method", false)]
        int PerformSynchronization(string requestedAgentName);

        int Priority { get; }

    }
}
