using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Stencil.Native.Core
{
    public static class CoreUtility
    {
        public static bool ALLOW_DISPOSE_TRACE = true; // release mode never uses this
        public static bool ALLOW_TRACING = false; // release mode never uses this

        #region Aspect Wrap

        [Obsolete("Incorrect api call, use the Async Version of this method", true)]
        public static void ExecuteMethod(string name, Func<Task> method, Action<Exception> onError = null, bool supressMethodLogging = false)
        {
        }

        public static void ExecuteMethod(string name, Action method, Action<Exception> onError = null, bool supressMethodLogging = false)
        {
#if DEBUG_METHODS
            if(!supressMethodLogging)
            {
                LogMethodTrace(string.Format("<{0}>", name));
            }
#endif
            try
            {
                method();
            }
            catch (Exception ex)
            {
                Container.Track.LogError(ex, name);
                if (onError != null)
                {
                    onError(ex);
                }
                else
                {
                    HandleException(ex);
                }
            }
#if DEBUG_METHODS
            if(!supressMethodLogging)
            {
                LogMethodTrace(string.Format("</{0}>", name));
            }
#endif
        }
        public static async Task ExecuteMethodAsync(string name, Func<Task> method, Action<Exception> onError = null, bool supressMethodLogging = false)
        {
#if DEBUG_METHODS
            if(!supressMethodLogging)
            {
            LogMethodTrace(string.Format("<{0}>", name));
            }
#endif
            try
            {
                await method();
            }
            catch (Exception ex)
            {
                Container.Track.LogError(ex, name);
                if (onError != null)
                {
                    onError(ex);
                }
                else
                {
                    HandleException(ex);
                }
            }
#if DEBUG_METHODS
            finally
            {
            if(!supressMethodLogging)
            {
            LogMethodTrace(string.Format("</{0}>", name));
            }
            }
#endif
        }
        public static T ExecuteFunction<T>(string name, Func<T> method, Action<Exception> onError = null, bool supressMethodLogging = false)
        {
#if DEBUG_METHODS
            if(!supressMethodLogging)
            {
                LogMethodTrace(string.Format("<{0}>", name));
            }
#endif
            try
            {
                return method();
            }
            catch (Exception ex)
            {
                Container.Track.LogError(ex, name);
                if (onError != null)
                {
                    onError(ex);
                }
                else
                {
                    HandleException(ex);
                }
                return default(T);
            }
#if DEBUG_METHODS
            finally
            {
                if(!supressMethodLogging)
                {
                LogMethodTrace(string.Format("</{0}>", name));
                }
            }
#endif
        }

        public static async Task<T> ExecuteFunctionAsync<T>(string name, Func<Task<T>> method, Action<Exception> onError = null)
        {
#if DEBUG_METHODS
            LogMethodTrace(string.Format("<{0}>", name));
#endif
            try
            {
                return await method();
            }
            catch (Exception ex)
            {
                Container.Track.LogError(ex, name);
                if (onError != null)
                {
                    onError(ex);
                }
                else
                {
                    HandleException(ex);
                }
                return default(T);
            }
#if DEBUG_METHODS
            finally
            {
            LogMethodTrace(string.Format("</{0}>", name));
            }
#endif
        }

        #endregion

        #region Exception Logging Helpers

        public static void HandleException(Exception ex)
        {
            AggregateException aggregate = ex as AggregateException;
            if (aggregate != null)
            {
                foreach (var item in aggregate.InnerExceptions)
                {
                    HandleException(item);
                }
                return;
            }

            //TODO:COULD: Process Special Exception Types
            //IE: Catch all HTTP:429 errors, etc
        }

        private static void LogMethodTrace(string message)
        {
#if DEBUG
            if (ALLOW_TRACING)
            {
                Container.Track.LogTrace(message);
            }
#endif
        }

        #endregion

        #region Misc Helpers

        public static void CopyStream(Stream input, Stream output, int bufferSizeBytes = 512)
        {
            byte[] buffer = new byte[bufferSizeBytes];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
            }
        }

        public static bool IsValidEmail(string email, out string parsedEmail)
        {
            parsedEmail = email;
            if (string.IsNullOrEmpty(email))
            {
                return false;
            }
            int ix = email.LastIndexOf("@");
            if (ix < 0)
            {
                return false;
            }
            try
            {
                // translate global
                string name = email.Substring(0, ix);
                string domain = email.Substring(ix + 1);
                domain = new IdnMapping().GetAscii(domain);
                parsedEmail = string.Format("{0}@{1}", name, domain);

                // verify valid
                return Regex.IsMatch(parsedEmail,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(10000));
            }
            catch
            {
                return false;// gulp
            }
        }
      
        public static string Coalesce(params string[] strings)
        {
            return strings.FirstOrDefault(s => !string.IsNullOrEmpty(s));
        }

        #endregion
    }
}
