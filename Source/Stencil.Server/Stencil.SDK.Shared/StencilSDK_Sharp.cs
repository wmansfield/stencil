#if !WEB
using Newtonsoft.Json;
using RestSharp;
#if WINDOWS_PHONE_APP
using RestSharp.Portable;
#endif
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Stencil.SDK.Exceptions;


namespace Stencil.SDK
{

    public partial class StencilSDK
    {
        #region Public Methods

        public IRestResponse Execute(RestRequest request)
        {
            RestClient client = new RestClient();

            this.PrepareRequest(client, request);

#if WINDOWS_PHONE_APP
            IRestResponse response = client.ExecuteSynchronous(request);
#else
            IRestResponse response = client.Execute(request);
#endif

            this.ValidateResponse(response);

            return response;
        }
        public T Execute<T>(RestRequest request)
            where T : new()
        {
            RestClient client = new RestClient();

            this.PrepareRequest(client, request);

#if WINDOWS_PHONE_APP
            IRestResponse<T> response = client.ExecuteSynchronous<T>(request);
#else
            IRestResponse<T> response = client.Execute<T>(request);
#endif

            this.ValidateResponse(response);

            return response.Data;
        }

        public async Task<IRestResponse> ExecuteAsync(RestRequest request)
        {
            return await ExecuteAsync(request, this.AsyncTimeoutMillisecond);
        }
        public async Task<IRestResponse> ExecuteAsync(RestRequest request, int milliSecondTimeout)
        {
            return await Task.Factory.StartNew(() =>
            {
                if (milliSecondTimeout <= 0)
                {
                    milliSecondTimeout = this.AsyncTimeoutMillisecond;
                }
                Task<IRestResponse> task = ExecuteAsyncInternal(request);
                bool completed = task.Wait(milliSecondTimeout);
                if (completed)
                {
                    return task.Result;
                }
                throw new EndpointTimeoutException(System.Net.HttpStatusCode.GatewayTimeout, "Error communicating with server, connection timed out.");
            });
        }

        public async Task<T> ExecuteAsync<T>(RestRequest request)
            where T : new()
        {
            return await ExecuteAsync<T>(request, this.AsyncTimeoutMillisecond);
        }
        public async Task<T> ExecuteAsync<T>(RestRequest request, int milliSecondTimeout)
            where T : new()
        {
            return await Task.Factory.StartNew(() =>
            {
                if (milliSecondTimeout <= 0)
                {
                    milliSecondTimeout = this.AsyncTimeoutMillisecond;
                }
                Task<T> task = ExecuteAsyncInternal<T>(request);
                bool completed = task.Wait(milliSecondTimeout);
                if (completed)
                {
                    return task.Result;
                }
                throw new EndpointTimeoutException(System.Net.HttpStatusCode.GatewayTimeout, "Error communicating with server, connection timed out.");
            });
        }

        #endregion

        #region Protected Methods

        protected virtual void PrepareRequest(RestClient client, RestRequest request)
        {
            client.BaseUrl = new Uri(BaseUrl);

#if !WINDOWS_PHONE_APP
            request.RequestFormat = DataFormat.Json;
            request.JsonSerializer = new Stencil.SDK.Serialization.NewtonSoftSerializer();
#endif

            this.AddAuthorizationHeaders(client, request);
            this.AddCustomHeaders(client, request);
        }
        protected virtual async Task<IRestResponse> ExecuteAsyncInternal(RestRequest request)
        {
            RestClient client = new RestClient();

            this.PrepareRequest(client, request);

            IRestResponse response = await client.ExecuteTaskAsync(request);

            this.ValidateResponse(response);

            return response;
        }
        protected virtual async Task<T> ExecuteAsyncInternal<T>(RestRequest request)
            where T : new()
        {
            RestClient client = new RestClient();
#if WINDOWS_PHONE_APP
            client.IgnoreResponseStatusCode = true;
#endif
            this.PrepareRequest(client, request);

#if DEBUG && __MOBILE__
            Console.WriteLine("------> Calling Server: " + request.Resource);
#endif

            IRestResponse response = await client.ExecuteTaskAsync(request);

            this.ValidateResponse(response);
#if WINDOWS_PHONE_APP
            string content = response.GetContent();
#else
            string content = response.Content;
#endif
            return JsonConvert.DeserializeObject<T>(content);
        }


        protected virtual void AddCustomHeaders(RestClient client, RestRequest request)
        {
            if (this.CustomHeaders != null)
            {
                foreach (var item in this.CustomHeaders)
                {
                    if (!string.IsNullOrEmpty(item.Key))
                    {
                        client.AddDefaultHeader(item.Key, item.Value);
                    }
                }
            }
        }
        protected virtual void AddAuthorizationHeaders(RestClient client, RestRequest request)
        {
            List<KeyValuePair<string, string>> authHeaders = this.GenerateAuthorizationHeader();
            foreach (var item in authHeaders)
            {
                client.AddDefaultHeader(item.Key, item.Value);
            }
            if (!string.IsNullOrEmpty(this.ApplicationKey) && !string.IsNullOrEmpty(this.ApplicationSecret))
            {

                client.AddDefaultHeader(API_PARAM_SIG, this.SignatureGenerator.CreateSignature(this.ApplicationKey, this.ApplicationSecret));
            }
        }
        public virtual List<KeyValuePair<string, string>> GenerateAuthorizationHeader()
        {
            List<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();
            if (!string.IsNullOrEmpty(this.ApplicationKey) && !string.IsNullOrEmpty(this.ApplicationSecret))
            {
                headers.Add(new KeyValuePair<string, string>(API_PARAM_KEY, this.ApplicationKey));
                headers.Add(new KeyValuePair<string, string>(API_PARAM_SIG, this.SignatureGenerator.CreateSignature(this.ApplicationKey, this.ApplicationSecret)));
            }
            return headers;
        }
        protected virtual void ValidateResponse(IRestResponse response)
        {
            switch (response.StatusCode)
            {

                case System.Net.HttpStatusCode.Continue:
                case System.Net.HttpStatusCode.Accepted:
                case System.Net.HttpStatusCode.Created:
                case System.Net.HttpStatusCode.NoContent:
                case System.Net.HttpStatusCode.NotModified:
                case System.Net.HttpStatusCode.OK:
                    // do nothing
                    break;
                default:
                    throw new EndpointException(response.StatusCode, response.StatusDescription);
            }
#if !WINDOWS_PHONE_APP
            if (response.ErrorException != null)
            {
                throw new ApplicationException("Error retrieving response.  Check inner details for more info.", response.ErrorException);
            }
#endif
        }

        #endregion

    }
}
#endif