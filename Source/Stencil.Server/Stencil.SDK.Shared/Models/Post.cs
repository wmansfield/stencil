using System;
using System.Collections.Generic;
using System.Text;

namespace Stencil.SDK.Models
{
    public partial class Post : PostBase
    {	
        public Post()
        {
				
        }
    
        public virtual Guid post_id { get; set; }
        public virtual Guid account_id { get; set; }
        public virtual DateTime stamp_utc { get; set; }
        public virtual string body { get; set; }
        public virtual int remark_total { get; set; }
        
        //<IndexOnly>
        
        public int account_name { get; set; }
        
        //</IndexOnly>
	}
}

