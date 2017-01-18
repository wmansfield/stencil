using Codeable.Foundation.Common;
using Codeable.Foundation.Common.Aspect;
using Codeable.Foundation.Common.System;
using Stencil.Primary.Health;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Business.Index.Implementation
{
    /// <summary>
    /// Should only be inherited by IndexerBase
    /// </summary>
    public abstract class IndexerBaseHealth : ChokeableClass
    {
        public IndexerBaseHealth(IFoundation foundation, IHandleExceptionProvider iHandleExceptionProvider, string trackPrefix)
            : base(foundation, iHandleExceptionProvider)
        {
            this.TrackPrefix = trackPrefix;
        }

        public virtual string TrackPrefix { get; set; }

        #region Health Monitoring

        protected override void ExecuteMethod(string methodName, Action action, params object[] parameters)
        {
            using (var scope = HealthReporter.BeginTrack(HealthTrackType.CountAndDurationAverage, string.Format(HealthReporter.CACHE_READ_FORMAT, this.TrackPrefix + "." + methodName)))
            {
                base.ExecuteMethod(methodName, action, parameters);
            }
        }
        protected override K ExecuteFunction<K>(string methodName, Func<K> function, params object[] parameters)
        {
            using (var scope = HealthReporter.BeginTrack(HealthTrackType.CountAndDurationAverage, string.Format(HealthReporter.CACHE_READ_FORMAT, this.TrackPrefix + "." + methodName)))
            {
                return base.ExecuteFunction<K>(methodName, function, parameters);
            }
        }
        protected virtual K ExecuteFunctionWrite<K>(string methodName, Func<K> function, params object[] parameters)
        {
            using (var scope = HealthReporter.BeginTrack(HealthTrackType.CountAndDurationAverage, string.Format(HealthReporter.CACHE_WRITE_FORMAT, this.TrackPrefix + "." + methodName)))
            {
                return base.ExecuteFunction<K>(methodName, function, parameters);
            }
        }
        protected virtual K ExecuteFunctionNoHealth<K>(string methodName, Func<K> function, params object[] parameters)
        {
            return base.ExecuteFunction<K>(methodName, function, parameters);
        }
        protected virtual void ExecuteMethodNoHealth(string methodName, Action action, params object[] parameters)
        {
            base.ExecuteMethod(methodName, action, parameters);
        }
        #endregion
    }
}
