using System;
using System.Collections.Generic;
using System.Text;

namespace Stencil.SDK.Models
{
    public partial class GlobalSetting : SDKModel
    {	
        public GlobalSetting()
        {
				
        }
    
        public virtual Guid global_setting_id { get; set; }
        public virtual string name { get; set; }
        public virtual string value { get; set; }
        
	}
}

