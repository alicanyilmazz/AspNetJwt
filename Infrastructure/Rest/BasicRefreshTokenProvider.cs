using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Rest
{
    public class BasicRefreshTokenProvider : BasicTokenProvider
    {
        private readonly string refreshCacheName = Guid.NewGuid().ToString();
        private readonly Func<string> refreshEndpoint;
        public BasicRefreshTokenProvider(Func<string> endpoint, Func<string> refreshEndpoint, string username, string password) : base(endpoint, username, password)
        {
            this.refreshEndpoint = refreshEndpoint;
        }
        public BasicRefreshTokenProvider(Func<string> endpoint, Func<string> refreshEndpoint, string username, string password, ILogService logger) : base(endpoint, username, password, logger)
        {
            this.refreshEndpoint = refreshEndpoint ?? (() => string.Empty);
        }

        public override string FetchToken()
        {
            if (tokenCache.Contains(refreshCacheName))
            {
                var tokenResponse = tokenCache.Get(refreshCacheName) as dynamic;
                return Fetch(refreshEndpoint(), () => new
                {
                    AccessToken = tokenResponse.access_token,
                    RefreshToken = tokenResponse.refresh_token,
                }, null);
            }
            else
            {
                return base.FetchToken();
            }
        }
        protected override string HandleResponse(dynamic tokenResponse)
        {
            if (!string.IsNullOrEmpty((string)tokenResponse.refresh_token))
            {
                int expiresIn = tokenResponse.refresh_token_expires_in;
                if (EXPIRE_THRESHOLD_IN_SECONDS > expiresIn)
                {
                    expiresIn -= EXPIRE_THRESHOLD_IN_SECONDS;
                }
                var absoluteExpireTime = DateTime.Now.AddSeconds(expiresIn);
                tokenCache.Set(refreshCacheName, tokenResponse, absoluteExpireTime);
            }
            return base.HandleResponse((object)tokenResponse);
        }

        public override void ResetToken()
        {
            if (tokenCache.Contains(refreshCacheName))
            {
                tokenCache.Remove(refreshCacheName);
            }
            base.ResetToken();
        }

        public override void Dispose(bool disposing)
        {
            if (!disposedValue && disposing)
            {
                tokenCache.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
