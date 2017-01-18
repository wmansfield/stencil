using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Business.Direct.Implementation
{
    public class InterceptArgs<T>
    {
        public bool Intercepted { get; set; }
        public T ReturnEntity { get; set; }
        public bool ForInsert { get; internal set; }
    }
}
