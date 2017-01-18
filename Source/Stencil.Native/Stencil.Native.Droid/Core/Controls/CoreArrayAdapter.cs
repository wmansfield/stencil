using System;
using Android.Widget;
using Android.Content;
using System.Collections.Generic;
using Android.Views;
using Android.App;
using Android.Support.V7.App;
using Stencil.SDK.Models;

namespace Stencil.Native.Droid
{
    
    public class CoreArrayAdapter<TData, TCell> : CoreArrayAdapter<TData>
        where TCell : CoreCell<TData>, new()
    {
        public CoreArrayAdapter(object owner, Activity activity, List<TData> data)
            : base(owner, activity, data, null, null)
        {
            this.CreateCellMethod = this.CreateCell;
            this.GetCellTypeMethod = this.GetCellType;
        }

        public virtual CoreCell<TData> CreateCell(int position, Android.Views.View convertView, Android.Views.ViewGroup parent)
        {
            return new TCell();
        }
        public virtual Type GetCellType(int position)
        {
            return typeof(TCell);
        }
    }

    public class CoreArrayAdapter<TData> : ArrayAdapter<TData>
    {
        public CoreArrayAdapter(object owner, Activity activity, List<TData> data, Func<int, View, ViewGroup, CoreCell<TData>> createCellMethod, Func<int, Type> cellTypeMethod)
            : base(activity, Android.Resource.Layout.SimpleListItem1, new List<TData>())
        {
            this.Owner = owner;
            this.CreateCellMethod = createCellMethod;
            this.GetCellTypeMethod = cellTypeMethod;
            this.ReplaceData(data);
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

        public void ReplaceData(List<TData> data)
        {
            base.Clear();

            if (this.ViewBinderHelper != null)
            {
                bool onlyOnce = this.ViewBinderHelper.OpenOnlyOnce; // silly way
                this.ViewBinderHelper = new SwipeRevealLayoutViewBinderHelper();
                this.ViewBinderHelper.setOpenOnlyOne(onlyOnce);
            }

            base.AddAll(data.ToArray());
        }
        public void AppendData(List<TData> data)
        {
            base.AddAll(data.ToArray());
        }

        public Func<int, View, ViewGroup, CoreCell<TData>> CreateCellMethod { get; set; }
        public Func<int, Type> GetCellTypeMethod { get; set; }
        public override int Count
        {
            get
            {
                try
                {
                    return base.Count;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public void ExpandToWrapContent(ListView listView, SDK.Models.SDKModel sdkModel)
        {
            int totalHeight = 0;

            if (sdkModel.TagExists("ExpandToWrapContent"))
            {
                totalHeight = sdkModel.TagGetAsInt("ExpandToWrapContent", 100);
            }
            else
            {
                int total = this.Count;

                for (int i = 0; i < total; i++)
                {
                    View item = this.GetView(i, null, listView);
                    item.Measure(0, 0);
                    totalHeight += item.MeasuredHeight;
                }

                totalHeight += listView.DividerHeight * (total - 1);

                sdkModel.TagSet("ExpandToWrapContent", totalHeight.ToString());
            }
            ViewGroup.LayoutParams param = listView.LayoutParameters;
            param.Height = totalHeight;
            listView.LayoutParameters  = param;
            listView.RequestLayout();
        }

        public override Android.Views.View GetView(int position, Android.Views.View convertView, Android.Views.ViewGroup parent)
        {
            if (this.CreateCellMethod == null)
            {
                return base.GetView(position, convertView, parent); // must be configured wrong
            }
            else
            {
                TData item = (TData)this.GetItem(position);
                View view = convertView;
                object rawCell = null;
                if (view != null)
                {
                    rawCell = view.GetTag(Resource.String.cell_tag);
                }
                CoreCell<TData> cell = rawCell as CoreCell<TData>;
                if (cell != null && rawCell.GetType() != GetCellTypeMethod(position))
                {
                    // make it create a new one, can't re-use
                    view = null;
                    cell = null;
                }
                if (view == null)
                {
                    cell = this.CreateCellMethod(position, convertView, parent);
                    cell.View = (this.Context as AppCompatActivity).LayoutInflater.Inflate(cell.ResourceID, null);
                    cell.OnViewCreated(cell.View);
                    view = cell.View;
                    view.SetTag(Resource.String.cell_tag, cell);
                }

                cell = view.GetTag(Resource.String.cell_tag) as CoreCell<TData>;
                if (cell == null) // jic ref is gone?
                {
                    cell = this.CreateCellMethod(position, convertView, parent);
                    cell.View = view;
                    view.SetTag(Resource.String.cell_tag, cell);
                }

                if (this.ViewBinderHelper != null)
                {
                    long id = position;
                    IItemID itemID = item as IItemID;
                    if(itemID != null)
                    {
                        id = itemID.item_id;
                    }
                    this.ViewBinderHelper.bind(cell.View as SwipeRevealLayout, id.ToString());
                }

                cell.BindCellData(this.Owner, this.Context, item);
                return view;
            }
        }

    }
}

