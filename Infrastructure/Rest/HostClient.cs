using Infrastructure.Models;
using Infrastructure.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Infrastructure.Rest
{
    public class HostClient<RequestMessage, ResponseMessage> : HostClientBase, IHostClient<RequestMessage, ResponseMessage>
    {
        #region Constructors
        public HostClient(ITokenProvider tokenProvider, Func<string> apiUrl, ILogService logger) : base(tokenProvider, apiUrl, logger)
        {
        }

        public HostClient(Func<string> apiUrl) : base(apiUrl)
        {
        }

        public HostClient(string apiUrl) : base(apiUrl)
        {
        }

        public HostClient(ITokenProvider tokenProvider, Func<string> apiUrl) : base(tokenProvider, apiUrl)
        {
        }

        public HostClient(Func<string> apiUrl, ILogService logger) : base(apiUrl, logger)
        {
        }

        #endregion


        public C Get<C>(NameValueCollection queryStringFields, string method, int timeoutInMilliseconds) where C : ResponseMessage
        {
            string baseUrl = ApiUrl();
            string endpoint = string.IsNullOrEmpty(baseUrl) ? baseUrl : $"{baseUrl}/{method}";
            endpoint = string.Format("{0}{1}", endpoint, PrepareQueryString(queryStringFields));
            return Send<C>(default, "GET", endpoint, timeoutInMilliseconds).Body;
        }

        public C Post<C>(RequestMessage message, string method, int timeoutInMilliseconds) where C : ResponseMessage
        {
            string baseUrl = ApiUrl();
            string endpoint = string.IsNullOrEmpty(baseUrl) ? baseUrl : $"{baseUrl}/{method}";
            return Send<C>(message, "POST", endpoint, timeoutInMilliseconds).Body;
        }

        public RestResponse<C> Send<C>(RequestMessage message, string verb, string endpoint, int timeoutInMilliseconds, bool throwTimeoutException = true, NameValueCollection headerFields = null, JsonSerializerSettings jsonSeralizeSettings = null)
        {
            InfoLog($"Host request is starting for {endpoint} ({verb})");

            jsonSeralizeSettings = jsonSeralizeSettings ?? new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
            };
            string payloadJson = null;
            if (!Equals(message, default(RequestMessage)))
            {
                payloadJson = JsonConvert.SerializeObject(message, jsonSeralizeSettings);
                if (Logger.IsDebugEnabled())
                {
                    DebugLog($"Host Request: {payloadJson}");
                }
            }

            var response = Send(payloadJson, verb, endpoint, timeoutInMilliseconds, throwTimeoutException, headerFields);

            var body = JsonConvert.DeserializeObject<C>(response.Body);
            if (Logger.IsDebugEnabled())
            {
                DebugLog($"Host Header: {response.Header} Response {response}");
            }
            return new RestResponse<C> { Header = response.Header, Body = body };
        }

        public string PrepareQueryString(NameValueCollection queryStringFields)
        {
            if (queryStringFields == null || queryStringFields.Count == 0)
            {
                return string.Empty;
            }
            var qs = string.Join("&", queryStringFields.AllKeys
                .SelectMany(key => queryStringFields.GetValues(key)
                .Select(value => String.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value)))).ToArray());

            return string.Format("?{0}", qs);
        }
    }
}
