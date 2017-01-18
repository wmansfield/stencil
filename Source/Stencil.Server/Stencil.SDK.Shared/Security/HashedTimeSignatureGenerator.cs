using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Stencil.SDK.Security
{
    public partial class HashedTimeSignatureGenerator
    {
        public virtual string CreateSignature(string apiKey, string apiSecret)
        {
            string prefix = string.Format("{0}{1}", apiKey, apiSecret);
            string unixUTCNow = GetUnixTime().ToString();
            return MD5.Create().GenerateHash(prefix + unixUTCNow);

        }
#if !WEB
        protected virtual long GetUnixTime()
        {
            var unixTime = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)unixTime.TotalSeconds;
        }
#endif
    }
}
