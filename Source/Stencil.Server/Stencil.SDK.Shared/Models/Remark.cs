using System;
using System.Collections.Generic;
using System.Text;

namespace Stencil.SDK.Models
{
    public partial class Remark : RemarkBase
    {	
        public Remark()
        {
				
        }
    
        public virtual Guid remark_id { get; set; }
        public virtual Guid post_id { get; set; }
        public virtual Guid account_id { get; set; }
        public virtual DateTime stamp_utc { get; set; }
        public virtual string text { get; set; }
        
	}
}

