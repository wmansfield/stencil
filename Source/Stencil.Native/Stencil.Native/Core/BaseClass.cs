using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Native.Core
{
    public abstract partial class BaseClass : INotifyPropertyChanged, IDisposable
    {
        public BaseClass(string trackPrefix)
        {
            this.TrackPrefix = trackPrefix;
        }
        ~BaseClass()
        {
            this.Dispose(false);
        }
        protected string TrackPrefix { get; set; }

        [Obsolete("Incorrect api call, use the Async Version of this method", true)]
        protected virtual void ExecuteMethod(string name, Func<Task> method, Action<Exception> onError = null, bool supressMethodLogging = false)
        {
        }
        protected virtual void ExecuteMethod(string name, Action method, Action<Exception> onError = null, bool supressMethodLogging = false)
        {
            CoreUtility.ExecuteMethod(string.Format("{0}.{1}", this.TrackPrefix, name), method, onError, supressMethodLogging);
        }
        protected virtual Task ExecuteMethodAsync(string name, Func<Task> method, Action<Exception> onError = null, bool supressMethodLogging = false)
        {
            return CoreUtility.ExecuteMethodAsync(string.Format("{0}.{1}", this.TrackPrefix, name), method, onError, supressMethodLogging);
        }
        protected virtual T ExecuteFunction<T>(string name, Func<T> method, Action<Exception> onError = null, bool supressMethodLogging = false)
        {
            return CoreUtility.ExecuteFunction<T>(string.Format("{0}.{1}", this.TrackPrefix, name), method, onError, supressMethodLogging);
        }
        protected virtual T ExecuteThrowingFunction<T>(string name, Func<T> method, bool supressMethodLogging = false)
        {
            return CoreUtility.ExecuteFunction<T>(string.Format("{0}.{1}", this.TrackPrefix, name), method, delegate (Exception ex) { throw ex; }, supressMethodLogging);
        }
        protected virtual Task<T> ExecuteFunctionAsync<T>(string name, Func<Task<T>> method, Action<Exception> onError = null)
        {
            return CoreUtility.ExecuteFunctionAsync<T>(string.Format("{0}.{1}", this.TrackPrefix, name), method, onError);
        }
        protected virtual Task<T> ExecuteThrowingFunctionAsync<T>(string name, Func<Task<T>> method)
        {
            return CoreUtility.ExecuteFunctionAsync<T>(string.Format("{0}.{1}", this.TrackPrefix, name), method, delegate (Exception ex) { throw ex; });
        }

        protected virtual void LogWarning(string message, string tag = "")
        {
            Container.Track.LogWarning(this.TrackPrefix + ":" + tag + ": " + message);
        }
        protected virtual void LogTrace(string message, string tag = "")
        {
            Container.Track.LogTrace(this.TrackPrefix + ":" + tag + ": " + message);
        }
        protected virtual void LogError(Exception ex, string tag = "")
        {
            Container.Track.LogError(ex, this.TrackPrefix + ":" + tag);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
#if DEBUG
                if (CoreUtility.ALLOW_DISPOSE_TRACE)
                {
                    this.LogTrace("BaseClass.Disposing: " + this.TrackPrefix);
                }
#endif
            }
        }
        public void Dispose()
        {
            this.Dispose(true);
        }

        public void RaisePropertyChanged(string propertyName)
        {
            var changedArgs = new PropertyChangedEventArgs(propertyName);
            RaisePropertyChanged(changedArgs);
        }
        public void RaisePropertyChanged(PropertyChangedEventArgs args)
        {
            var evnt = this.PropertyChanged;
            if (evnt != null)
            {
                evnt(this, args);
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;


        private HashSet<string> _executingCommands = new HashSet<string>();
        protected virtual HashSet<string> executingCommands
        {
            get
            {
                return _executingCommands;
            }
        }

        [Obsolete("Incorrect api call, use the Async Version of this method", true)]
        protected virtual void ExecuteMethodOrSkip(string name, Func<Task> method, Action<Exception> onError = null)
        {
            this.ExecuteMethodAsync("ExecuteOrSkip", async delegate ()
            {
                bool added = _executingCommands.Add(name);
                if (!added) { return; }
                try
                {
                    await method();
                }
                finally
                {
                    _executingCommands.Remove(name);
                }
            });
        }

        /// <summary>
        /// Executes the command unless the command is already running, then its skipped
        /// </summary>
        protected virtual void ExecuteMethodOrSkip(string name, Action method, Action<Exception> onError = null)
        {
            this.ExecuteMethod("ExecuteOrSkip", delegate ()
            {
                bool added = _executingCommands.Add(name);
                if (!added) { return; }
                try
                {
                    method();
                }
                finally
                {
                    _executingCommands.Remove(name);
                }
            });
        }
        /// <summary>
        /// Executes the command unless the command is already running, then its skipped
        /// </summary>
        protected virtual Task ExecuteMethodOrSkipAsync(string name, Func<Task> method, Action<Exception> onError = null)
        {
            return this.ExecuteMethodAsync("ExecuteMethodOrSkipAsync", async delegate ()
            {
                bool added = _executingCommands.Add(name);
                if (!added) { return; }
                try
                {
                    await method();
                }
                finally
                {
                    _executingCommands.Remove(name);
                }
            });
        }

        protected virtual bool IsExecutingCommand(string name)
        {
            return this.ExecuteFunction("IsExecutingCommand", delegate ()
            {
                return _executingCommands.Contains(name);
            });
        }
    }
}
