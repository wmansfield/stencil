using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Common
{
    public static class _CommonExtensions
    {
        #region Date Extensions

        private static readonly DateTime EPOCH_START = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static int ToUnixSecondsUTC(this DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Unspecified)
            {
                dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            }
            TimeSpan ts = new TimeSpan(dateTime.ToUniversalTime().Ticks - EPOCH_START.Ticks);
            double seconds = Math.Round(ts.TotalMilliseconds, 0) / 1000.0;
            return (int)seconds;
        }

        #endregion

        #region Encryption Extensions
        public static string HashAsString(this MD5 md5, string content)
        {
            StringBuilder sBuilder = new StringBuilder();

            if (md5 != null)
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(content));
                for (int i = 0; i < hash.Length; i++)
                {
                    sBuilder.Append(hash[i].ToString("x2"));
                }
            }
            return sBuilder.ToString();
        }
        public static string HashAsString(this SHA256Managed sha256, string content)
        {
            StringBuilder sBuilder = new StringBuilder();
            if (sha256 != null)
            {
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
                for (int i = 0; i < hash.Length; i++)
                {
                    sBuilder.Append(hash[i].ToString("x2"));
                }
            }
            return sBuilder.ToString();
        }

        #endregion

        #region Exception Extensions

        public static Exception FirstNonAggregateException(this Exception ex)
        {
            AggregateException aggregate = ex as AggregateException;
            if (aggregate != null)
            {
                foreach (var item in aggregate.InnerExceptions)
                {
                    return FirstNonAggregateException(item);
                }
            }
            return ex;
        }

        #endregion
    }
}
