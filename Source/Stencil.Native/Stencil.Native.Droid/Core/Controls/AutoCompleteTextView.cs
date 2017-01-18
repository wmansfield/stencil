using System;
using Android.Widget;
using System.ComponentModel;
using Android.Content;
using Android.Util;
using Android.Content.Res;
using Android.Graphics;
using Stencil.Native.Droid.Core;

namespace Stencil.Native.Droid
{
    public class AutoCompleteTextView<TItem> : AutoCompleteTextView, INotifyPropertyChanged
    {
        public AutoCompleteTextView(Context context) 
            : this(context, null)
        {
        }

        public AutoCompleteTextView(Context context, IAttributeSet attrs) 
            : this(context, attrs, Android.Resource.Attribute.AutoCompleteTextViewStyle)
        {
        }

        public AutoCompleteTextView(Context context, IAttributeSet attrs, int defStyle) 
            : base(context, attrs, defStyle)
        {

            // note - we shouldn't realy need both of these... but we do
            this.ItemClick += OnItemClick;
            this.ItemSelected += OnItemSelected;


            TypedArray array = context.ObtainStyledAttributes(attrs, Resource.Styleable.CustomFont, defStyle, 0);
            this.CustomFontPath = array.GetString(Resource.Styleable.CustomFont_customFont);
            array.Recycle();

            SetCustomFont(this.CustomFontPath);

        }
        public string CustomFontPath { get; set; }

        public void SetCustomFont(string assetPath)
        {
            try
            {
                if(!string.IsNullOrEmpty(assetPath))
                {
                    Typeface face = FontLoader.GetFont(Context.Assets, assetPath);
                    if (face != null)
                    {
                        TypefaceStyle style = TypefaceStyle.Normal;
                        if (this.Typeface != null) //Takes care of android:textStyle=""
                        {    
                            style = Typeface.Style;
                        }
                        this.SetTypeface(face, style);
                    }
                }
            }
            catch(Exception ex)
            {
                Stencil.Native.Core.Container.Track.LogWarning(ex.Message, "AutoCompleteTextView");
            }
        }

        private TItem _selectedItem;

        public TItem SelectedItem
        {
            get { return _selectedItem; }
            private set
            {
                _selectedItem = value;
                RaiseSelectedObjectChanged();
            }
        }

        public new AutoCompleteAdapter<TItem> Adapter
        {
            get { return base.Adapter as AutoCompleteAdapter<TItem>; }
            set
            {
                base.Adapter = value;
            }
        }

        protected void OnItemClick(object sender, AdapterView.ItemClickEventArgs itemClickEventArgs)
        {
            OnItemClick(itemClickEventArgs.Position);
        }

        protected void OnItemSelected(object sender, AdapterView.ItemSelectedEventArgs itemSelectedEventArgs)
        {
            OnItemSelected(itemSelectedEventArgs.Position);
        }
        protected virtual void OnItemClick(int position)
        {
            var selectedObject = Adapter.GetItem(position);
            SelectedItem = selectedObject;
        }

        protected virtual void OnItemSelected(int position)
        {
            var selectedObject = Adapter.GetItem(position);
            SelectedItem = selectedObject;
        }

        public event EventHandler SelectedObjectChanged;

        protected void RaiseSelectedObjectChanged()
        {
            var handler = SelectedObjectChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

