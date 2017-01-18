using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Stencil.Native.Core;

namespace Stencil.Native.Droid
{
    public static class _DictionaryHelper
    {
        public static void put<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey id, TValue value)
        {
            dictionary[id] = value;
        }
        public static TValue get<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey id)
        {
            TValue value = default(TValue);
            dictionary.TryGetValue(id, out value);
            return value;
        }
        public static void removeAllWithValue<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TValue value)
            where TValue : class
        {
            List<TKey> keys = new List<TKey>();
            foreach (var item in dictionary)
            {
                if(item.Value == value)
                {
                    keys.Add(item.Key);
                }
            }
            foreach (var item in keys)
            {
                TValue ignore = default(TValue);
                dictionary.TryRemove(item, out ignore);
            }
        }
    }
    public class SwipeRevealLayoutViewBinderHelper : BaseClass
    {
        public SwipeRevealLayoutViewBinderHelper()
            : base("SwipeRevealLayoutViewBinderHelper")
        {

        }
        private const string BUNDLE_MAP_KEY = "ViewBinderHelper_Bundle_Map_Key";

        private ConcurrentDictionary<string, int> mapStates = new ConcurrentDictionary<string, int>();
        private ConcurrentDictionary<String, SwipeRevealLayout> mapLayouts = new ConcurrentDictionary<String, SwipeRevealLayout>();
        private ConcurrentDictionary<string, byte> lockedSwipeSet = new ConcurrentDictionary<string, byte>();

        private volatile bool openOnlyOne = false;
        public bool OpenOnlyOnce
        {
            get
            {
                return this.openOnlyOne;
            }
        }
        private static object stateChangeLock = new object();

        /**
         * Help to save and restore open/close state of the swipeLayout. Call this method
         * when you bind your view holder with the data object.
         *
         * @param swipeLayout swipeLayout of the current view.
         * @param id a string that uniquely defines the data object of the current view.
         */
        public void bind(SwipeRevealLayout swipeLayout, string id)
        {
            base.ExecuteMethod("bind", delegate ()
            {
                if (swipeLayout.shouldRequestLayout())
                {
                    swipeLayout.RequestLayout();
                }

                mapLayouts.removeAllWithValue(swipeLayout);
                mapLayouts.put(id, swipeLayout);

                swipeLayout.abort();

                swipeLayout.setDragStateChangeListener(new CustomListener(this, swipeLayout, id));


                // first time binding.
                if (!mapStates.ContainsKey(id))
                {
                    mapStates.put(id, SwipeRevealLayout.STATE_CLOSE);
                    swipeLayout.close(false);
                }

                // not the first time, then close or open depends on the current state.
                else
                {
                    int state = mapStates.get(id);

                    if (state == SwipeRevealLayout.STATE_CLOSE || state == SwipeRevealLayout.STATE_CLOSING ||
                            state == SwipeRevealLayout.STATE_DRAGGING)
                    {
                        swipeLayout.close(false);
                    }
                    else
                    {
                        swipeLayout.open(false);
                    }
                }

                // set lock swipe
                swipeLayout.setLockDrag(lockedSwipeSet.ContainsKey(id));
            });
        }

        private class CustomListener : SwipeRevealLayout.DragStateChangeListener
        {
            public CustomListener(SwipeRevealLayoutViewBinderHelper helper, SwipeRevealLayout swipeLayout, string id)
            {
                _swipeLayout = swipeLayout;
                _helper = helper;
                _id = id;
            }
            private SwipeRevealLayout _swipeLayout;
            private SwipeRevealLayoutViewBinderHelper _helper;
            private string _id;

            public void onDragStateChanged(int state)
            {
                _helper.mapStates.put(_id, state);

                if (_helper.openOnlyOne)
                {
                    _helper.closeOthers(_id, _swipeLayout);
                }
            }
        }

        /**
         * Only if you need to restore open/close state when the orientation is changed.
         * Call this method in {@link android.app.Activity#onSaveInstanceState(Bundle)}
         */
        public void saveStates(Bundle outState)
        {
            if (outState == null)
                return;

            Bundle statesBundle = new Bundle();
            foreach (var item in mapStates)
            {
                statesBundle.PutInt(item.Key, item.Value);
            }

            outState.PutBundle(BUNDLE_MAP_KEY, statesBundle);
        }


        /**
         * Only if you need to restore open/close state when the orientation is changed.
         * Call this method in {@link android.app.Activity#onRestoreInstanceState(Bundle)}
         */
        public void restoreStates(Bundle inState)
        {
            if (inState == null)
                return;

            if (inState.ContainsKey(BUNDLE_MAP_KEY))
            {
                ConcurrentDictionary<string, int> restoredMap = new ConcurrentDictionary<string, int>();

                Bundle statesBundle = inState.GetBundle(BUNDLE_MAP_KEY);
                ICollection<String> keySet = statesBundle.KeySet();

                if (keySet != null)
                {
                    foreach (var key in keySet)
                    {
                        restoredMap.put(key, statesBundle.GetInt(key));
                    }
                }

                mapStates = restoredMap;
            }
        }

        /**
        * Lock swipe for some layouts.
        * @param id a string that uniquely defines the data object.
        */
        public void lockSwipe(params string[] id)
        {
            setLockSwipe(true, id);
        }

        /**
        * Unlock swipe for some layouts.
        * @param id a string that uniquely defines the data object.
        */
        public void unlockSwipe(params string[] id)
        {
            setLockSwipe(false, id);
        }

        /**
        * @param openOnlyOne If set to true, then only one row can be opened at a time.
        */
        public void setOpenOnlyOne(bool openOnlyOne)
        {
            this.openOnlyOne = openOnlyOne;
        }

        /**
        * Open a specific layout.
        * @param id unique id which identifies the data object which is bind to the layout.
        */
        public void openLayout(string id)
        {
            lock(stateChangeLock)
            {
                mapStates.put(id, SwipeRevealLayout.STATE_OPEN);

                SwipeRevealLayout layout = null;
                if (mapLayouts.TryGetValue(id, out layout) && layout != null)
                {
                    layout.open(true);
                }
                else if (openOnlyOne)
                {
                    closeOthers(id, mapLayouts.get(id));
                }
            }
        }

        /**
            * Close a specific layout.
            * @param id unique id which identifies the data object which is bind to the layout.
            */
        public void closeLayout(string id)
        {
            lock(stateChangeLock)
            {
                mapStates.put(id, SwipeRevealLayout.STATE_CLOSE);

                SwipeRevealLayout layout = null;
                if (mapLayouts.TryGetValue(id, out layout) && layout != null)
                {
                    layout.close(true);
                }
            }
        }

        /**
         * Close others swipe layout.
         * @param id layout which bind with this data object id will be excluded.
         * @param swipeLayout will be excluded.
         */
        private void closeOthers(String id, SwipeRevealLayout swipeLayout)
        {
            lock(stateChangeLock)
            {
                // close other rows if openOnlyOne is true.
                if (getOpenCount() > 1)
                {
                    var keys = mapStates.Keys.ToList();
                    foreach (var key in keys)
                    {
                        if(key != id)
                        {
                            mapStates[key] = SwipeRevealLayout.STATE_CLOSE;
                        }
                    }
                    
                    foreach (SwipeRevealLayout layout in mapLayouts.Values)
                    {
                        if (layout != swipeLayout)
                        {
                            layout.close(true);
                        }
                    }
                }
            }
        }

        private void setLockSwipe(bool lockSwipe, params string[] ids)
        {
            if (ids == null || ids.Length == 0)
                return;

            if (lockSwipe)
            {
                foreach (var item in ids)
                {
                    lockedSwipeSet.TryAdd(item, 0);
                }
            }
            else
            {
                foreach (var item in ids)
                {
                    byte ignore = 0;
                    lockedSwipeSet.TryRemove(item, out ignore);
                }
            }

            foreach (string item in ids)
            {
                SwipeRevealLayout layout = mapLayouts.get(item);
                if (layout != null)
                {
                    layout.setLockDrag(lockSwipe);
                }
            }
        }

        private int getOpenCount()
        {
            int total = 0;
            foreach (var item in mapStates.Values)
            {
                if (item == SwipeRevealLayout.STATE_OPEN || item == SwipeRevealLayout.STATE_OPENING)
                {
                    total++;
                }
            }

            return total;
        }
    }
}