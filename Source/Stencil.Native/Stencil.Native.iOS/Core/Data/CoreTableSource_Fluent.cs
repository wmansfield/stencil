using System;
using System.Collections.Generic;
using UIKit;
using Foundation;

namespace Stencil.Native.iOS.Core.Data
{
    public static class CoreTableSource_Fluent
    {
        public static CoreTableSource<TItem> For<TItem>(this CoreTableSource<TItem> source, IEnumerable<TItem> data)
        {
            source.Items = new List<TItem>();
            foreach (var item in data)
            {
                source.Items.Add(item);
            }
            return source;
        }
        public static CoreTableSource<TItem> WhenSizingRows<TItem>(this CoreTableSource<TItem> source, Func<TItem, NSIndexPath, nfloat> rowSizeMethod)
        {
            source.RowSizeMethod = rowSizeMethod;
            return source;
        }
        public static CoreTableSource<TItem> WhenCountingSections<TItem>(this CoreTableSource<TItem> source, Func<nint> countSectionMethod)
        {
            source.CountSectionsMethod = countSectionMethod;
            return source;
        }
        public static CoreTableSource<TItem> WhenCountingRows<TItem>(this CoreTableSource<TItem> source, Func<nint, nint> countRowsMethod)
        {
            source.CountRowsInSectionMethod = countRowsMethod;
            return source;
        }
        public static CoreTableSource<TItem> WhenCreatingCell<TItem>(this CoreTableSource<TItem> source, Func<TItem, NSIndexPath, UITableViewCell> createCellMethod)
        {
            source.CreateCellMethod = createCellMethod;
            return source;
        }
        public static CoreTableSource<TItem> WhenCreatingHeader<TItem>(this CoreTableSource<TItem> source, Func<CoreTableSource<TItem>, nint, UIView> createHeaderMethod)
        {
            source.CreateHeaderMethod = createHeaderMethod;
            return source;
        }

        public static CoreTableSource<TItem> WhenItemSelected<TItem, TCell>(this CoreTableSource<TItem> source, Action<TItem, TCell> selectedMethod)
            where TCell : UITableViewCell
        {
            source.OnSelectedMethod = delegate(TItem item, UITableViewCell cell) 
            {
                selectedMethod(item,(TCell)cell);
            };
            return source;
        }
        public static CoreTableSource<TItem> WhenItemSelected<TItem>(this CoreTableSource<TItem> source, Action<TItem, UITableViewCell> selectedMethod)
        {
            source.OnSelectedMethod = selectedMethod;
            return source;
        }



        public static CoreTableSource<TItem> SetSectionCount<TItem>(this CoreTableSource<TItem> source, int sectionCount)
        {
            source.DefaultSectionCount = sectionCount;
            return source;
        }
        public static CoreTableSource<TItem> WhenSizingHeaders<TItem>(this CoreTableSource<TItem> source, Func<nint, nfloat> headerSizeMethod)
        {
            source.HeaderSizeMethod = headerSizeMethod;
            return source;
        }
        public static CoreTableSource<TItem> WhenSizingFooters<TItem>(this CoreTableSource<TItem> source, Func<nint, nfloat> foooterSizeMethod)
        {
            source.FooterSizeMethod = foooterSizeMethod;
            return source;
        }
        public static CoreTableSource<TItem> SetHeaderSize<TItem>(this CoreTableSource<TItem> source, nfloat headerSize)
        {
            source.DefaultHeaderSize = headerSize;
            return source;
        }
        public static CoreTableSource<TItem> SetFooterSize<TItem>(this CoreTableSource<TItem> source, nfloat footerSize)
        {
            source.DefaultFooterSize = footerSize;
            return source;
        }
        public static CoreTableSource<TItem> WhenDeletingRow<TItem>(this CoreTableSource<TItem> source, Func<NSIndexPath, bool> canDelete, Action<NSIndexPath> performDelete, Func<NSIndexPath, string> deleteText = null)
        {
            source.CanDeleteRowMethod = canDelete;
            source.RowDeleteMethod = performDelete;
            source.RowDeleteTextMethod = deleteText;
            return source;
        }
        public static CoreTableSource<TItem> WhenEditingRow<TItem>(this CoreTableSource<TItem> source, Func<NSIndexPath, UITableViewRowAction[]> customActionsMethod)
        {
            source.CustomRowActionsMethod = customActionsMethod;
            return source;
        }



        public static CoreFlexibleTableSource WhenCreatingFlexibleHeader(this CoreFlexibleTableSource source, Func<CoreTableSource<object>, nint, UIView> createHeaderMethod)
        {
            source.CreateHeaderMethod = createHeaderMethod;
            return source;
        }
        public static CoreFlexibleTableSource WhenCountingFlexibleSections(this CoreFlexibleTableSource source, Func<nint> countSectionMethod)
        {
            source.CountSectionsMethod = countSectionMethod;
            return source;
        }
        public static CoreFlexibleTableSource WhenCountingFlexibleRows(this CoreFlexibleTableSource source, Func<nint, nint> countRowsMethod)
        {
            source.CountRowsInSectionMethod = countRowsMethod;
            return source;
        }
        public static CoreFlexibleTableSource WhenCreatingFlexibleCell(this CoreFlexibleTableSource source, Func<NSIndexPath, UITableViewCell> createCellMethod)
        {
            source.CreateCellRawMethod = createCellMethod;
            return source;
        }
        public static CoreFlexibleTableSource WhenFlexibleItemSelected(this CoreFlexibleTableSource source, Action<NSIndexPath, UITableViewCell> selectedMethod)
        {
            source.OnSelectedMethodRaw = selectedMethod;
            return source;
        }
        public static CoreFlexibleTableSource WhenSizingFlexibleRows(this CoreFlexibleTableSource source, Func<NSIndexPath, nfloat> rowSizeMethod)
        {
            source.RowSizeMethodRaw = rowSizeMethod;
            return source;
        }

        public static CoreFlexibleTableSource WhenSizingFlexibleHeaders(this CoreFlexibleTableSource source, Func<nint, nfloat> headerSizeMethod)
        {
            source.HeaderSizeMethod = headerSizeMethod;
            return source;
        }

        public static CoreFlexibleTableSource WhenDeletingFlexibleRow(this CoreFlexibleTableSource source, Func<NSIndexPath, bool> canDelete, Action<NSIndexPath> performDelete, Func<NSIndexPath, string> deleteText = null)
        {
            source.CanDeleteRowMethod = canDelete;
            source.RowDeleteMethod = performDelete;
            source.RowDeleteTextMethod = deleteText;
            return source;
        }

        public static CoreFlexibleTableSource WhenEditingFlexibleRow(this CoreFlexibleTableSource source, Func<NSIndexPath, UITableViewRowAction[]> customActionsMethod)
        {
            source.CustomRowActionsMethod = customActionsMethod;
            return source;
        }
    }
}

