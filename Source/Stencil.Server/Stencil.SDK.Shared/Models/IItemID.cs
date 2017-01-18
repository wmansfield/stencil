using System;
using System.Collections.Generic;
using System.Text;

namespace Stencil.SDK.Models
{
    /// <summary>
    /// Used in native for unique id requirements
    /// </summary>
    public interface IItemID
    {
        long item_id { get; set; }
    }
}
