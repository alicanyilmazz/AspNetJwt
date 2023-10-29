using Infrastructure.Controller.Manages;
using Infrastructure.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Infrastructure.Controller
{
    public class TokenController<T> : ApiController where T : BasicTokenRequestMessage
    {
        private readonly TokenManager manager = ServiceLocator.Create<TokenManager>();

        protected string UserId = string.Empty;
        protected T TokenRequest;

        protected async Task<RefreshTokenResponseMessage> GetTokenAsync(string authenticationType)
        {
            switch (authenticationType)
            {
                case "basic":
                    await CheckBasicAuthAsync();
                    break;
                default:
                    break;
            }

            List<Claim> claims = manager.GetUserClaims(UserId, GetClaims());
            
            return manager.GenerateToken(claims);
        }

        protected RefreshTokenResponseMessage GetRefreshToken(string accessToken , string refreshToken)
        {
            if (!manager.Options.IsRefreshTokenEnabled)
            {
                throw manager.GenerateUnauthorizedException("Refresh token is not allowed.");
            }

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            List<Claim> claims = manager.ValidateRefreshTokenAndGetClaims(accessToken, refreshToken);

            return manager.GenerateToken(claims);
        }

        private async Task CheckBasicAuthAsync()
        {
            (string username, string password) = ValidateRequestAndGetBasicCredentials();
            var jsonRequest = await Request.Content.ReadAsStringAsync();
            TokenRequest = JsonConvert.DeserializeObject<T>(jsonRequest);
            manager.ValidateUser(username, password, TokenRequest.GrantType);
            UserId = username;
        }
        protected (string,string) ValidateRequestAndGetBasicCredentials()
        {
            var authorizationHeader = Request.Headers.Authorization;
            if (authorizationHeader is null)
            {
                throw manager.GenerateUnauthorizedException("Authorization header is empty.");
            }

            if (authorizationHeader.Scheme != "Basic")
            {
                throw manager.GenerateUnauthorizedException("Authorization scheme is not valid.");
            }

            if (string.IsNullOrEmpty(authorizationHeader.Parameter))
            {
                throw manager.GenerateUnauthorizedException("Authorization credential is not valid.");
            }

            var decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(authorizationHeader.Parameter));

            if (string.IsNullOrEmpty(decodedCredentials))
            {
                throw manager.GenerateUnauthorizedException("Authorization credential is not valid.");
            }

            var credentials = decodedCredentials.Split(':');

            if (credentials.Length != 2)
            {
                throw manager.GenerateUnauthorizedException("Authorization credential is not valid.");
            }

            return (credentials[0], credentials[1]);
        }
        protected virtual List<Claim> GetClaims()
        {
            return new List<Claim>();
        }
    }
}
