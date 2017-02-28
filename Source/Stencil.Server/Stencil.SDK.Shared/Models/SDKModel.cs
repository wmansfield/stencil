using System;
using System.Collections.Generic;
using System.Text;

namespace Stencil.SDK.Models
{
    public abstract partial class SDKModel
    {
        // Maybe you'll do something here one day

#if __MOBILE__
        public string ui_token { get; set; }
#endif
    }
}
