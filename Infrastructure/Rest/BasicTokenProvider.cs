using Infrastructure.Common;
using Infrastructure.FakeSafeMechanisim;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Rest
{
    public class BasicTokenProvider : ITokenProvider, IDisposable
    {
        protected const int EXPIRE_THRESHOLD_IN_SECONDS = 60;
        protected const string CONTENT_TYPE_JSON = "application/json";
        private const int DEFAULT_TIMEOUT = 60000;

        private readonly Func<string> endpoint;
        private readonly SemaphoreSlim cacheLock;
        private readonly string cacheName = Guid.NewGuid().ToString();

        protected readonly MemoryCache tokenCache;
        protected readonly ILogService logger;
        protected bool disposedValue;
        private X509Certificate2 clientCertificate;

        private readonly string contentType;
        protected NetworkCredential Credentials;

        public Func<object> GetRequestBody { get; set; }
        public Func<int> Timeout { get; set; } = () => DEFAULT_TIMEOUT;

        public BasicTokenProvider(Func<string> endpoint, string contentType, string username, string password, ILogService logger)
        {
            this.endpoint = endpoint ?? (()=> string.Empty);
            this.contentType = contentType;
            this.Credentials = new NetworkCredential(username, SecretManager.ToSecureString(password));
            this.logger = logger;
            this.tokenCache = new MemoryCache("TokenCache");
            this.cacheLock = new SemaphoreSlim(1);
        }
        public BasicTokenProvider(Func<string> endpoint, string username, string password, ILogService logger) : this(endpoint, CONTENT_TYPE_JSON, username, password, logger)
        {

        }
        public BasicTokenProvider(Func<string> endpoint, string username, string password) : this(endpoint, CONTENT_TYPE_JSON, username, password, null)
        {

        }
        public void SetCredentials(string username, string password)
        {
            this.Credentials = new NetworkCredential(username, SecretManager.ToSecureString(password));
        }
        public string GetToken()
        {
            if (tokenCache.Contains(cacheName))
            {
                logger.Debug($"Cached token found for {endpoint()}");
                return tokenCache.Get(cacheName) as string;
            }

            cacheLock.Wait();

            try
            {
                if (tokenCache.Contains(cacheName))
                {
                    logger.Debug($"Cached token found for {endpoint()}");
                    return tokenCache.Get(cacheName) as string;
                }

                string result = FetchToken();

                logger.Debug($"Token response : {result}");

                dynamic tokenResponse = JsonConvert.DeserializeObject(result);
                string token = HandleResponse(tokenResponse);

                return token;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return null;
            }
            finally
            {
                cacheLock.Release();
            }
        }

        public virtual string FetchToken()
        {
            CheckUsernameAndPassword();

            return Fetch(endpoint(), GetRequestBody, FillHeaders);

        }

        private void CheckUsernameAndPassword()
        {
            if (string.IsNullOrEmpty(Credentials.UserName) || string.IsNullOrEmpty(Credentials.Password))
            {
                logger.Error("Username or password not found in token provider.");
                throw new InvalidOperationException("Username and password must be set to retrieve token.");
            }
        }

        protected string Fetch(string endpoint, Func<object> bodyFunc, Action<RestWebClient> headerFunc)
        {
            string result = string.Empty;
            using (var webClient = new RestWebClient(Timeout(), clientCertificate))
            {
                logger.Info($"Token provider is retrieving for {endpoint}");

                webClient.Headers["Content-type"] = contentType;
                webClient.Encoding = Encoding.UTF8;
                webClient.Headers["Cache-Control"] = "no-cache";
                headerFunc.Invoke(webClient);

                try
                {
                    string body = bodyFunc != null ? (contentType == CONTENT_TYPE_JSON ? JsonConvert.SerializeObject(bodyFunc()) : (string)bodyFunc()) : string.Empty;
                    result = webClient.UploadString(endpoint, body);
                }
                catch (WebException e)
                {
                    if (e.Status == WebExceptionStatus.Timeout)
                    {
                        string timeoutMessage = $"Token request timed out for {endpoint}";
                        logger.Error(timeoutMessage, e);
                        throw new TimeoutException(timeoutMessage, e);
                    }
                    logger.Error($"Token request failed for {endpoint}", e);
                    string errorResponse = GetErrorResponse(e);
                    logger.Debug(errorResponse);

                    HandleException(errorResponse, e);
                }
                logger.Info($"Token sucessfully retrieved for {endpoint}");
            }
            return result;
        }

        private void FillHeaders(RestWebClient webClient)
        {
            webClient.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Credentials.UserName}:{Credentials.Password}"));
        }

        private string GetErrorResponse(WebException webException)
        {
            string errorResponse = string.Empty;
            if (webException.Response != null)
            {
                try
                {
                    var errorStream = webException.Response.GetResponseStream();
                    using (StreamReader reader = new StreamReader(errorStream))
                    {
                        errorResponse = reader.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }
            return errorResponse;
        }

        public virtual void HandleException(string response, WebException e)
        {
            throw e;
        }

        protected virtual string HandleResponse(dynamic tokenResponse)
        {
            string token = $"{tokenResponse.token_type} {tokenResponse.access_token}";
            int expiresIn = tokenResponse.expires_in;
            if (EXPIRE_THRESHOLD_IN_SECONDS > expiresIn)
            {
                expiresIn -= EXPIRE_THRESHOLD_IN_SECONDS;
            }
            var absoluteExpireTime = DateTime.Now.AddSeconds(expiresIn);
            tokenCache.Set(cacheName, token, absoluteExpireTime);
            return token;
        }

        public void AddClientCertificate(string certificateThumbprint, StoreName storeName, StoreLocation location)
        {
            try
            {
                clientCertificate = Helper.FindCertificate(certificateThumbprint, storeName, location);
            }
            catch (Exception ex)
            {
                logger.Error("Unable to initialize client certificate", ex);
            }
        }

        public virtual void ResetToken()
        {
            if (tokenCache.Contains(cacheName))
            {
                tokenCache.Remove(cacheName);
            }
        }

        public virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    tokenCache.Dispose();
                    cacheLock.Dispose();
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
