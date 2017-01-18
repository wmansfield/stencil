using Codeable.Foundation.Common;
using Stencil.Common.Configuration;
using Stencil.Common.Integration;
using Stencil.Common.Synchronization;
using Stencil.Primary.Business.Integration;
using Stencil.Primary.Emaling;

namespace Stencil.Primary
{
    public class StencilAPIIntegration : BaseClass
    {
        public StencilAPIIntegration(IFoundation ifoundation)
            : base(ifoundation)
        {
        }

        public IEmailer Email
        {
            get { return this.IFoundation.Resolve<IEmailer>(); }
        }

        public INotifySynchronizer Synchronization
        {
            get { return this.IFoundation.Resolve<INotifySynchronizer>(); }
        }
        public ISettingsResolver SettingsResolver
        {
            get { return this.IFoundation.Resolve<ISettingsResolver>(); }
        }
        public IDependencyCoordinator Dependencies
        {
            get { return this.IFoundation.Resolve<IDependencyCoordinator>(); }
        }
        public IPushNotifications Push
        {
            get { return this.IFoundation.Resolve<IPushNotifications>(); }
        }
    }
}