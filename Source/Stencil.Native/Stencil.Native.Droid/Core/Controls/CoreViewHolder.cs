using System;
using Android.Views;
using Android.Support.V7.Widget;

namespace Stencil.Native.Droid
{
    public class CoreViewHolder<TCell, TData> : RecyclerView.ViewHolder 
        where TCell : CoreViewHolderCell<TData>, new()
    {
        public CoreViewHolder(View itemView) 
            : base(itemView)
        {
            this.Cell = new TCell();
            this.Cell.View = itemView;
            this.Cell.OnViewCreated(this.Cell.View);
        }

        public TCell Cell { get; set; }
    }
}

