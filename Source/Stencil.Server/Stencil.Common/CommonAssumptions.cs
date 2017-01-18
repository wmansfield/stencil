using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Common
{
    public static class CommonAssumptions
    {
        public static readonly string APP_KEY_HEALTH_APIKEY = "Stencil-Health-ApiKey";
        public static readonly string APP_KEY_SQL_DB = "Stencil-SQL";
        public static readonly string APP_KEY_ES_INDEX = "Stencil-ES-INDEX";
        public static readonly string APP_KEY_ES_URL = "Stencil-ES-HOST";
        public static readonly string APP_KEY_ES_REPLICA = "Stencil-ES-REPLICA";
        public static readonly string APP_KEY_ES_SHARDS = "Stencil-ES-SHARDS";
        public static readonly string APP_KEY_DEBUG_QUERIES = "Stencil-ES-DEBUG";
        public static readonly string APP_KEY_IS_BACKING = "Stencil-IsBacking";
        public static readonly string APP_KEY_ENVIRONMENT = "Stencil-Environment";
        public static readonly string APP_KEY_IS_HYDRATE = "Stencil-IsHydrate";
        public static readonly string APP_KEY_AZUREPUSH_HUBNAME = "Stencil-PUSH-HUB";
        public static readonly string APP_KEY_AZUREPUSH_CONNECTION = "Stencil-PUSH-CONN";

        public static readonly string CONFIG_KEY_BACKING_URL = "Stencil-Backing-Url";

        public static readonly string CONFIG_KEY_AMAZON_CLOUDFRONT_URL = "AWS-{0}-Url-CloudFront";
        public static readonly string CONFIG_KEY_AMAZON_PUBLIC_URL = "AWS-{0}-Url-Public";
        public static readonly string CONFIG_KEY_AMAZON_KEY = "AWS-{0}-Access-Key";
        public static readonly string CONFIG_KEY_AMAZON_SECRET = "AWS-{0}-Access-Secret";
        public static readonly string CONFIG_KEY_AMAZON_BUCKET = "AWS-{0}-Bucket-Media";
        public static readonly string CONFIG_KEY_AMAZON_PIPELINE_ID = "AWS-{0}-Video-Pipeline";
        public static readonly string CONFIG_KEY_AMAZON_PRESET_ID = "AWS-{0}-Video-Preset";

        public static readonly string CONFIG_KEY_RESIZE_ATTEMPT_LIMIT = "AWS-ImageRetry-Max";
        public static readonly string CONFIG_KEY_AMAZON_ENCODE_RETRY_MAX = "AWS-Retry-Max";
        public static readonly string CONFIG_KEY_ENCODE_RETRY_WINDOW_MINUTES = "AWS-Retry-WindowMins";

        public static readonly int INDEX_RETRY_THRESHOLD_SECONDS = 5;
    }
}
