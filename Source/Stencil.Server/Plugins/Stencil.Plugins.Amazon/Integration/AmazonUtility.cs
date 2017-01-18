using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stencil.Plugins.Amazon.Integration
{
    public class AmazonUtility
    {
        public static string ConstructAmazonUrl(string amazonCloudFrontUrl, string amazonS3PublicUrl, string amazonS3Bucket, string filePathAndName)
        {
            if (string.IsNullOrEmpty(amazonCloudFrontUrl))
            {
                return string.Format("{0}/{1}/{2}", amazonS3PublicUrl.Trim('/'), amazonS3Bucket, filePathAndName);
            }
            else
            {
                return string.Format("{0}/{1}", amazonCloudFrontUrl.Trim('/'), filePathAndName);
            }
        }
    }
}