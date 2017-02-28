using System;
using Android.Content;
using Android.Widget;
using Stencil.SDK.Models;

namespace Stencil.Native.Droid
{
    public class CellPostText : CoreCell<Post>
    {
        public CellPostText()
            : base("CellPostText")
        {
        }
        public override int ResourceID
        {
            get
            {
                return Resource.Layout.CellPostText;
            }
        }

        protected TextView lblText { get { return this.GetControl<TextView>(Resource.Id.general_text); } }

        private bool _initialized;
        private void EnsureInitialized()
        {
            base.ExecuteMethod("EnsureInitialized", delegate ()
            {
                if(_initialized) { return; }

                _initialized = true;

            });
        }

        public override void BindData(object owner, Context context, Post data)
        {
            base.ExecuteMethod("BindData", delegate ()
            {
                this.EnsureInitialized();
                this.Data = data;

                lblText.Text = data.body;
            });
        }

    }
}

