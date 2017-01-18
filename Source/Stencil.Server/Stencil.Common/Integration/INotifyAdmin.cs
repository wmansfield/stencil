using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Common.Integration
{
    public interface INotifyAdmin
    {
        void SendAdminEmail(string subject, string body);
    }
}
