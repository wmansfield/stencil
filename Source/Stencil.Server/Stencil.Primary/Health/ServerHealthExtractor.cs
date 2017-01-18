using Codeable.Foundation.Common;
using Codeable.Foundation.Common.Aspect;
using Codeable.Foundation.Core.Caching;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Health
{
    public class ServerHealthExtractor : ChokeableClass, IHealthExtractor
    {
        public ServerHealthExtractor(IFoundation foundation)
            : base(foundation)
        {
            HealthReporter.Current.AddExtractor(this);
        }

        #region Health Extractor

        public void ExtractHealthMetrics(HealthReportGenerator generator)
        {
            try
            {
                // extract cache size
                using(Process process = Process.GetCurrentProcess())
                {
                    process.Refresh();
                    int mbMemory = (int)((process.WorkingSet64 / 1024m) / 1024m);
                    generator.UpdateMetric(HealthTrackType.Count, string.Format(HealthReporter.SERVER_MEMORY_SIZE, "private"), 0, mbMemory);
                }
            }
            catch (Exception ex)
            {
                this.IFoundation.LogError(ex, "ExtractHealthMetrics");
            }
        }

        #endregion
    }
}
