using System;
using UIKit;
using System.Collections.Generic;
using Foundation;
using System.Linq;
using Stencil.Native.Core;

namespace Stencil.Native.iOS.Core.Data
{
    public class CoreTableSource<TItem> : UITableViewSource, ICoreScrollView
    {
        public CoreTableSource()
        {
            this.CachedCustomViews = new Dictionary<string, List<UIView>>();
            this.DefaultSectionCount = 1;
        }
        public CoreTableSource(string defaultCellIdentifier)
            : this()
        {
            this.DefaultCellIdentifier = defaultCellIdentifier;
        }
        public CoreTableSource(IEnumerable<TItem> items, string defaultCellIdentifier, Action<TItem, UITableViewCell> onSelectedMethod, Action<UITableViewCell, NSIndexPath, TItem> bindCellMethod)
            : this()
        {
            this.Items = new List<TItem>();
            foreach (var item in items)
            {
                this.Items.Add(item);
            }
            this.DefaultCellIdentifier = defaultCellIdentifier;
            this.OnSelectedMethod = onSelectedMethod;
            this.BindCellMethod = bindCellMethod;
        }

        public Dictionary<string, List<UIView>> CachedCustomViews { get; set; }
        public virtual List<TItem> Items { get; set; }
        public virtual string DefaultCellIdentifier { get; set; }
        public virtual bool DisableRowDeselection { get; set; }
        public virtual bool CreateSectionPerRow { get; set; }
        public virtual int DefaultSectionCount  { get; set; }
        public virtual nfloat DefaultHeaderSize  { get; set; }
        public virtual nfloat DefaultFooterSize  { get; set; }

        public virtual IScrollListener ScrollListener { get; set; }
        public virtual Action<TItem, UITableViewCell> OnSelectedMethod { get; set; }
        public virtual Action<NSIndexPath, UITableViewCell> OnSelectedMethodRaw { get; set; }
        public virtual Action<UITableViewCell, NSIndexPath, TItem> BindCellMethod { get; set; }
        public virtual Func<TItem, NSIndexPath, nfloat> RowSizeMethod { get; set; }
        public virtual Func<NSIndexPath, nfloat> RowSizeMethodRaw { get; set; }
        public virtual Func<nint> CountSectionsMethod { get; set; }
        public virtual Func<nint, nint> CountRowsInSectionMethod { get; set; }
        public virtual Func<TItem, NSIndexPath, UITableViewCell> CreateCellMethod { get; set; }
        public virtual Func<CoreTableSource<TItem>, nint, UIView> CreateHeaderMethod { get; set; }
        public virtual Func<NSIndexPath, UITableViewCell> CreateCellRawMethod { get; set; }
        public virtual Func<nint, nfloat> HeaderSizeMethod { get; set; }
        public virtual Func<nint, nfloat> FooterSizeMethod { get; set; }

        public Func<NSIndexPath, bool> CanDeleteRowMethod { get; set; }
        public Action<NSIndexPath> RowDeleteMethod { get; set; }
        public Func<NSIndexPath, string> RowDeleteTextMethod { get; set; }

        public Func<NSIndexPath, UITableViewRowAction[]> CustomRowActionsMethod { get; set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.OnSelectedMethod = null;
                this.BindCellMethod = null;
                this.Items.Clear();
                this.Items = null;
                this.RowSizeMethod = null;
                this.RowSizeMethodRaw = null;
                this.CountSectionsMethod = null;
                this.CreateCellMethod = null;
                this.CreateCellRawMethod = null;
                this.ScrollListener = null;
                this.HeaderSizeMethod = null;
                this.FooterSizeMethod = null;

                this.CanDeleteRowMethod = null;
                this.RowDeleteMethod = null;
                this.RowDeleteTextMethod = null;

                this.CustomRowActionsMethod = null;

                if(this.CachedCustomViews != null)
                {
                    foreach(var list in CachedCustomViews.Values)
                    {
                        foreach(var item in list)
                        {
                            item.Dispose();
                        }
                        list.Clear();
                    }
                    this.CachedCustomViews.Clear();
                    this.CachedCustomViews = null;
                }
            }
            base.Dispose(disposing);
        }

        public virtual TItem GetItem(int index)
        {
            return CoreUtility.ExecuteFunction<TItem>("GetItem", delegate()
            {
                if (this.Items != null && this.Items.Count >= index)
                {
                    return this.Items[index];
                }
                return default(TItem);
            });
        }
        public virtual void AppendItems(IEnumerable<TItem> items)
        {
            CoreUtility.ExecuteMethod("AppendItems", delegate()
            {
                foreach (var item in items)
                {
                    this.Items.Add(item);
                }
            });
        }
        public virtual void ClearItems()
        {
            CoreUtility.ExecuteMethod("ClearItems", delegate()
            {
                if(this.Items == null) { return; }
                this.Items.Clear();
            });
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            CoreUtility.ExecuteMethod("RowSelected", delegate()
            {
                UITableViewCell cell = tableView.CellAt(indexPath);
                if(this.OnSelectedMethodRaw != null)
                {
                    if(!this.DisableRowDeselection)
                    {
                        tableView.DeselectRow(indexPath, true); // iOS convention is to remove the highlight
                    }
                    if (this.OnSelectedMethodRaw != null)
                    {
                        this.OnSelectedMethodRaw(indexPath, cell);
                    }
                }
                else
                {
                    TItem info = this.GetItem(indexPath.Row);
                    if(!this.DisableRowDeselection)
                    {
                        tableView.DeselectRow(indexPath, true); // iOS convention is to remove the highlight
                    }
                    if (this.OnSelectedMethod != null)
                    {
                        this.OnSelectedMethod(info, cell);
                    }
                }
            });
        }
        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return CoreUtility.ExecuteFunction<nfloat>("GetHeightForRow", delegate() 
            {
                if(this.RowSizeMethodRaw != null)
                {
                    return this.RowSizeMethodRaw(indexPath);
                }
                if(this.RowSizeMethod != null)
                {
                    TItem item = default(TItem);
                    if(CreateSectionPerRow)
                    {
                        item = this.Items[indexPath.Section];
                    }
                    else
                    {
                        item = this.Items[indexPath.Row];
                    }
                    return this.RowSizeMethod(item, indexPath);
                }
                return tableView.RowHeight;
            });
        }

        public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return CoreUtility.ExecuteFunction("EditingStyleForRow", delegate()
            {
                if(this.CanDeleteRowMethod != null && this.CanDeleteRowMethod(indexPath))
                {
                    return UITableViewCellEditingStyle.Delete;
                }
                return UITableViewCellEditingStyle.None;
            });
        }
        public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return CoreUtility.ExecuteFunction("UITableViewCellEditingStyle", delegate ()
            {
                if(this.CustomRowActionsMethod != null)
                {
                    return this.CustomRowActionsMethod(indexPath);
                }
                return null;
            });
        }
        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            return CoreUtility.ExecuteFunction("CanEditRow", delegate()
            {
                if(this.CanDeleteRowMethod != null)
                {
                    return this.CanDeleteRowMethod(indexPath);
                }
                return false;
            });
        }
        public override string TitleForDeleteConfirmation(UITableView tableView, NSIndexPath indexPath)
        {
            return CoreUtility.ExecuteFunction("TitleForDeleteConfirmation", delegate()
            {
                if(RowDeleteTextMethod != null)
                {
                    string text = RowDeleteTextMethod(indexPath);
                    if(!string.IsNullOrEmpty(text))
                    {
                        return text;
                    }
                }

                return "Delete";
            });
        }
        public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
        {
            CoreUtility.ExecuteMethod("CommitEditingStyle", delegate()
            {
                if(editingStyle == UITableViewCellEditingStyle.Delete)
                {
                    if(RowDeleteMethod != null)
                    {
                        RowDeleteMethod(indexPath);
                    }
                }
            });
        }
        public override nint NumberOfSections(UITableView tableView)
        {
            if (CreateSectionPerRow)
            {
                if (this.Items != null)
                {
                    return this.Items.Count;
                }
            }
            if (CountSectionsMethod != null)
            {
                return CountSectionsMethod();
            }
            return this.DefaultSectionCount;
        }
        public override nint RowsInSection(UITableView tableview, nint section)
        {
            if (CreateSectionPerRow)
            {
                if (this.Items != null && this.Items.Count > 0)
                {
                    return 1;
                }
            }
            else
            {
                if (CountRowsInSectionMethod != null)
                {
                    return CountRowsInSectionMethod(section);
                }
                else if (this.Items != null)
                {
                    return this.Items.Count;
                }
            }
            return 0;
        }
        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            return CoreUtility.ExecuteFunction<UITableViewCell>("GetCell", delegate() 
            {
                try 
                {
                    if(this.CreateCellRawMethod != null)
                    {
                        return this.CreateCellRawMethod(indexPath);
                    }

                    UITableViewCell cell = null;
                    TItem item = default(TItem);

                    if(this.CreateSectionPerRow)
                    {
                        item = this.Items[indexPath.Section];
                    }
                    else
                    {
                        item = this.Items[indexPath.Row];
                    }
                    if(this.CreateCellMethod != null)
                    {
                        cell = this.CreateCellMethod(item, indexPath);
                    }
                    if(cell == null)
                    {
                        cell = tableView.DequeueReusableCell(this.DefaultCellIdentifier);
                    }
                    if(cell == null)
                    {
                        cell = new UITableViewCell();// wowsers
                    }
                    if(this.BindCellMethod == null && this.CreateCellMethod == null)
                    {
                        cell.TextLabel.Text = item.ToString();
                    }
                    else
                    {
                        if(this.BindCellMethod != null)
                        {
                            this.BindCellMethod(cell, indexPath, item);
                        }
                    }
                    return cell;
                } 
                catch (Exception ex) 
                {
                    Container.Track.LogError(ex, "GetCell");
                    return new UITableViewCell(); // prevent crash
                }
            });

        }
        public override UIView GetViewForHeader(UITableView tableView, nint section)
        {
            return CoreUtility.ExecuteFunction("GetViewForHeader", delegate ()
            {
                UIView header = null;

                if(this.CreateHeaderMethod != null)
                {
                    header = this.CreateHeaderMethod(this, section);
                }

                return header;
            });
        }
        public override nfloat GetHeightForHeader(UITableView tableView, nint section)
        {
            return CoreUtility.ExecuteFunction<nfloat>("GetHeightForHeader", delegate() 
            {
                if(this.HeaderSizeMethod != null)
                {
                    return this.HeaderSizeMethod(section);
                }
                return this.DefaultHeaderSize;
            });
        }
        public override nfloat GetHeightForFooter(UITableView tableView, nint section)
        {
            return CoreUtility.ExecuteFunction<nfloat>("GetHeightForHeader", delegate() 
            {
                if(this.FooterSizeMethod != null)
                {
                    return this.FooterSizeMethod(section);
                }
                return this.DefaultFooterSize;
            });
        }

        public override void Scrolled(UIScrollView scrollView)
        {
            // don't wrap for performance
            if (this.ScrollListener != null)
            {
                this.ScrollListener.OnScrolled(scrollView);
            }

            this.OnViewScrolled(scrollView);
        }

        public virtual void EnqueueReusableCustomView(UIView view, string identifier)
        {
            CoreUtility.ExecuteMethod("EnqueueReusableCustomView", delegate ()
            {
                if(!this.CachedCustomViews.ContainsKey(identifier))
                {
                    this.CachedCustomViews[identifier] = new List<UIView>();
                }
                this.CachedCustomViews[identifier].Add(view);
            });
        }
        public virtual T DequeueReusableCustomView<T>(UITableView tableView, string identifier)
            where T : UIView, new()
        {
            return CoreUtility.ExecuteFunction("DequeueReusableCustomView", delegate ()
            {
                T result = null;
                if(!this.CachedCustomViews.ContainsKey(identifier))
                {
                    this.CachedCustomViews[identifier] = new List<UIView>();
                }
                result = (T)this.CachedCustomViews[identifier].FirstOrDefault(x => x.Superview == null);
                if(tableView != null && result == null)
                {
                    result = (T)(object)tableView.DequeueReusableCell(identifier);

                    if(result != null)
                    {
                        this.CachedCustomViews[identifier].Add(result);
                    }
                }
                if(result == null)
                {
                    result = new T();
                    if(result != null)
                    {
                        this.CachedCustomViews[identifier].Add(result);
                    }
                }

                return result;
            });
        }

        #region ICoreScrollView Members

        public event EventHandler ViewScrolled;
        protected virtual void OnViewScrolled(UIScrollView scrollView)
        {
            EventHandler handler = ViewScrolled;
            if(handler != null)
            {
                handler(scrollView, EventArgs.Empty);
            }
        }

        #endregion
    
    }
}

