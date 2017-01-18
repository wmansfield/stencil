using Stencil.SDK.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stencil.SDK
{
    public partial class StencilSDK
    {
        #region Constructor

        public StencilSDK(string baseUrl)
            : this(string.Empty, string.Empty, baseUrl)
        {
        }
        public StencilSDK(string applicationKey, string applicationSecret)
            : this(applicationKey, applicationSecret, API_BASE_URL)
        {
        }

        public StencilSDK(StencilAuthInfo authInfo)
            : this(authInfo.ApiKey, authInfo.ApiSecret, API_BASE_URL)
        {

        }
        public StencilSDK(StencilAuthInfo authInfo, string baseUrl)
            : this(authInfo.ApiKey, authInfo.ApiSecret, baseUrl)
        {

        }
        public StencilSDK(string applicationKey, string applicationSecret, string baseUrl)
        {
            this.CustomHeaders = new List<KeyValuePair<string, string>>();
            this.SignatureGenerator = new HashedTimeSignatureGenerator();

            this.AsyncTimeoutMillisecond = (int)TimeSpan.FromSeconds(40).TotalMilliseconds;
            this.ApplicationKey = applicationKey;
            this.ApplicationSecret = applicationSecret;
            if (baseUrl == null)
            {
                baseUrl = API_BASE_URL;
            }
            this.BaseUrl = baseUrl;

            this.InstanceCache = new Dictionary<string, object>();

            this.ConstructCoreEndpoints();
            this.ConstructManualEndpoints();
        }

        #endregion

        #region Constants

        public const string API_BASE_URL = "https://stencil.socialhaven.com/api";
        protected const string API_PARAM_KEY = "api_key";
        protected const string API_PARAM_SIG = "api_signature";

        #endregion

        #region Properties

        public int AsyncTimeoutMillisecond; // member for web ease
        public string BaseUrl; // member for web ease
        public string ApplicationKey; // member for web ease
        public string ApplicationSecret; // member for web ease
        /// <summary>
        /// adds the headers to every request
        /// </summary>
        public List<KeyValuePair<string, string>> CustomHeaders { get; set; }

        protected HashedTimeSignatureGenerator SignatureGenerator { get; set; }
        protected internal Dictionary<string, object> InstanceCache { get; set; }

        #endregion

    }
}
