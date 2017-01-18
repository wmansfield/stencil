using System;
using System.Collections.Generic;
using System.Text;

namespace Stencil.SDK.Models
{
    public partial class Account : SDKModel
    {	
        public Account()
        {
				
        }
    
        public virtual Guid account_id { get; set; }
        public virtual string email { get; set; }
        public virtual string password { get; set; }
        public virtual string password_salt { get; set; }
        public virtual bool disabled { get; set; }
        public virtual string api_key { get; set; }
        public virtual string api_secret { get; set; }
        public virtual string first_name { get; set; }
        public virtual string last_name { get; set; }
        public virtual string entitlements { get; set; }
        public virtual string password_reset_token { get; set; }
        public virtual DateTime? password_reset_utc { get; set; }
        public virtual string push_ios { get; set; }
        public virtual string push_google { get; set; }
        public virtual string push_microsoft { get; set; }
        public virtual DateTime? last_login_utc { get; set; }
        public virtual string last_login_platform { get; set; }
        
	}
}

