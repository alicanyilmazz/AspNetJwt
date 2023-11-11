using AuthorizationApp.Core;
using Infrastructure;
using Infrastructure.Controller;
using Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AuthorizationApp.Controllers
{
    [AllowAnonymous]
    public class AuthorizationController : TokenController<CoreTokenRequestMessage>
    {
        [Route("api/token")]
        [HttpPost]
        public async Task<RefreshTokenResponseMessage> GetToken()
        {
            string authType = HostConfiguration.GetSetting("HostCoreAuthenticationType", "basic");
            return await GetTokenAsync(authType);
        }

        [Route("api/refresh_token")]
        [HttpPost]
        public RefreshTokenResponseMessage GetRefreshToken(RefreshTokenRequestMessage tokenRequest)
        {
            return GetRefreshToken(tokenRequest.AccessToken,tokenRequest.RefreshToken);
        }
    }
}
