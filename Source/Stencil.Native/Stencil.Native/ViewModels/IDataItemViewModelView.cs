using System;
using System.Collections.Generic;
using System.Text;

namespace Stencil.Native.ViewModels
{
    public interface IDataItemViewModelView<T> : IViewModelView
    {
        void BindData(bool live_data, T data);
        void OnDataRefreshing(bool refreshing);
    }
}
