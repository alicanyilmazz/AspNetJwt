using Infrastructure.Common;
using Infrastructure.Rest;
using Infrastructure.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Models
{
    public class HostClientBase : IDisposable
    {
        protected readonly Func<string> ApiUrl;
        protected readonly ITokenProvider TokenProvider;
        protected readonly ILogService Logger;

        private X509Certificate2 clientCertificate;
        private bool disposedValue;

        #region Constructor

        protected HostClientBase(ITokenProvider tokenProvider, Func<string> apiUrl, ILogService logger)
        {
            this.TokenProvider = tokenProvider;
            this.ApiUrl = apiUrl ?? (() => string.Empty);
            this.Logger = logger;
        }

        protected HostClientBase(Func<string> apiUrl) : this(null, apiUrl, null)
        {
        }

        protected HostClientBase(string apiUrl) : this(null, () => apiUrl, null)
        {
        }

        protected HostClientBase(ITokenProvider tokenProvider, Func<string> apiUrl) : this(tokenProvider, apiUrl, null)
        {
        }
        protected HostClientBase(Func<string> apiUrl, ILogService logger) : this(null, apiUrl, logger)
        {
        }

        #endregion

        protected RestResponse Send(string payloadJson, string verb, string endpoint, int timeoutInMilliseconds, bool throwTimeoutException, NameValueCollection headerFields, bool retryWhenUnauthorize = true)
        {
            string result = string.Empty;
            WebHeaderCollection header = null;

            using (var webClient = new RestWebClient(timeoutInMilliseconds, clientCertificate))
            {
                FillHeaders(webClient, headerFields);
                DebugLog($"Request Header: {webClient.Headers}");
                try
                {
                    switch (verb)
                    {
                        case "POST":
                            result = webClient.UploadString(endpoint, payloadJson);
                            break;
                        case "PUT":
                            result = webClient.UploadString(endpoint, WebRequestMethods.Http.Put, payloadJson);
                            break;
                        case "GET":
                            result = webClient.DownloadString(endpoint);
                            break;
                        default:
                            throw new NotImplementedException($"Verb {verb} is not implemented.");
                    }
                    header = webClient.ResponseHeaders;
                }
                catch (WebException we)
                {
                    if (we.Status == WebExceptionStatus.Timeout)
                    {
                        string timeoutMessage = $"Host request timed out for {endpoint}.";
                        ErrorLog(timeoutMessage, we);
                        if (throwTimeoutException)
                        {
                            throw new TimeoutException(timeoutMessage, we);
                        }
                        else
                        {
                            HandleException(null, we);
                        }

                    }
                    string errorResponse = GetErrorResponse(we);
                    DebugLog($"Error Response: {errorResponse}");

                    if (retryWhenUnauthorize)
                    {
                        var webResponse = we.Response as HttpWebResponse;
                        if (webResponse != null && webResponse.StatusCode == HttpStatusCode.Unauthorized && TokenProvider != null)
                        {
                            InfoLog("Authorization failed. Retrying host request with refresh token.");
                            TokenProvider.ResetToken();
                            return Send(payloadJson,verb,endpoint,timeoutInMilliseconds,throwTimeoutException,headerFields,false);
                        }
                    }

                    ErrorLog($"Host request failed for {endpoint}", we);
                    HandleException(errorResponse, we);
                }
                catch(Exception e) 
                {
                    throw new Exception("systemerror", e);
                }

                InfoLog($"Host request successful for {endpoint}");

                return new RestResponse
                {
                    Body = result,
                    Header = header,
                };
            }
        }

        public void AddClientCertificate(string certificateThumbprint , StoreName storeName , StoreLocation location)
        {
            try
            {
                Helper.FindCertificate(certificateThumbprint, storeName, location);
            }
            catch (Exception ex)
            {
                ErrorLog("Unable to initialize client certificate.", ex);
            }
        }

        private void FillHeaders(RestWebClient webClient, NameValueCollection headerFields)
        {
            if (TokenProvider != null)
            {
                var token = TokenProvider.GetToken();
                webClient.Headers["Authorization"] = token;
            }

            webClient.Headers["Content-type"] = "application/json";
            webClient.Encoding = Encoding.UTF8;

            if (headerFields != null)
            {
                foreach (var key in headerFields)
                {
                    var fieldId = key.ToString();
                    webClient.Headers[fieldId] = headerFields[fieldId];
                }
            }
        }

        #region Logging

        public virtual void DebugLog(string text)
        {
            Logger.Debug(text);
        }

        public virtual void ErrorLog(Exception e)
        {
            Logger.Error(e);
        }
        public virtual void ErrorLog(string text, Exception e)
        {
            Logger.Error(text, e);
        }
        public virtual void InfoLog(string text)
        {
            Logger.Info(text);
        }

        #endregion

        public virtual void HandleException(string response, WebException ex)
        {
            throw new WebException("systemerror", ex);
        }

        private string GetErrorResponse(WebException we)
        {
            string errorResponse = string.Empty;
            if (we.Response != null)
            {
                try
                {
                    var errorStream = we.Response.GetResponseStream();
                    using (StreamReader reader = new StreamReader(errorStream))
                    {
                        errorResponse = reader.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {

                    ErrorLog(ex);
                }
            }
            return errorResponse;
        }

        public void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing && TokenProvider is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
