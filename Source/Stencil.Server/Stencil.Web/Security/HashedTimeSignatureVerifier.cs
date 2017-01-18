using Stencil.Common;
using System;
using System.Security.Cryptography;

namespace Stencil.Web.Security
{
    public class HashedTimeSignatureVerifier
    {
        public virtual string CreateSignature(string apiKey, string apiSecret)
        {
            string prefix = string.Format("{0}{1}", apiKey, apiSecret);
            string unixUTCNow = ToUnixTime(DateTime.UtcNow).ToString();
            using (MD5 md5 = MD5.Create())
            {
                return md5.HashAsString(prefix + unixUTCNow);
            }
        }
        public virtual bool ValidateSignature(string key, string secret, string signature)
        {
            using (MD5 md5 = MD5.Create())
            {
                string prefix = string.Format("{0}{1}", key, secret);
                DateTime start = DateTime.UtcNow;
                DateTime end = start;
                for (int i = 0; i < 300; i++)
                {
                    string startTime = ToUnixTime(start.AddSeconds(i)).ToString();
                    string endTime = ToUnixTime(end.AddSeconds(-i)).ToString();
                    if ((signature == md5.HashAsString(prefix + startTime))
                        || (signature == md5.HashAsString(prefix + endTime)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected virtual long ToUnixTime(DateTime utcTime)
        {
            var unixTime = utcTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)unixTime.TotalSeconds;
        }

    }
}