using DuoCode.Runtime;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.SDK
{
    public partial class StencilSDK
    {
        public Task<T> ExecuteAsync<T>(RestRequest request)
        {
            dynamic sdkBridge = Js.referenceAs<dynamic>(WebAssumptions.JS_BRIDGE_NAME);
            object jsonBody = null;
            List<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();
            List<KeyValuePair<string, object>> parameters = null;

            // body
            if (request.JsonBody != null)
            {
                jsonBody = request.GetData();
            }

            //parameters
            if (request.Parameters.Values.Count > 0)
            {
                parameters = new List<KeyValuePair<string, object>>();
                foreach (var item in request.Parameters)
                {
                    parameters.Add(item);
                }
            }

            // custom headers
            if (this.CustomHeaders != null)
            {
                foreach (var item in this.CustomHeaders)
                {
                    headers.Add(item);
                }
            }

            // auth headers
            if (!string.IsNullOrEmpty(this.ApplicationKey) && !string.IsNullOrEmpty(this.ApplicationSecret))
            {
                headers.Add(new KeyValuePair<string, string>(API_PARAM_KEY, this.ApplicationKey));
                headers.Add(new KeyValuePair<string, string>(API_PARAM_SIG, this.SignatureGenerator.CreateSignature(this.ApplicationKey, this.ApplicationSecret)));
            }

            return sdkBridge.executeAsync(this.BaseUrl, request.GetResource(), request.Method.ToString().ToUpper(), jsonBody, parameters, headers, request.ContentType);
        }

    }
}
