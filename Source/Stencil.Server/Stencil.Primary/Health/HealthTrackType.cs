using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Health
{
    public enum HealthTrackType
    {
        None = 0,
        Count = 1,
        DurationAverage = 2,
        CountAndDurationAverage = 3,
        Each = 4
    }
}
