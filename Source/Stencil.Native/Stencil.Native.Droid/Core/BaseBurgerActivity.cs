using System;
using Android.Views;
using Android.Content.Res;
using Android.Support.V4.Widget;
using Android.OS;
using System.Collections.Generic;
using Android.Text;
using Android.Support.V4.Content;
using Android.Content;
using System.Threading.Tasks;
using Android.Widget;
using Stencil.Native.Droid.Core.UI;
using Stencil.Native.Core;

namespace Stencil.Native.Droid.Core
{
    public abstract class BaseBurgerActivity : BaseFragmentActivity, IMultiViewHost, DrawerLayout.IDrawerListener
    {
        #region Constructor

        public BaseBurgerActivity()
            : this("BaseBurgerActivity")
        {
            this.VisibleFragments = new Dictionary<int, BaseFragment>();
        }
        public BaseBurgerActivity(string trackPrefix)
            : base(trackPrefix)
        {
            this.VisibleFragments = new Dictionary<int, BaseFragment>();
        }

        #endregion

        #region Properties

        public DrawerLayout Drawer { get; set; }
        private int _viewIDLeft;
        private int _viewIDRight;
        private int _viewIDCenter;
        public string PrimaryTitle { get; set; }

        protected virtual BurgerActionBarDrawerToggle DrawerToggleLeft { get; set; }

        protected virtual bool HasOnCreateCompleted { get; set; }
        public virtual bool SyncTitleWithFragmentTitleDisabled { get; set; }
        public Dictionary<int, BaseFragment> VisibleFragments { get; set; }

        #endregion

        #region Events & Invokers

        public event EventHandler<LayoutZoneChangedArgs> LayoutZoneChanged;
        protected void RaiseLayoutZoneChanged(LayoutZone zone, BaseFragment fragment)
        {
            var handler = this.LayoutZoneChanged;
            if (handler != null)
            {
                handler(this, new LayoutZoneChangedArgs(){ Zone = zone, Host = this, Fragment = fragment });
            }
        }

        #endregion

        #region Overrides

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.ExecuteMethod("OnPostCreate", delegate()
            {
                this.HasOnCreateCompleted = true;
                base.OnPostCreate(savedInstanceState);
                if (this.DrawerToggleLeft != null)
                {
                    this.DrawerToggleLeft.SyncState();
                }
            });
        }
        protected override void OnResume()
        {
            base.ExecuteMethod("OnResume", delegate()
            {
                base.OnResume();

                this.StencilApp.ViewPlatform.RecentMultiViewHost = this;
            });
        }

        protected override void OnPause()
        {
            base.ExecuteMethod("OnPause", delegate ()
            {
                if(this.StencilApp.ViewPlatform.RecentMultiViewHost == this)
                {
                    this.StencilApp.ViewPlatform.RecentMultiViewHost = null;
                }
                base.OnPause();

            });
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.ExecuteMethod("OnConfigurationChanged", delegate()
            {
                base.OnConfigurationChanged(newConfig);
                if (this.DrawerToggleLeft != null)
                {
                    this.DrawerToggleLeft.OnConfigurationChanged(newConfig);
                }

            });
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            return base.ExecuteFunction("OnOptionsItemSelected", delegate()
            {
                if ((this.DrawerToggleLeft != null) && this.DrawerToggleLeft.OnOptionsItemSelected(item))
                {
                    return true;
                }
                return base.OnOptionsItemSelected(item);
            });
        }


        #endregion

        #region Protected Methods

        protected virtual void InitializeBurgerMenu(DrawerLayout drawer, Android.Support.V7.Widget.Toolbar toolbar, int leftViewID, int centerViewID, int rightViewID = 0)
        {
            base.ExecuteMethod("InitializeBurgerMenu", delegate()
            {
                this.Drawer = drawer;
                _viewIDLeft = leftViewID;
                _viewIDCenter = centerViewID;
                _viewIDRight = rightViewID;

                this.Drawer.SetDrawerShadow(Resource.Drawable.drawer_shadow_dark, (int)GravityFlags.Start);
                this.Drawer.SetDrawerShadow(Resource.Drawable.drawer_shadow_dark_invert, (int)GravityFlags.End);

                if(this.SupportActionBar != null)
                {
                    this.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    this.SupportActionBar.SetHomeButtonEnabled(true);
                }
                if(rightViewID == 0) // we're using normal drawers
                {
                    this.DrawerToggleLeft = new BurgerActionBarDrawerToggle(this, 
                        drawer,
                        toolbar,
                        Resource.String.drawer_open,
                        Resource.String.drawer_close);
                }
                else if(leftViewID != 0)
                {
                    if(this.SupportActionBar != null)
                    {
                        TextDrawable burger = new TextDrawable(this);
                        burger.Text = "";// fa-bars
                        burger.TextSize = 15;
                        burger.SetCustomFont(NativeAssumptions.FONT_AWESOME);
                        burger.TextAlign = Layout.Alignment.AlignCenter;
                        this.SupportActionBar.SetHomeAsUpIndicator(burger);
                    }
                }
                else
                {
                    // not typical at all, let the caller figure it out
                }
                drawer.SetDrawerListener(this);
            });
        }

        protected virtual bool FragmentAllowsBackButton(BaseFragment fragment, string tag)
        {
            return base.ExecuteFunction("FragmentAllowsBackButton", delegate()
            {
                if (!this.HasOnCreateCompleted)
                {
                    return false;
                }
                else
                {
                    if (fragment != null)
                    {
                        switch (fragment.LayoutZone)
                        {
                            case LayoutZone.PrimaryContent:
                                return true;
                            case LayoutZone.PrimaryMenu:
                            case LayoutZone.SecondaryMenu:
                            default:
                                return false;
                        }
                    }
                    return false;
                }
            });
        }


        /// <summary>
        /// Gets the target view id from the requested data
        /// </summary>
        protected virtual int GetContainerViewId(BaseFragment fragment, string tag)
        {
            return base.ExecuteFunction("GetContainerViewId", delegate()
            {
                if (fragment != null)
                {
                    switch (fragment.LayoutZone)
                    {
                        case LayoutZone.PrimaryContent:
                            return _viewIDCenter;
                        case LayoutZone.PrimaryMenu:
                            return _viewIDLeft;
                        case LayoutZone.SecondaryMenu:
                            return _viewIDRight;
                        default:
                            break;
                    }
                }
                return _viewIDCenter;
            });
        }

        /// <summary>
        /// Extensibility point to allow augmentation when showing a child view
        /// </summary>
        protected virtual void OnFragmentViewShown(BaseFragment fragment, int viewIdOrIndex)
        {
            base.ExecuteMethod("OnFragmentViewShown", delegate()
            {
                
                if (fragment != null)
                {
                    switch (fragment.LayoutZone)
                    {
                        case LayoutZone.PrimaryMenu:
                        case LayoutZone.SecondaryMenu:
                            break;
                        default:
                            this.PrimaryTitle = fragment.Title;
                            break;
                    }
                    if(this.VisibleFragments.ContainsKey(viewIdOrIndex))
                    {
                        BaseFragment existing = this.VisibleFragments[viewIdOrIndex] ;
                        if(existing != fragment)
                        {
                            this.OnFragmentViewRemoved(fragment, viewIdOrIndex);
                        }
                    }
                    this.VisibleFragments[viewIdOrIndex] = fragment;
                    this.RaiseLayoutZoneChanged(fragment.LayoutZone, fragment);
                    fragment.OnAddedToMultiHost(this, fragment.LayoutZone);
                }
                if (!SyncTitleWithFragmentTitleDisabled)
                {
                    this.SupportActionBar.Title = OnActivityTitleUpdating(this.PrimaryTitle, fragment);
                }
                this.Drawer.CloseDrawers();

            });
        }
        /// <summary>
        /// Extensibility point to allow augmentation when showing a child view
        /// </summary>
        protected virtual void OnFragmentViewRemoved(BaseFragment fragment, int viewIdOrIndex)
        {
        }

        /// <summary>
        /// Extensibility point
        /// </summary>
        protected virtual string OnActivityTitleUpdating(string newTitle, BaseFragment fragment)
        {
            return newTitle;
        }

        #endregion

        #region Public Methods

        public abstract void ShowMain();

        public virtual bool Show(BaseFragment fragment, string tag = "")
        {
            return base.ExecuteFunction("Show", delegate()
            {
                int viewID = GetContainerViewId(fragment, tag);
                if (viewID == 0)
                {
                    throw new Exception(string.Format("View type did not return a proper view id for {0}", tag));
                }

                var transaction = this.SupportFragmentManager.BeginTransaction();
                transaction.Replace(viewID, fragment, tag);
                if (FragmentAllowsBackButton(fragment, tag))
                {
                    transaction.AddToBackStack(null);
                }
                transaction.Commit();
                this.OnFragmentViewShown(fragment, viewID);
                return true;
            });

        }

        #endregion

        #region Drawer Listener

        public virtual void OnDrawerClosed(View drawerView)
        {
            base.ExecuteMethod("OnDrawerClosed", delegate()
            {
                this.InvalidateOptionsMenu();
                if (this.DrawerToggleLeft != null)
                {
                    this.DrawerToggleLeft.OnDrawerClosed(drawerView);
                }
            });
        }

        public virtual void OnDrawerOpened(View drawerView)
        {
            base.ExecuteMethod("OnDrawerOpened", delegate()
            {
                this.InvalidateOptionsMenu();
                if (this.DrawerToggleLeft != null)
                {
                    this.DrawerToggleLeft.OnDrawerOpened(drawerView);
                }
            });
        }

        public virtual void OnDrawerSlide(View drawerView, float slideOffset)
        {
            base.ExecuteMethod("OnDrawerSlide", delegate()
            {
                if (this.DrawerToggleLeft != null)
                {
                    this.DrawerToggleLeft.OnDrawerSlide(drawerView, slideOffset);
                }
            });
        }

        public virtual void OnDrawerStateChanged(int newState)
        {
            base.ExecuteMethod("OnDrawerStateChanged", delegate()
            {
                if (this.DrawerToggleLeft != null)
                {
                    this.DrawerToggleLeft.OnDrawerStateChanged(newState);
                }
            });
        }

        #endregion
    }
}

