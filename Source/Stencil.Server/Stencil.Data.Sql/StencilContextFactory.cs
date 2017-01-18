using Codeable.Foundation.Common;
using Codeable.Foundation.Common.Aspect;
using Codeable.Foundation.Core.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stencil.Common;
using Stencil.Common.Configuration;
using System.Data.Entity.Core.EntityClient;

namespace Stencil.Data.Sql
{
    public class StencilContextFactory : ChokeableClass, IStencilContextFactory
    {
        public StencilContextFactory(IFoundation foundation)
            : base(foundation)
        {
            this.ConnectionStringCache = new AspectCache("StencilContextFactory", this.IFoundation);
        }

        public virtual StencilContext CreateContext()
        {
            return base.ExecuteFunction<StencilContext>("CreateContext", delegate ()
            {
                return new StencilContext(this.GetSqlConnectionString());
            });
        }

        protected AspectCache ConnectionStringCache { get; set; }

        protected virtual string GetSqlConnectionString()
        {
            return base.ExecuteFunction<string>("GetSqlConnectionString", delegate ()
            {
                return this.ConnectionStringCache.PerFoundation("StencilContextFactory.GetSqlConnectionString", delegate ()
                {
                    ISettingsResolver settingsResolver = this.IFoundation.Resolve<ISettingsResolver>();
                    string connectionString = settingsResolver.GetSetting(CommonAssumptions.APP_KEY_SQL_DB);
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        throw new Exception("Connection string not found: " + CommonAssumptions.APP_KEY_SQL_DB);
                    }
                    if (!connectionString.StartsWith("metadata"))
                    {
                        EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder();
                        entityBuilder.ProviderConnectionString = connectionString;
                        entityBuilder.Provider = "System.Data.SqlClient";
                        entityBuilder.Metadata = @"res://*/StencilModel.csdl|res://*/StencilModel.ssdl|res://*/StencilModel.msl";
                        connectionString = entityBuilder.ToString();
                    }
                    this.IFoundation.LogWarning("ConnectionString: " + connectionString);
                    return connectionString;
                });
            });
        }
    }
}
