using Codeable.Foundation.Common;
using Codeable.Foundation.Common.System;
using Stencil.Primary.Health;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Web.Controllers
{
    public abstract class HealthRestApiController : RestApiBaseController
    {
        public HealthRestApiController(IFoundation foundation, string trackPrefix)
            : base(foundation)
        {
            this.TrackPrefix = trackPrefix;
        }
        public HealthRestApiController(IFoundation foundation, IHandleExceptionProvider iHandleExceptionProvider, string trackPrefix)
            : base(foundation, iHandleExceptionProvider)
        {
            this.TrackPrefix = trackPrefix;
        }

        public virtual string TrackPrefix { get; set; }

        #region Health Monitoring

        protected override void ExecuteMethod(string methodName, Action action, params object[] parameters)
        {
            using (var scope = HealthReporter.BeginTrack(HealthTrackType.CountAndDurationAverage, string.Format(HealthReporter.RESTAPI_FORMAT, this.TrackPrefix + "." + methodName)))
            {
                base.ExecuteMethod(methodName, action, parameters);
            }
        }
        protected override K ExecuteFunction<K>(string methodName, Func<K> function, params object[] parameters)
        {
            using (var scope = HealthReporter.BeginTrack(HealthTrackType.CountAndDurationAverage, string.Format(HealthReporter.RESTAPI_FORMAT, this.TrackPrefix + "." + methodName)))
            {
                return base.ExecuteFunction<K>(methodName, function, parameters);
            }
        }
        protected override T ExecuteFunction<T>(string methodName, bool forceThrow, Func<T> function, params object[] parameters)
        {
            using (var scope = HealthReporter.BeginTrack(HealthTrackType.CountAndDurationAverage, string.Format(HealthReporter.RESTAPI_FORMAT, this.TrackPrefix + "." + methodName)))
            {
                return base.ExecuteFunction<T>(methodName, forceThrow, function, parameters);
            }
        }
        protected void ExecuteMethod(HealthTrackType type, string methodName, Action action, params object[] parameters)
        {
            using (var scope = HealthReporter.BeginTrack(type, string.Format(HealthReporter.RESTAPI_FORMAT, this.TrackPrefix + "." + methodName)))
            {
                base.ExecuteMethod(methodName, action, parameters);
            }
        }
        protected K ExecuteFunction<K>(HealthTrackType type, string methodName, Func<K> function, params object[] parameters)
        {
            using (var scope = HealthReporter.BeginTrack(type, string.Format(HealthReporter.RESTAPI_FORMAT, this.TrackPrefix + "." + methodName)))
            {
                return base.ExecuteFunction<K>(methodName, function, parameters);
            }
        }

        #endregion
    }
}
