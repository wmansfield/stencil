using Codeable.Foundation.Common;
using Stencil.Domain;
using Stencil.Primary.Daemons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Business.Integration
{
    /// <summary>
    /// This is an abnormal pattern.
    /// TEntity is used to ensure a worker instance for each type, not used anywhere else
    /// </summary>
    public class DependencyWorker<TEntity> : WorkerBase<DependencyRequest>
    {
        /// <summary>
        /// Abornal pattern, takes the first processMethod and uses that for all instances
        /// This means a potential memory leak, so ensure the caller has the same lifetime as this instance.
        /// [done so that dependencies can be visualized in one shared place]
        /// </summary>
        public static void EnqueueRequest(IFoundation foundation, Dependency dependencies, Guid entity_id, Action<Dependency, Guid> processMethod)
        {
            DependencyWorker<TEntity> worker = EnqueueRequest<DependencyWorker<TEntity>>(foundation, WORKER_NAME, new DependencyRequest() { Dependencies = dependencies, EntityID = entity_id });
            if (worker != null)
            {
                if (worker.ProcessMethod == null)
                {
                    worker.ProcessMethod = processMethod;
                    worker.Execute(worker.IFoundation); // start it now (may have been waiting for processmethod)
                }
            }
        }

        public static string WORKER_NAME
        {
            get
            {
                return "Dependency_" + Path.GetExtension(typeof(TEntity).ToString()).Trim('.');
            }
        }
        public DependencyWorker(IFoundation iFoundation)
            : base(iFoundation, WORKER_NAME)
        {
        }

        public Action<Dependency, Guid> ProcessMethod { get; set; }

        protected override void ProcessRequests()
        {
            // prevent processing until we have an implementation
            if (this.ProcessMethod != null)
            {
                base.ProcessRequests();
            }
        }
        protected override void ProcessRequest(DependencyRequest request)
        {
            base.ExecuteMethod("ProcessRequest", delegate ()
            {
                this.ProcessMethod(request.Dependencies, request.EntityID);
            });
        }

    }
    public class DependencyRequest
    {
        public Guid EntityID { get; set; }
        public Dependency Dependencies { get; set; }
    }
}
