using Codeable.Foundation.Common;
using Codeable.Foundation.Core.Caching;
using Codeable.Foundation.Core.Unity;
using Stencil.Data.Sql;
using Stencil.Primary.Business.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Business.Direct.Implementation
{
    public abstract class BusinessBase : BusinessBaseHealth
    {
        public BusinessBase(IFoundation foundation, string trackPrefix)
            : base(foundation, trackPrefix)
        {
            this.DataContextFactory = foundation.Resolve<IStencilContextFactory>();
            this.API = new StencilAPI(foundation);
            this.SharedCacheStatic15 = new AspectCache("BusinessBase", foundation, new ExpireStaticLifetimeManager("BusinessBase", TimeSpan.FromMinutes(15)));
        }

        public StencilAPI API { get; set; }
        /// <summary>
        /// Shared with all business elements, use Keyed
        /// </summary>
        public AspectCache SharedCacheStatic15 { get; set; }


        public virtual string DefaultAgent
        {
            get
            {
                return Daemons.Agents.AGENT_DEFAULT;
            }
        }

        public IFoundation Foundation
        {
            get
            {
                return base.IFoundation;
            }
        }

        protected virtual IStencilContextFactory DataContextFactory { get; set; }
        protected virtual IDependencyCoordinator DependencyCoordinator
        {
            get
            {
                return this.IFoundation.Resolve<IDependencyCoordinator>();
            }
        }

        public virtual StencilContext CreateSQLContext()
        {
            return DataContextFactory.CreateContext();
        }

    }
}
