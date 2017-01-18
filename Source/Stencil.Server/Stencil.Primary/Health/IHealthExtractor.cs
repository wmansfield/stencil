using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Health
{
    public interface IHealthExtractor
    {
        void ExtractHealthMetrics(HealthReportGenerator generator);
    }
}
