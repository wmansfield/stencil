using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Common.Integration
{
    public interface INotifyEncoder
    {
        void OnVideoAdded();
        void OnPhotoAdded();
    }
}
