using System;
using System.Collections.Generic;
using System.Text;

namespace Stencil.Native.Core
{
    public partial class CoreTrack : ITrack // do not inherit from baseclass, could cause endless loop
    {
        public virtual void LogError(Exception ex, string tag = "")
        {
            this.LogError(ex.FormatException(tag));
        }
        public virtual void LogError(string message, string tag = "")
        {
            if (string.IsNullOrEmpty(tag))
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Error: {0}", message));
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Error:{0}: {1}", tag, message));
            }
        }
        public virtual void LogWarning(string message, string tag = "")
        {
            if (string.IsNullOrEmpty(tag))
            {
                System.Diagnostics.Debug.WriteLine(string.Format("{0}", message));
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(string.Format("{0}: {1}", tag, message));
            }
        }

        public virtual void LogTrace(string message, string tag = "")
        {
#if DEBUG
            if (string.IsNullOrEmpty(tag))
            {
                System.Diagnostics.Debug.WriteLine(string.Format("{0}", message));
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(string.Format("{0}: {1}", tag, message));
            }
#endif
        }
    }
}
