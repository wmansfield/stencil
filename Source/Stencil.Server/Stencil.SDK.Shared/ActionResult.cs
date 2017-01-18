using System;
using System.Collections.Generic;
using System.Text;

namespace Stencil.SDK
{
    public class ActionResult
    {
        public ActionResult()
        {
        }
        public virtual bool success { get; set; }
        public virtual string message { get; set; }
    }
}
