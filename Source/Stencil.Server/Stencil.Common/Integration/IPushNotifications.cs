using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Common.Integration
{
    public interface IPushNotifications
    {
        Task RegisterApple(Guid account_id, string deviceToken);
        Task RegisterGoogle(Guid account_id, string deviceToken);

        /// <summary>
        /// Generic alert, no special processing
        /// </summary>
        void NotifyGenericAlert(List<Guid> account_ids, string message, string ignoreGroup);

        /// <summary>
        /// Sends badge only
        /// </summary>
        void NotifyBadge(List<Tuple<Guid, int>> account_id_badges);

        /// <summary>
        /// Sample App-Specific push notification
        /// </summary>
        void NotifySpecificAlertSample(List<Guid> account_ids, Guid route_id, string userName, string message);
    }
}
