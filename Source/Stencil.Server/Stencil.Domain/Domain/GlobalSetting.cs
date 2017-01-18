using Codeable.Foundation.Core;
using System;
using System.Collections.Generic;
using System.Text;


namespace Stencil.Domain
{
    public partial class GlobalSetting : DomainModel
    {	
        public GlobalSetting()
        {
				
        }
    
        public Guid global_setting_id { get; set; }
        public string name { get; set; }
        public string value { get; set; }
        
	}
}

