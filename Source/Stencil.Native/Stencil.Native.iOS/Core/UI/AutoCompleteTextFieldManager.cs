using System;
using System.Collections.Generic;
using UIKit;
using Stencil.SDK;
using System.Threading.Tasks;
using Foundation;
using CoreGraphics;
using Stencil.Native.Core;

namespace Stencil.Native.iOS.Core.UI
{
    /// <summary>
    /// Quick and dirty. Use Caution
    /// </summary>
    public static class AutoCompleteTextFieldManager
    {
        private static List<IAutoCompleteTextField> autoCompleteTextFields;

        public static IDisposable Add<TItem>(UIView targetView, UITextField textView, Action<TItem> onSelected, Func<string, Task<List<TItem>>> findElements, Func<TItem, string> getItemNameMethod, Func<TItem, string> getItemValueMethod, Func<string, TItem> createEmptyItem)
        {
            if(autoCompleteTextFields == null)
            {
                autoCompleteTextFields = new List<IAutoCompleteTextField>();
            }
            AutoCompleteTextField<TItem> result = new AutoCompleteTextField<TItem>(targetView, textView, onSelected, findElements, getItemNameMethod, getItemNameMethod, createEmptyItem);
            autoCompleteTextFields.Add(result);
            return result;
        }
        private static void Remove(IAutoCompleteTextField field)
        {
            if (autoCompleteTextFields != null)
            {
                autoCompleteTextFields.Remove(field);
            }
        }
        public static void RemoveAll()
        {
            if (autoCompleteTextFields != null)
            {
                autoCompleteTextFields.Clear();
            }
        }

        private interface IAutoCompleteTextField : IDisposable
        {
        }
        private class AutoCompleteTextField<TItem> : BaseClass, IAutoCompleteTextField
        {
            public AutoCompleteTextField(UIView targetView, UITextField textView, Action<TItem> onSelected, Func<string, Task<List<TItem>>> findElements, Func<TItem, string> getItemNameMethod, Func<TItem, string> getItemValueMethod, Func<string, TItem> createEmptyItem)
                : base("AutoCompleteTextField")
            {
                this.GetItemNameMethod = getItemNameMethod;
                this.GetItemValueMethod = getItemValueMethod;
                this.onSelected = onSelected;
                this.targetView = targetView;
                this.textField = textView;
                this.selectedIndex = -1;
                this.findElements = findElements;
                this.GetEmptySearchItemMethod = createEmptyItem;
                this.matchedElements = new List<TItem>();

                CGPoint position = targetView.ConvertPointFromView(new CoreGraphics.CGPoint(0,0), textField);

                this.autoCompleteTableView = new UITableView(new CoreGraphics.CGRect(0, position.Y + this.textField.Frame.Height, targetView.Frame.Width, targetView.Frame.Height - position.Y - this.textField.Frame.Height));
                this.autoCompleteTableViewSource = new AutoCompleteTableViewSource<TItem>(this.matchedElements, this.OnSelectedElement, getItemNameMethod);
                this.autoCompleteTableView.ReloadData();
                this.autoCompleteTableView.ScrollEnabled = true;
                this.autoCompleteTableView.Hidden = true;
                this.autoCompleteTableView.TableFooterView = new UIView();
                //TODO:SHOULD:Styling: AutoComplete background color: 
                //this.autoCompleteTableView.BackgroundColor = CoreAssumptions.COLOR_GREY_MID.ConvertHexToColor();
                targetView.AddSubview(this.autoCompleteTableView);

                this.textField.EditingChanged -= textField_EditingChanged;
                this.textField.EditingChanged += textField_EditingChanged;
                this.textField.EditingDidEnd -= this.textField_EditingDidEnd;
                this.textField.EditingDidEnd += this.textField_EditingDidEnd;
                this.textField.EditingDidBegin -= this.textField_EditingDidBegin;
                this.textField.EditingDidBegin += this.textField_EditingDidBegin;
            }
            private UIView targetView;
            private UITextField textField;
            private int selectedIndex = -1;
            private Func<string, Task<List<TItem>>> findElements;
            private List<TItem> matchedElements;
            private UITableView autoCompleteTableView;
            private UITableViewSource autoCompleteTableViewSource;
            private Action<TItem> onSelected;
            private Func<TItem, string> GetItemNameMethod;
            private Func<TItem, string> GetItemValueMethod;
            private Func<string, TItem> GetEmptySearchItemMethod;

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    AutoCompleteTextFieldManager.Remove(this);
                    if (this.textField != null)
                    {
                        this.textField.EditingDidEnd -= this.textField_EditingDidEnd;
                        this.textField.EditingDidBegin -= this.textField_EditingDidBegin;
                        this.textField.EditingChanged -= this.textField_EditingChanged;
                    }
                }
                base.Dispose(disposing);
            }

            private void OnSelectedElement(nint index)
            {
                base.ExecuteMethod("OnSelectedElement", delegate()
                {
                    this.selectedIndex = (int)index;
                    TItem selectedElement = this.matchedElements[(int) this.selectedIndex];
                    if (selectedElement != null && !string.IsNullOrEmpty(this.GetItemValueMethod(selectedElement)))
                    {
                        this.textField.Text = this.GetItemNameMethod(selectedElement);
                        this.autoCompleteTableView.Hidden = true;
                        if (onSelected != null)
                        {
                            onSelected(selectedElement);
                        }
                    }
                });
            }
            private async void textField_EditingChanged(object sender, EventArgs args)
            {
                await base.ExecuteMethodAsync("textField_EditingChanged", async delegate()
                {
                    string search = this.textField.Text;
                    this.selectedIndex = -1;
                    CGPoint position = targetView.ConvertPointFromView(new CoreGraphics.CGPoint(0,0), textField);
                    this.autoCompleteTableView.Frame = new CoreGraphics.CGRect(0, position.Y + this.textField.Frame.Height, targetView.Frame.Width, targetView.Frame.Height - position.Y - this.textField.Frame.Height);
                    this.autoCompleteTableView.Hidden = false;
                    this.matchedElements = await findElements(search);
                    if (this.matchedElements.Count == 0)
                    {
                        this.matchedElements.Add(this.GetEmptySearchItemMethod(search));
                    }
                    this.autoCompleteTableViewSource = new AutoCompleteTableViewSource<TItem>(this.matchedElements, this.OnSelectedElement, this.GetItemNameMethod);
                    this.autoCompleteTableView.Source = this.autoCompleteTableViewSource;
                    this.autoCompleteTableView.ReloadData();
                });
            }
            private void textField_EditingDidEnd(object sender, EventArgs args)
            {
                base.ExecuteMethod("textField_EditingDidEnd", delegate()
                {
                    this.autoCompleteTableView.Hidden = true;
                });
            }
            private void textField_EditingDidBegin(object sender, EventArgs args)
            {
                base.ExecuteMethod("textField_EditingDidBegin", delegate()
                {
                    UIScrollView scroll = targetView.Superview as UIScrollView;
                    if(scroll != null)
                    {
                        CGPoint point = targetView.ConvertPointFromView(new CGPoint(0,0), textField);
                        scroll.SetContentOffset(new CGPoint(0, point.Y), true);
                    }

                });
            }
        }

        private class AutoCompleteTableViewSource<TItem> : UITableViewSource
        {
            public AutoCompleteTableViewSource (List<TItem> elements, Action<nint> selectedElementCallback, Func<TItem, string> getItemNameMethod)
            {
                this.elements = elements;
                this.selectedElementCallback = selectedElementCallback;
                this.GetItemNameMethod = getItemNameMethod;
            }

            private List<TItem> elements;
            private Action<nint> selectedElementCallback;
            private Func<TItem, string> GetItemNameMethod;

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return CoreUtility.ExecuteFunction("RowsInSection", delegate()
                {
                    return elements.Count;
                });
            }

            public override UITableViewCell GetCell(UITableView tableView, Foundation.NSIndexPath indexPath)
            {
                return CoreUtility.ExecuteFunction("GetCell", delegate()
                {
                    TItem element = this.elements[indexPath.Row];

                    UITableViewCell cell = new UITableViewCell();
                    cell.TextLabel.Text = this.GetItemNameMethod(element);
                    cell.TextLabel.TextColor = UIColor.DarkGray;

                    return cell;
                });
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                CoreUtility.ExecuteMethod("RowSelected", delegate()
                {
                    if (selectedElementCallback != null)
                    {
                        selectedElementCallback(indexPath.Row);
                    }
                });

            }
        }
    }
}

