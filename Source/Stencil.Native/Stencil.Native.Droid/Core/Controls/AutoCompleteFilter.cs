using System;
using Android.Widget;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Stencil.Native.Droid
{
    public class AutoCompleteFilter<TItem> : Filter
    {
        public AutoCompleteFilter(AutoCompleteAdapter<TItem> adapter, Func<string, Task<List<TItem>>> searchMethod)
        {
            this.Adapter = adapter;
            this.SearchMethod = searchMethod;
        }

        public Func<string, Task<List<TItem>>> SearchMethod { get; set; }
        public AutoCompleteAdapter<TItem> Adapter { get; set; }

        public List<TItem> RecentData { get; set; }


        protected override void PublishResults(Java.Lang.ICharSequence constraint, FilterResults results)
        {
            if (RecentData != null)
            { 
                int count = this.Adapter.Count;
                for (int i = count - 1; i >= 0; i--)
                {
                    this.Adapter.Remove(this.Adapter.GetItem(i));
                }
                foreach (var item in RecentData)
                {
                    this.Adapter.Add(item);
                }
            }
            if ((results != null) && (results.Count > 0)) 
            { 
                this.Adapter.NotifyDataSetChanged(); 
            } 
            else 
            {
                this.Adapter.NotifyDataSetInvalidated(); 
            } 
        }
        protected override FilterResults PerformFiltering(Java.Lang.ICharSequence constraint)
        {
            FilterResults filterResults = new FilterResults();
            if (constraint != null)
            {
                var task = this.SearchMethod(constraint.ToString());
                Task.WaitAll(task);

                List<TItem> result = task.Result;
                this.RecentData = result;
                filterResults.Count = result.Count;
            }
            return filterResults;
        }

    }
}

