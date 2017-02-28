using Codeable.Foundation.Core;
using System;
using System.Collections.Generic;
using System.Text;


namespace Stencil.Domain
{
    public partial class Post : DomainModel
    {	
        public Post()
        {
				
        }
    
        public Guid post_id { get; set; }
        public Guid account_id { get; set; }
        public DateTime stamp_utc { get; set; }
        public string body { get; set; }
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

