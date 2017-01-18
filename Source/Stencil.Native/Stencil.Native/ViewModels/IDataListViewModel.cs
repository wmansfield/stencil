using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Native.ViewModels
{
    public interface IDataListViewModel
    {
        bool HasMoreData { get; }
        int ScrollThresholdCount { get; }
        Task DoGetMoreData();
    }
}
