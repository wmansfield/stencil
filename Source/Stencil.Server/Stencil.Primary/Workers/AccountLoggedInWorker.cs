using Codeable.Foundation.Common;
using Stencil.Primary.Daemons;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Workers
{
    public class AccountLoggedInWorker : WorkerBase<LoggedInRequest>
    {
        public static void EnqueueRequest(IFoundation foundation, LoggedInRequest request)
        {
            EnqueueRequest<AccountLoggedInWorker>(foundation, WORKER_NAME, request, (int)TimeSpan.FromMinutes(2).TotalMilliseconds); // updates every 2 mins
        }
        public const string WORKER_NAME = "AccountLoggedInWorker";

        public AccountLoggedInWorker(IFoundation iFoundation)
            : base(iFoundation, WORKER_NAME)
        {
            this.API = iFoundation.Resolve<StencilAPI>();
        }

        public StencilAPI API { get; set; }
        private ConcurrentDictionary<Guid, DateTime> _userCache = new ConcurrentDictionary<Guid, DateTime>();

        public override void EnqueueRequest(LoggedInRequest request)
        {
            base.ExecuteMethod("EnqueueRequest", delegate ()
            {
                _userCache[request.account_id] = request.login_utc;
                
                this.RequestQueue.Enqueue(request);
                // we aren't agitating, we wait for the daemon to do a full time cycle
                //this.IFoundation.GetDaemonManager().StartDaemon(this.DaemonName);
            });
        }

        protected override void ProcessRequest(LoggedInRequest request)
        {
            base.ExecuteMethod("ProcessRequest", delegate ()
            {
                DateTime info = default(DateTime);
                if (_userCache.TryRemove(request.account_id, out info))
                {
                    if (info == default(DateTime))
                    {
                        info = DateTime.UtcNow;
                    }
                    this.API.Direct.Accounts.UpdateLastLogin(request.account_id, info, request.platform);
                }
            });
        }
    }
}
