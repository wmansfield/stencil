using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;

namespace Stencil.Native.iOS.Core.Data
{
    public interface IScrollListener
    {
        void OnScrolled(UIScrollView scrollView);
        bool ListeningDisabled { get; set; }
    }
}