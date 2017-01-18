using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Common.Integration
{
    /// <summary>
    /// Essentially designed for amazon. Could use more abstract terminology
    /// </summary>
    public class TemporaryUploadCredentials
    {
        public virtual string bucket { get; set; }
        public virtual string secret_access_key { get; set; }
        public virtual string access_key_id { get; set; }
        public virtual string session_token { get; set; }
    }
}
