using System;
using Android.Support.V7.Widget;
using Android.Content;
using Android.Views;
using Stencil.Native.Droid;
using Stencil.Native.Core;
using Stencil.SDK.Models;

namespace Stencil.Native.Droid
{
    public class CoreRecyclerArrayAdapter<TCell, TData> : RecyclerView.Adapter
        where TCell : CoreViewHolderCell<TData>, new()
        where TData : IItemID
    {
        public CoreRecyclerArrayAdapter(object owner, Context context, int viewResourceID, TData[] items)
        {
            this.Owner = owner;
            this.Context = context;
            this.ResourceID = viewResourceID;
            this.Items = items;
            this.HasStableIds = true;
        }

        public SwipeRevealLayoutViewBinderHelper ViewBinderHelper { get; set; }

        public void IntializeRevealLayout(bool singleOpen = true)
        {
            if (this.ViewBinderHelper == null)
            {
                this.ViewBinderHelper = new SwipeRevealLayoutViewBinderHelper();
            }
            this.ViewBinderHelper.setOpenOnlyOne(singleOpen);
        }

        public object Owner { get; set; }
        public Context Context { get; set; }
        public int ResourceID { get; set; }

        /// <summary>
        /// Adapter Position
        /// </summary>
        public Action<int> OnItemClick { get; set; }
        public TData[] Items { get; protected set; }
        public override int ItemCount 
        {
            get 
            {
                if (this.Items == null)
                {
                    return 0;
                }
                return Items.Length;
            }
        }
        public override long GetItemId(int position)
        {
            return this.Items[position].item_id;
        }
        public void ReplaceData(TData[] data)
        {
            if (this.ViewBinderHelper != null)
            {
                bool onlyOnce = this.ViewBinderHelper.OpenOnlyOnce; // silly way
                this.ViewBinderHelper = new SwipeRevealLayoutViewBinderHelper();
                this.ViewBinderHelper.setOpenOnlyOne(onlyOnce);
            }
            this.Items = data;
        }
        
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            return CoreUtility.ExecuteFunction("OnCreateViewHolder", delegate()
            {
                View itemView = LayoutInflater.From(parent.Context).Inflate(this.ResourceID, parent, false);

                return new CoreViewHolder<TCell, TData>(itemView);
            });
        } 

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            CoreUtility.ExecuteMethod("OnBindViewHolder", delegate()
            {
                CoreViewHolder<TCell, TData> holder = viewHolder as CoreViewHolder<TCell, TData>;

                if (this.ViewBinderHelper != null)
                {
                    this.ViewBinderHelper.bind(holder.Cell.View as SwipeRevealLayout, holder.ItemId.ToString());
                }

                holder.Cell.BindData(this.Owner, this.Context, this.Items[position]);
            });
        }

    }
}

