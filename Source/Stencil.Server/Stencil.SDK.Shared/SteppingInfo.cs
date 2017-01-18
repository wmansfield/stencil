using System;
using System.Collections.Generic;
using System.Text;

namespace Stencil.SDK
{
    public class SteppingInfo
    {
        public SteppingInfo()
        {
        }

        public bool more { get; set; }
        public long skip { get; set; }
        public long current { get; set; }
        /// <summary>
        /// not always present
        /// </summary>
        public long total { get; set; }
    }
}
