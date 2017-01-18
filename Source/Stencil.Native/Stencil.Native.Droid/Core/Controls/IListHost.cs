using System;

namespace Stencil.Native.Droid
{
    public interface IListHost<TData>
    {
        void OnListItemClicked(TData data);
    }
}

