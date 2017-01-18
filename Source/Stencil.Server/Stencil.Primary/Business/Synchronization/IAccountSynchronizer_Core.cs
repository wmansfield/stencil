using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Synchronization
{
    public partial interface IAccountSynchronizer : ISynchronizer
    {
        void SynchronizeItem(Guid primaryKey, Availability availability);
    }
}

