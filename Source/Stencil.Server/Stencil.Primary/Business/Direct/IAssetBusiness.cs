using Stencil.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Business.Direct
{
    public partial interface IAssetBusiness
    {
        void UpdateResizeAttemptInfo(Guid asset_id, int attempt_count, DateTime attempt_utc, string resize_log);
        void UpdateResizeInfo(Guid asset_id, bool is_processing, string processor_status, string processor_log);
        Asset UpdateEncodingInfo(Guid asset_id, string encode_identifier, bool is_processing, string processor_status, string processor_log, bool incrementEncodingAttempt);


        List<Asset> GetPhotosForProcessing(string resize_status, bool include_empty_status, int maxRetries);
        List<Asset> GetVideosForProcessing(string encode_status, bool include_empty_status);
        List<Asset> GetVideosForRetrying(int attemptsLessThan, DateTime minimumAttemptTime);
        List<Asset> GetVideosFailedProcessingAfter(int attemptsGreaterThanOrEqualTo, DateTime dateTime);

    }
}
