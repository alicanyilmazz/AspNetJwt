using Infrastructure.Common;
using Infrastructure.Controller.Options;
using Infrastructure.FakeCacheMechanisim;
using Infrastructure.FakeSafeMechanisim;
using Infrastructure.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Infrastructure.Controller.Manages
{
    public class TokenManager
    {
        private static readonly string[] refreshRemoveClaims = new[]
        {
            JwtRegisteredClaimNames.Aud,
            JwtRegisteredClaimNames.Iss,
            JwtRegisteredClaimNames.Exp
        };
        public TokenOptions Options { get; }
        public TokenManager(TokenOptions options)
        {
            Options = options;
        }

        public List<Claim> ValidateRefreshTokenAndGetClaims(string accessToken, string refreshToken)
        {
            var principal = GetPrincipalFromExpiredToken(accessToken);
            if (principal is null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            var refreshPrincipal = GetPrincipalFromRefreshToken(refreshToken);
            if (refreshPrincipal is null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            string refreshTyp = refreshPrincipal.Claims.First(x => x.Type == JwtRegisteredClaimNames.Typ).Value;
            if (refreshTyp != Options.RefreshTokenType)
            {
                throw GenerateUnauthorizedException("Given token is not valid refresh token.");
            }
            string jti = refreshPrincipal.Claims.First(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
            string relatedJti = refreshPrincipal.Claims.First(x => x.Type == "related_jti").Value;
            if (jti != relatedJti)
            {
                throw GenerateUnauthorizedException("Given refresh token is not authorized for access token.");
            }
            var claims = principal.Claims.ToList();
            ValidateAndIncreaseRefreshCount(principal, claims);
            claims.RemoveAll(x => refreshRemoveClaims.Contains(x.Type));
            return claims;

        }

        private void ValidateAndIncreaseRefreshCount(ClaimsPrincipal principal, List<Claim> claims)
        {
            string refreshCount = principal.Claims.FirstOrDefault(x => x.Type == "refresh_count").Value;
            if (refreshCount is null)
            {
                claims.Add(new Claim("refresh_count", "1"));
            }
            else if (int.Parse(refreshCount) >= Options.MaxRefreshCount)
            {
                throw GenerateUnauthorizedException("Refresh token is expired.");
            }
            else
            {
                claims.RemoveAll(x => x.Type == "refresh_count");
                claims.Add(new Claim("refresh_count", (int.Parse(refreshCount) + 1).ToString()));
            }
        }

        public List<Claim> GetUserClaims(string userId, List<Claim> additionalClaims)
        {
            var claims = new List<Claim>()
            {
                new Claim("user_id", userId ?? ""),
                new Claim("name","server")
            };

            additionalClaims.ForEach(x =>
            {
                if (!claims.Any(y => y.Type == x.Type))
                    claims.Add(x);
            });

            (bool success, List<string> userRoles) = ServerUserRoleCache.GetUserRoles(userId);

            if (success)
            {
                userRoles.ForEach(x => claims.Add(new Claim(ClaimTypes.Role, x)));
            }

            return claims;
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = Options.GenerateBaseValidationParameters();
            tokenValidationParameters.ValidateLifetime = false;
            tokenValidationParameters.ValidateIssuerSigningKey = true;

            return GetPrincipalFromRefreshToken(token, tokenValidationParameters);
        }

        public ClaimsPrincipal GetPrincipalFromRefreshToken(string token)
        {
            var tokenValidationParameters = Options.GenerateBaseValidationParameters();
            tokenValidationParameters.ValidateIssuerSigningKey = true;
            return GetPrincipalFromRefreshToken(token, tokenValidationParameters);
        }
        public ClaimsPrincipal GetPrincipalFromRefreshToken(string token, TokenValidationParameters tokenValidationParameters)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (!(securityToken is JwtSecurityToken jwtSecurityToken) || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid Token");
            }
            return principal;
        }

        public void ValidateUser(string username, string password, string grantType)
        {
            (bool success, List<string> ipList) = ServerUserIpCache.GetServerIp(username);
            var clientIp = HttpContext.Current.Request.UserHostAddress;
            if (success && !ipList.Contains(clientIp))
            {
                throw GenerateUnauthorizedException($"Authorization failed for ip {clientIp}");
            }

            if (string.IsNullOrEmpty(grantType))
            {
                throw GenerateUnauthorizedException("Grant type can not be null or empty");
            }

            (bool isSuccessSafeDefinition, TokenSafeCode userRoles) = TokenSafeCodeCache.GetSafeDefinition(grantType);

            if (!isSuccessSafeDefinition)
            {
                throw GenerateUnauthorizedException($"Invalid grant type is : {clientIp}");
            }

            (bool isSafeExist, TokenSafeCode tokenSafeCode) = TokenSafeCodeCache.GetSafeDefinition(grantType);

            if (!isSafeExist)
            {
                throw GenerateUnauthorizedException($"Invalid grant type {grantType} for safe definitions");
            }

            (bool isSuccessSafe, NetworkCredential safeInformations) = SecretManager.GetUserCredentials(tokenSafeCode);

            if (!isSuccessSafe)
            {
                throw GenerateUnauthorizedException($"Invalid grant type is : {clientIp}");
            }

            var usernameInformation = safeInformations.UserName;
            var passwordInformation = safeInformations.Password;
            if (username != usernameInformation || password != passwordInformation)
            {
                throw GenerateUnauthorizedException($"Authorization failed.");
            }
        }

        public RefreshTokenResponseMessage GenerateToken(List<Claim> claims)
        {
            var tokenValidationParameters = Options.GenerateBaseValidationParameters();
            var securityKey = new SymmetricSecurityKey(Options.SecurityKey);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            string jti = Guid.NewGuid().ToString();
            var permClaims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Jti,jti),
                new Claim(JwtRegisteredClaimNames.Typ,Options.TokenTyp),
            };

            claims.ForEach(x =>
            {
                if (!permClaims.Any(y => y.Type == x.Type))
                    permClaims.Add(x);
            });

            var token = new JwtSecurityToken(tokenValidationParameters.ValidIssuer,
                tokenValidationParameters.ValidAudience,
                permClaims, 
                expires: DateTime.Now.AddSeconds(Options.TokenExpireTime),
                signingCredentials: credentials);
            
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            var tokenResponse = new RefreshTokenResponseMessage
            {
                TokenType = Options.TokenTyp,
                AccessToken = jwt,
                ExpiresIn = Options.TokenExpireTime
            };

            if (Options.IsRefreshTokenEnabled)
            {
                (string refreshJwt, int refreshExpireTimeInSeconds) = GenerateRefreshToken(credentials, jti);
                tokenResponse.RefreshToken = refreshJwt;
                tokenResponse.RefreshTokenExpiresIn = refreshExpireTimeInSeconds;
            }
            return tokenResponse;
        }

        private (string , int) GenerateRefreshToken(SigningCredentials credentials , string tokenJti)
        {
            var tokenValidationParameters = Options.GenerateBaseValidationParameters();

            string jti = Guid.NewGuid().ToString();
            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Jti,jti),
                new Claim(JwtRegisteredClaimNames.Typ,Options.RefreshTokenType),
                new Claim("related_jti",tokenJti),
            };

            var token = new JwtSecurityToken(tokenValidationParameters.ValidIssuer,
                tokenValidationParameters.ValidAudience,
                claims,
                expires: DateTime.Now.AddSeconds(Options.RefreshTokenExpireTime),
                signingCredentials: credentials
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken (token);

            return (jwt, Options.RefreshTokenExpireTime);
        }

        public HttpResponseException GenerateUnauthorizedException(string message)
        {
            return Helper.GenerateUnAuthorizeException(message, -1);
        }
        public HttpResponseException GenerateUnauthorizedException(string message , int code)
        {
            return Helper.GenerateUnAuthorizeException(message, code);
        }

    }
}
