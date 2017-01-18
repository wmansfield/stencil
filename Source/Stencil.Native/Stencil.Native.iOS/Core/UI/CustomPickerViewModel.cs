using System;
using UIKit;
using Stencil.SDK;
using CoreGraphics;
using Stencil.Native.Core;

namespace Stencil.Native.iOS.Core.UI
{
    public class CustomPickerViewModel : UIPickerViewModel
    {
        #region Constructors

        public CustomPickerViewModel ()
        {
        }

        public CustomPickerViewModel(IDPair[] dataSource, Action<UIPickerView, nint, nint, IDPair> onSelected, Func<UIPickerView, nint, nint, string> titleCreator, Func<UIPickerView,nint,nfloat> rowSizer)
        {
            _dataSource = dataSource;
            _onSelected = onSelected;
            _customRowSizer = rowSizer;
            _customTitleCreator = titleCreator;
        }

        #endregion

        #region Private Properties

        private IDPair[] _dataSource = null;
        private Action<UIPickerView, nint, nint, IDPair> _onSelected = null;
        private Func<UIPickerView, nint, nint, string> _customTitleCreator = null;
        private Func<UIPickerView, nint, nfloat> _customRowSizer = null;

        #endregion

        #region Overrides

        public override nint GetComponentCount (UIPickerView picker)
        {
            return CoreUtility.ExecuteFunction<int> ("GetComponentCount", delegate() 
            {
                return 1;
            });
        }

        public override string GetTitle (UIPickerView picker, nint row, nint component)
        {
            return CoreUtility.ExecuteFunction<string> ("GetTitle", delegate() 
            {
                string title = string.Empty;
                if(_customTitleCreator != null)
                {
                    title = _customTitleCreator(picker, row, component);
                }
                if(string.IsNullOrEmpty(title) && _dataSource != null)
                {
                    title = _dataSource[row].name;
                }
                return title;
            });
        }

        public override nint GetRowsInComponent (UIPickerView picker, nint component)
        {
            return CoreUtility.ExecuteFunction<int> ("GetRowsInComponent", delegate() 
            {
                if(_dataSource!=null)
                {
                    return _dataSource.Length;
                }
                return 0;
            });
        }
        public override nfloat GetRowHeight (UIPickerView picker, nint component)
        {
            return CoreUtility.ExecuteFunction<nfloat> ("GetRowHeight", delegate() 
            {
                if(_customRowSizer!=null)
                {
                    return _customRowSizer(picker, component);
                }
                return 40f;
            });
        }

        public override UIView GetView (UIPickerView picker, nint row, nint component, UIView view)
        {
            return CoreUtility.ExecuteFunction<UIView> ("GetView", delegate() 
            {
                float labelOffset = 30f;
                UIView vwPickerRow = new UIView(new CGRect(0f, 0f, picker.RowSizeForComponent(component).Width, picker.RowSizeForComponent(component).Height));
                UILabel lblPickerText = new UILabel(new CGRect(labelOffset, 0f, picker.RowSizeForComponent(component).Width - labelOffset, 40f));
                lblPickerText.Text = _dataSource[row].name;
                vwPickerRow.AddSubview(lblPickerText);
                vwPickerRow.BackgroundColor = UIColor.White;

                return vwPickerRow;
            });
        }

        public override void Selected (UIPickerView picker, nint row, nint component)
        {
            CoreUtility.ExecuteMethod ("Selected", delegate() 
            {
                if(_onSelected != null)
                {
                    IDPair selectedItem = null;
                    if(_dataSource!=null)
                    {
                        selectedItem = _dataSource[row];
                    }

                    _onSelected(picker,row,component,selectedItem);
                }
            });
        }

        #endregion

        #region Public Methods

        public CustomPickerViewModel For(IDPair[] model)
        {
            _dataSource = model;
            return this;
        }
        public CustomPickerViewModel WhenCreatingRowTitle(Func<UIPickerView, nint, nint, string> titleCreator)
        {
            _customTitleCreator = titleCreator;
            return this;
        }
        public CustomPickerViewModel WhenSizingRows(Func<UIPickerView,nint,nfloat> rowSizer)
        {
            _customRowSizer = rowSizer;
            return this;
        }
        public CustomPickerViewModel WhenSelected(Action<UIPickerView, nint, nint, IDPair> onSelected)
        {
            _onSelected = onSelected;
            return this;
        }

        #endregion
    }
}

