using Codeable.Foundation.Common;
using Codeable.Foundation.Common.Aspect;
using Codeable.Foundation.Common.Daemons;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Daemons
{
    public abstract class WorkerBase<TRequest> : ChokeableClass, IDaemonTask
    {
        #region Constructor

        public WorkerBase(IFoundation iFoundation, string daemonName)
            : base(iFoundation)
        {
            this.DaemonName = daemonName;
            this.RequestQueue = new ConcurrentQueue<TRequest>();
        }

        #endregion

        #region Static Methods

        protected static readonly object _RegistrationLock = new object();

        /// <summary>
        /// Not Aspect Wrapped
        /// </summary>
        protected static TWorker EnqueueRequest<TWorker>(IFoundation foundation, string workerName, TRequest request, int millisecondInterval = 5000)
            where TWorker : WorkerBase<TRequest>
        {
            TWorker worker = EnsureWorker<TWorker>(foundation, workerName, millisecondInterval);
            worker.EnqueueRequest(request);
            return worker;
        }
        /// <summary>
        /// Not Aspect Wrapped
        /// </summary>
        protected static TWorker EnsureWorker<TWorker>(IFoundation foundation, string workerName, int millisecondInterval = 5000)
            where TWorker : WorkerBase<TRequest>
        {
            IDaemonManager daemonManager = foundation.GetDaemonManager();
            IDaemonTask daemonTask = daemonManager.GetRegisteredDaemonTask(workerName);
            if (daemonTask == null)
            {
                lock (_RegistrationLock)
                {
                    daemonTask = daemonManager.GetRegisteredDaemonTask(workerName);
                    if (daemonTask == null)
                    {
                        if (millisecondInterval <= 1000)
                        {
                            millisecondInterval = 5000; // if you give bad data, we force to 5 seconds.
                        }
                        DaemonConfig config = new DaemonConfig()
                        {
                            InstanceName = workerName,
                            ContinueOnError = true,
                            IntervalMilliSeconds = millisecondInterval,
                            StartDelayMilliSeconds = 0,
                            TaskConfiguration = string.Empty
                        };
                        TWorker worker = (TWorker)foundation.Container.Resolve(typeof(TWorker), null);
                        daemonManager.RegisterDaemon(config, worker, true);
                    }
                }
                daemonTask = daemonManager.GetRegisteredDaemonTask(workerName);
            }
            return daemonTask as TWorker;
        }

        #endregion

        #region Protected Properties

        protected virtual bool Executing { get; set; }

        protected virtual ConcurrentQueue<TRequest> RequestQueue { get; set; }

        #endregion

        #region Public Methods

        public virtual void EnqueueRequest(TRequest request)
        {
            base.ExecuteMethod("EnqueueRequest", delegate ()
            {
                this.RequestQueue.Enqueue(request);
                this.IFoundation.GetDaemonManager().StartDaemon(this.DaemonName); // agitate
            });
        }

        #endregion

        #region IDaemonTask Members

        public virtual DaemonSynchronizationPolicy SynchronizationPolicy
        {
            get { return DaemonSynchronizationPolicy.None; }
        }
        public virtual string DaemonName { get; private set; }
        public virtual void Execute(IFoundation iFoundation)
        {
            if (this.Executing) { return; } // safety

            base.ExecuteMethod("Execute", delegate ()
            {
                try
                {
                    this.Executing = true;

                    this.ProcessRequests();
                }
                finally
                {
                    this.Executing = false;
                }
            });
        }

        #endregion

        #region Protected Methods

        protected abstract void ProcessRequest(TRequest request);

        protected virtual void ProcessRequests()
        {
            base.ExecuteMethod("ProcessRequests", delegate ()
            {
                TRequest request = default(TRequest);
                while (this.RequestQueue.TryDequeue(out request))
                {
                    try
                    {
                        this.ProcessRequest(request);
                    }
                    catch (Exception ex)
                    {
                        base.IFoundation.LogError(ex, "ProcessRequest");
                    }
                }
            });
        }

        #endregion
    }
}
