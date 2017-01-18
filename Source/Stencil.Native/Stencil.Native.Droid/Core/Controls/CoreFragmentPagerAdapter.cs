using System;
using System.Linq;
using Android.Support.V4.App;
using Java.Lang;
using Stencil.Native.Droid.Core;

namespace Stencil.Native.Droid
{
    public class CoreFragmentPagerAdapter : FragmentPagerAdapter, INamePageAdapter, ICustomWidthAdapter
    {
        public CoreFragmentPagerAdapter(global::Android.Support.V4.App.FragmentManager fragmentManager, BaseFragment[] fragments, string[] titles)
            : base(fragmentManager)
        {
            _fragmentManager = fragmentManager;
            _loadedFragments = fragments;
            _titles = titles;
        }

        private global::Android.Support.V4.App.FragmentManager _fragmentManager;
        private BaseFragment[] _loadedFragments;
        private string[] _titles;
        public Func<int, ICharSequence> GetPageTitleFormattedMethod { get; set; }
        public Func<int, ICharSequence> GetPageNameFormattedMethod { get; set; }
        public Func<int, int> GetCustomTabWidthMethod { get; set; }


        public override int GetItemPosition(Java.Lang.Object @object)
        {
            if (_loadedFragments.Contains(@object))
            {
                return PositionUnchanged;
            }
            return PositionNone;
        }
        public override Java.Lang.ICharSequence GetPageTitleFormatted(int position)
        {
            if (this.GetPageTitleFormattedMethod != null)
            {
                return this.GetPageTitleFormattedMethod(position);
            }
            return new Java.Lang.String(_titles[position]);
        }
        public ICharSequence GetNameFormatted(int position)
        {
            if (this.GetPageNameFormattedMethod != null)
            {
                return this.GetPageNameFormattedMethod(position);
            }
            return new Java.Lang.String("");
        }

        public override global::Android.Support.V4.App.Fragment GetItem(int position)
        {
            return _loadedFragments[position];
        }

        public void LoadFragment(int index, BaseFragment fragment, string title)
        {
            _loadedFragments[index] = fragment;
            if (_titles != null && _titles.Length > index)
            {
                _titles[index] = title;
            }
            base.NotifyDataSetChanged();
        }

        public override int Count
        {
            get { 
                return _loadedFragments.Length; 
            }
        }

        public virtual void SetPageTitle(int index, string newTitle)
        {
            if(_titles != null && _titles.Length > index)
            {
                _titles[index] = newTitle;
            }
        }

        public virtual int GetTabWidth(int index)
        {
            if(this.GetCustomTabWidthMethod != null)
            {
                return this.GetCustomTabWidthMethod(index);
            }
            return 0;
        }
    }
}

