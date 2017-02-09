using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Data.Sql
{
    public static class DatabaseExtensions
    {
        
        public static void InvalidateSync(this dbAccount model, string agent, string reason)
        {
            if (model != null)
            {
                model.sync_attempt_utc = null;
                model.sync_success_utc = null;
                model.sync_hydrate_utc = null;
                model.sync_log = reason;
                model.sync_invalid_utc = DateTime.UtcNow;
                model.sync_agent = agent;
            }
        }
        
        public static void InvalidateSync(this dbPost model, string agent, string reason)
        {
            if (model != null)
            {
                model.sync_attempt_utc = null;
                model.sync_success_utc = null;
                model.sync_hydrate_utc = null;
                model.sync_log = reason;
                model.sync_invalid_utc = DateTime.UtcNow;
                model.sync_agent = agent;
            }
        }
        
        public static void InvalidateSync(this dbRemark model, string agent, string reason)
        {
            if (model != null)
            {
                model.sync_attempt_utc = null;
                model.sync_success_utc = null;
                model.sync_hydrate_utc = null;
                model.sync_log = reason;
                model.sync_invalid_utc = DateTime.UtcNow;
                model.sync_agent = agent;
            }
        }
        
    }
}
