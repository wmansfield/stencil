using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Workers
{
    public class LoggedInRequest
    {
        public LoggedInRequest()
        {
        }
        public DateTime login_utc { get; set; }
        public Guid account_id { get; set; }
        public string platform { get; set; }
    }
}
