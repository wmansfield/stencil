using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Native.Core
{
    public static class _CoreExtensions
    {

        #region Error Methods
        public static TException FirstExceptionOfType<TException>(this Exception ex)
            where TException : Exception
        {
            TException result = ex as TException;
            if (result != null)
            {
                return result;
            }
            AggregateException aggregate = ex as AggregateException;
            if (aggregate != null)
            {
                foreach (var item in aggregate.InnerExceptions)
                {
                    result = FirstExceptionOfType<TException>(item);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }
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
        public static string FormatException(this Exception ex, string tag)
        {
            if (ex != null)
            {
                string message = string.Empty;
                AggregateException aggregates = ex as AggregateException;
                if (aggregates != null)
                {
                    message = string.Format("<Exception tag=\"{2}\" message=\"{0}\" type=\"{1}\">", EscapeXml(ex.Message), ex.GetType().ToString(), tag);
                    foreach (var item in aggregates.InnerExceptions)
                    {
                        message += string.Format("\r\n<InnerException message=\"{0}\" type=\"{1}\" stack=\"{2}\" />\r\n", EscapeXml(item.Message), item.GetType().ToString(), EscapeXml(item.StackTrace));
                    }
                    message += "</Exception>";
                }
                else
                {
                    message = string.Format("<Exception tag=\"{3}\" message=\"{0}\" type=\"{1}\" stack=\"{2}\">", EscapeXml(ex.Message), ex.GetType().ToString(), EscapeXml(ex.StackTrace), tag);
                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                        message += string.Format("\r\n<InnerException message=\"{0}\" type=\"{1}\" stack=\"{2}\" />\r\n", EscapeXml(ex.Message), ex.GetType().ToString(), EscapeXml(ex.StackTrace));
                    }
                    message += "</Exception>";
                }
                return message;
            }
            return string.Empty;
        }
        public static string EscapeXml(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml)) { return string.Empty; }

            //TODO:COULD: Escape xml for purity
            return xml;
        }

        #endregion


        #region Web Methods

        public static async Task<WebResponse> WithTimeoutMilliseconds(this Task<WebResponse> task, int millisecondsTimeout)
        {
            if (task == await Task.WhenAny(task, Task.Delay(millisecondsTimeout)).ConfigureAwait(false))
            {
                return await task;
            }
            else
            {
                throw new Exception(Container.StencilApp.GetLocalizedText(I18NToken.ConnectionTimeOut, "Connection timed out."));
            }
        }

        #endregion


        #region Time Methods

        public static readonly DateTime EPOCH_START = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime ToDateTimeFromUnixSeconds(this int seconds)
        {
            return new DateTime(seconds * 10000000L + EPOCH_START.Ticks, DateTimeKind.Utc);
        }

        public static int ToUnixSeconds(this DateTime dateTime)
        {
            return (int)dateTime.ToUnixMilliSeconds();
        }
        public static double ToUnixMilliSeconds(this DateTime dateTime)
        {
            TimeSpan ts = new TimeSpan(dateTime.ToUniversalTime().Ticks - EPOCH_START.Ticks);
            double milli = Math.Round(ts.TotalMilliseconds, 0) / 1000.0;
            return milli;
        }

        /// <summary>
        /// Takes a UTC converts to local and prints a friendly date stamp
        /// ie:  Thurs 5:00pm
        /// </summary>
        /// <returns>The local simple time stamp.</returns>
        /// <param name="dateTimeUTC">Date time UTC.</param>
        public static string ToLocalSimpleTimeStamp(this DateTime dateTimeUTC)
        {
            DateTime localTime = dateTimeUTC.ToLocalTime();
            if (localTime.Date == DateTime.Today)
            {
                return localTime.ToString("t");
            }
            else
            {
                if ((DateTime.Today - localTime.Date).TotalDays >= 7)
                {
                    return localTime.ToString("MMM d") + ", " + localTime.ToString("t");
                }
                else
                {
                    return localTime.ToString("ddd") + " " + localTime.ToString("t");
                }
            }
        }

        #endregion

        #region Dispose Methods

        public static T DisposeSafe<T>(this T item)
            where T : class, IDisposable
        {
            if (item != null)
            {
                item.Dispose();
            }
            return null;
        }

        #endregion

        #region Collection Methods


        public static IList<T> TakeLast<T>(this IList<T> source, int N)
        {
            if (source == null)
            {
                return null;
            }
            return source.Skip(Math.Max(0, source.Count - N)).ToList();
        }
        public static T LastOrDefault<T>(this IList<T> source, Func<T, bool> predicate, int maxFromEnd)
        {
            if (source == null)
            {
                return default(T);
            }
            return source.Skip(Math.Max(0, source.Count - maxFromEnd)).LastOrDefault(predicate);
        }

        public static bool ContainsKeySafe<Tkey, Tvalue>(this IDictionary<Tkey, Tvalue> item, Tkey key)
        {
            if (item != null)
            {
                return item.ContainsKey(key);
            }
            return false;
        }

        #endregion


    }
}
