using AutoMapper;
using Codeable.Foundation.Common;
using Codeable.Foundation.Common.Daemons;
using Codeable.Foundation.Common.System;
using Codeable.Foundation.Core;
using Microsoft.Practices.Unity;
using Stencil.Common.Configuration;
using Stencil.Data.Sql;
using Stencil.Primary.Business.Index;
using Stencil.Primary.Business.Integration;
using Stencil.Primary.Business.Integration.Implementation;
using Stencil.Primary.Emaling;
using Stencil.Primary.Exceptions;
using Stencil.Primary.Health;
using Stencil.Primary.Health.Daemons;
using Stencil.Primary.Health.Exceptions;
using Stencil.Primary.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Foundation
{
    public partial class StencilBootStrap : CoreBootStrap
    {
        public StencilBootStrap()
        {

        }

        public override void OnAfterSelfRegisters(IFoundation foundation)
        {
            base.OnAfterSelfRegisters(foundation);

            this.RegisterDataMapping(foundation);

            foundation.Container.RegisterType<IEmailer, SimpleEmailer>(new ContainerControlledLifetimeManager());
            foundation.Container.RegisterType<ISettingsResolver, AppConfigSettingsResolver>(new ContainerControlledLifetimeManager());
            foundation.Container.RegisterType<IStencilContextFactory, StencilContextFactory>(new ContainerControlledLifetimeManager());
            foundation.Container.RegisterType<IStencilElasticClientFactory, StencilElasticClientFactory>(new ContainerControlledLifetimeManager());
            foundation.Container.RegisterType<IDependencyCoordinator, DependencyCoordinator>(new ContainerControlledLifetimeManager());

            this.RegisterDataElements(foundation);

            this.RegisterErrorHandlers(foundation);
        }
        protected virtual void RegisterDataMapping(IFoundation foundation)
        {
            Mapper.AddProfile<PrimaryMappingProfile>();
        }

        protected virtual void RegisterErrorHandlers(IFoundation foundation)
        {
            // Replace Exception Handlers
            foundation.Container.RegisterType<IHandleException, FriendlyExceptionHandler>(new ContainerControlledLifetimeManager());
            foundation.Container.RegisterType<IHandleExceptionProvider, FriendlyExceptionHandlerProvider>(new ContainerControlledLifetimeManager());
            foundation.Container.RegisterInstance<FriendlyExceptionHandlerProvider>(new FriendlyExceptionHandlerProvider(foundation, foundation.GetLogger()), new ContainerControlledLifetimeManager());

        }

        public override void OnAfterBootStrapComplete(IFoundation foundation)
        {
            base.OnAfterBootStrapComplete(foundation);

            // Replace Exception Handlers
            foundation.Container.RegisterType<IHandleException, HealthFriendlyExceptionHandler>(new ContainerControlledLifetimeManager());
            foundation.Container.RegisterType<IHandleExceptionProvider, HealthFriendlyExceptionHandlerProvider>(new ContainerControlledLifetimeManager());
            foundation.Container.RegisterInstance<HealthFriendlyExceptionHandlerProvider>(new HealthFriendlyExceptionHandlerProvider(foundation, foundation.GetLogger()), new ContainerControlledLifetimeManager());

            foundation.Container.RegisterType<IHandleException, HealthSwallowExceptionHandler>(Assumptions.SWALLOWED_EXCEPTION_HANDLER, new ContainerControlledLifetimeManager());
            foundation.Container.RegisterType<IHandleExceptionProvider, HealthSwallowExceptionHandlerProvider>(Assumptions.SWALLOWED_EXCEPTION_HANDLER, new ContainerControlledLifetimeManager());
            foundation.Container.RegisterInstance<HealthSwallowExceptionHandlerProvider>(Assumptions.SWALLOWED_EXCEPTION_HANDLER, new HealthSwallowExceptionHandlerProvider(foundation, foundation.GetLogger()), new ContainerControlledLifetimeManager());

            foundation.Container.RegisterInstance<ServerHealthExtractor>(new ServerHealthExtractor(foundation), new ContainerControlledLifetimeManager());

            DaemonConfig healthConfig = new DaemonConfig()
            {
                InstanceName = HealthReportDaemon.DAEMON_NAME,
                ContinueOnError = true,
                IntervalMilliSeconds = 15 * 1000, // every 15 seconds
                StartDelayMilliSeconds = 60 * 1000,
                TaskConfiguration = string.Empty
            };
            foundation.GetDaemonManager().RegisterDaemon(healthConfig, new HealthReportDaemon(foundation), true);
            
        }

    }
}