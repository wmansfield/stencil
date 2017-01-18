using Codeable.Foundation.Core;
using System;
using System.Collections.Generic;
using System.Text;


namespace Stencil.Domain
{
    public partial class Account : DomainModel
    {	
        public Account()
        {
				
        }
    
        public Guid account_id { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string password_salt { get; set; }
        public bool disabled { get; set; }
        public string api_key { get; set; }
        public string api_secret { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string entitlements { get; set; }
        public string password_reset_token { get; set; }
        public DateTime? password_reset_utc { get; set; }
        public string push_ios { get; set; }
        public string push_google { get; set; }
        public string push_microsoft { get; set; }
        public DateTime? last_login_utc { get; set; }
        public string last_login_platform { get; set; }
        public DateTime created_utc { get; set; }
        public DateTime updated_utc { get; set; }
        public DateTime? deleted_utc { get; set; }
        public DateTime? sync_success_utc { get; set; }
        public DateTime? sync_invalid_utc { get; set; }
        public DateTime? sync_attempt_utc { get; set; }
        public string sync_agent { get; set; }
        public string sync_log { get; set; }
	}
}

