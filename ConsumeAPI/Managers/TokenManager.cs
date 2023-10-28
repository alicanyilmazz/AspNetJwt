using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Web;

namespace ConsumeAPI.Managers
{
    public class TokenManager
    {
        private static readonly string[] refreshRemoveClaims = new[]
        {
            JwtRegisteredClaimNames.Aud,
            JwtRegisteredClaimNames.Iss,
            JwtRegisteredClaimNames.Exp
        }

        public Token
    }
}