using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Business.Index
{
    public interface IStencilElasticClientFactory
    {
        ElasticClient CreateClient();
        string IndexName { get; }
        string HostUrl { get; }
    }
}
