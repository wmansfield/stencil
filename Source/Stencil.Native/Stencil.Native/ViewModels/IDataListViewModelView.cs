using System;
using System.Collections.Generic;
using System.Text;

namespace Stencil.Native.ViewModels
{
    public interface IDataListViewModelView<T> : IViewModelView
    {
        void BindData(bool live_data, List<T> data);
        void AddData(List<T> data);
        void OnDataRefreshing(bool refreshing);
        void OnDataGettingMore(bool gettingMore);
    }
}
