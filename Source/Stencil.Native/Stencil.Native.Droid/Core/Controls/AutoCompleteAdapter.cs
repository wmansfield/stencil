using System;
using Android.Widget;
using Android.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stencil.Native.Droid
{
    public class AutoCompleteAdapter<TItem> : ArrayAdapter<TItem>, IFilterable
    {
        public AutoCompleteAdapter(Context context, int textViewResourceId,  Func<string, Task<List<TItem>>> searchMethod)
            : this(context, textViewResourceId, new List<TItem>(), searchMethod)
        {
        }
        public AutoCompleteAdapter(Context context, int textViewResourceId, List<TItem> dataReference, Func<string, Task<List<TItem>>> searchMethod)
            : base(context, textViewResourceId)
        {
            _filter = new AutoCompleteFilter<TItem>(this, searchMethod);
        }

        private AutoCompleteFilter<TItem> _filter;

        public override Filter Filter
        {
            get
            {
                return _filter;
            }
        }
    }
}

