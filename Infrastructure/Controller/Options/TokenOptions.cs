using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Controller.Options
{
    public class TokenOptions
    {
        private const int DEFAULT_TOKEN_EXPIRE_TIME = 3600;
        private const int DEFAULT_REFRESH_TOKEN_EXPIRE_TIME = 3900;
        private const int DEFAULT_MAX_REFRESH_COUNT = 5;

        private readonly Func<TokenValidationParameters> validationParametersFunc;
        private readonly NetworkCredential securityKey;
        private readonly string maxRefreshCountSettingCode;
        private readonly string useRefreshTokenSettingCode;
        private readonly string tokenExpireSettingCode;
        private readonly string refreshTokenExpireSettingCode;

        public TokenOptions(Func<TokenValidationParameters> validationParametersFunc, string securitySettingCode, string maxRefreshCountSettingCode, string useRefreshTokenSettingCode, string tokenExpireSettingCode, string refreshTokenExpireSettingCode)
        {
            this.validationParametersFunc = validationParametersFunc;
            this.maxRefreshCountSettingCode = maxRefreshCountSettingCode;
            this.useRefreshTokenSettingCode = useRefreshTokenSettingCode;
            this.tokenExpireSettingCode = tokenExpireSettingCode;
            this.refreshTokenExpireSettingCode = refreshTokenExpireSettingCode;
            securityKey = SafeUserSafeService.getSecurityKey(securitySettingCode);
        }

        public string TokenTyp { get; set; } = "Bearer";
        public string RefreshTokenType { get; set; } = "Refresh";

        public bool IsRefreshTokenEnabled => HostConfiguration.GetSetting(useRefreshTokenSettingCode, "A") == "A";
        public int MaxRefreshCount => HostConfiguration.GetSetting(maxRefreshCountSettingCode, DEFAULT_MAX_REFRESH_COUNT);
        public int TokenExpireTime => HostConfiguration.GetSetting(tokenExpireSettingCode, DEFAULT_TOKEN_EXPIRE_TIME);
        public int RefreshTokenExpireTime => HostConfiguration.GetSetting(refreshTokenExpireSettingCode, DEFAULT_REFRESH_TOKEN_EXPIRE_TIME);
        public byte[] SecurityKey => Encoding.UTF8.GetBytes(securityKey.Password);
        public TokenValidationParameters GenerateBaseValidationParameters()
        {
            var parameters = validationParametersFunc();
            parameters.IssuerSigningKey = new SymmetricSecurityKey(SecurityKey);
            return parameters;
        }

    }
}
