using System;
using UIKit;
using CoreGraphics;
using Stencil.SDK;
using Stencil.Native.Core;

namespace Stencil.Native.iOS.Core.UI
{
    public class CustomPicker : UIView
    {
        #region Constructors

        public CustomPicker(UIView superView)
        {
            //this.Title = "";
            //_superView = superView;
            _superView = UIApplication.SharedApplication.KeyWindow;
            this.Frame = new CGRect(0,0, _superView.Frame.Width, _superView.Frame.Height);
            this.BackgroundColor = UIColor.FromWhiteAlpha(0f, .5f);
            this.Opaque = false;
        }

        #endregion

        #region Private Properties

        private UIView _superView;
        private UIView _triggerSource;
        private Action<IDPair> _onDoneClicked;
        private IDPair[] _data;

        private UIPickerView _pickerView;
        private UIView _vwBtnContainer;     
        private string _titleForDoneButton = "DONE";
        private UIColor _colorOfSelectedItem = UIColor.White;

        private string _initialValue;
        private bool _hasLoaded;
        private CGRect _screenRect
        { 
            get{ return UIScreen.MainScreen.ApplicationFrame; }
        }
        private CGRect _actionSheetFrame  
        {
            get { return _screenRect; }
        }
        public CustomPickerViewModel Data
        {
            get
            {
                return _source;
            }
        }


        #endregion


        #region Protected Methods


        protected void OnPickerItemSelected(UIPickerView picker, nint row, nint component, IDPair selectedItem)
        {
            CoreUtility.ExecuteMethod("OnPickerItemSelected", delegate() 
            {
                UIView selectedView = picker.ViewFor (row, component);
                selectedView.BackgroundColor = _colorOfSelectedItem;
            });
        }

        #endregion

        #region Public Methods

        public void ShowPicker()
        {
            CoreUtility.ExecuteMethod("ShowPicker", delegate() 
            {
                this.BeginInvokeOnMainThread(delegate() 
                {
                    _superView.EndEditing(true);
                    _superView.AddSubview(this);
                    _superView.BringSubviewToFront(this);
                });

            });
        }
        private CustomPickerViewModel _source;
        public CustomPicker CreateCustomPicker()
        {
            return CoreUtility.ExecuteFunction<CustomPicker> ("CreateCustomPicker", delegate() 
            {
                if(_superView != null)
                {
                    //create picker view
                    UIPickerView pickerView = new UIPickerView()
                    {
                        ShowSelectionIndicator = true,
                        Hidden = false,
                        Frame = new CGRect(0, (_superView.Frame.Height - 216f), _superView.Frame.Width, 216f),
                        Opaque = true,
                        BackgroundColor = "#D1D5DB".ConvertHexToColor()
                    };

                    _source = new CustomPickerViewModel()
                        .For(_data)
                        .WhenSelected(this.OnPickerItemSelected);

                    pickerView.Model = _source;

                    UIView selectedItem = pickerView.ViewFor(pickerView.SelectedRowInComponent(0),0);
                    if(selectedItem != null)
                    {
                        selectedItem.BackgroundColor = _colorOfSelectedItem;
                    }

                    _pickerView = pickerView;

                    //create grey container for done button
                    UIView vwBtnContainer = new UIView(new CGRect(0f, (_superView.Bounds.Height - 216f) - 40f, _superView.Bounds.Width, 40f));
                    vwBtnContainer.BackgroundColor = UIColor.DarkGray;
                    _vwBtnContainer = vwBtnContainer;


                    //create Done button
                    UIButton btnDone = new UIButton(UIButtonType.Custom)
                    {
                        Frame = new CGRect(_superView.Frame.Width - 60f, 5f, 50f, 30f),
                        Hidden = false
                    };
                    btnDone.Font = UIFont.BoldSystemFontOfSize(14f);
                    btnDone.BackgroundColor = UIColor.Clear;
                    btnDone.SetTitleColor(UIColor.White, UIControlState.Normal);
                    btnDone.SetTitle(_titleForDoneButton, UIControlState.Normal);
                    btnDone.TouchDown += (sender, e) => 
                    {
                        CoreUtility.ExecuteMethod("TouchDown", delegate() 
                        {
                            if(this._onDoneClicked != null)
                            {
                                nint selectedRow = pickerView.SelectedRowInComponent(0);
                                this._onDoneClicked(_data[selectedRow]);
                            }
                            this.RemoveFromSuperview();
                        });
                    };
                    vwBtnContainer.Add(btnDone);

                    if(_triggerSource != null)
                    {
                        if(_triggerSource is UITextField)
                        {
                            (_triggerSource as UITextField).ShouldBeginEditing = ShowPickerFromTextField;
                        }
                        else
                        {
                            UITapGestureRecognizer tapGesture = new UITapGestureRecognizer(this.ShowPicker);
                            _triggerSource.AddGestureRecognizer(tapGesture);
                        }
                    }

                    this.AddSubview(_pickerView);
                    this.AddSubview(_vwBtnContainer);

                    if(!string.IsNullOrEmpty(_initialValue))
                    {
                        if(_data != null)
                        {
                            for (int i = 0; i < _data.Length; i++) 
                            {
                                if(_data[i].id == _initialValue)
                                {
                                    _pickerView.Select(i, 0, false);
                                    break;
                                }
                            }
                        }
                    }
                }
                return this;
            });
        }

        private bool ShowPickerFromTextField (UITextField textField)
        {
            this.ShowPicker();
            return false;
        }

        public CustomPicker TriggerOnTapGestureOf(UIView tapSource)
        {
            this._triggerSource = tapSource;
            return this;
        }
        public CustomPicker WithDataSource(IDPair[] data)
        {
            this._data = data;
            return this;
        }
        public CustomPicker WhenDoneClicked(Action<IDPair> onDoneClicked)
        {
            this._onDoneClicked = onDoneClicked;
            return this;
        }
        public CustomPicker WithInitialValue(string textValue)
        {
            this._initialValue = textValue;
            return this;
        }
        public CustomPicker WithBackgroundColor(UIColor color)
        {
            this.BackgroundColor = color;
            return this;
        }

        #endregion
    }
}

