using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Models
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class TokenResponseMessage
    {
        public string TokenType { get; set; }
        public string AccessToken { get; set; }
        public int ExpiresIn { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class RefreshTokenResponseMessage : TokenResponseMessage
    {
        public string RefreshToken { get; set; }
        public int RefreshTokenExpiresIn { get; set; }
    }
}
