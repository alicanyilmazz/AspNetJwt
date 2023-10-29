using Infrastructure.Common;
using Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.FakeCacheMechanisim
{
    public class TokenSafeCodeCache
    {
        public static (bool, TokenSafeCode) GetSafeDefinition(string grantType)
        {
            var safeCodes = new Dictionary<string, TokenSafeCode>()
            {
                {"terminal_credentials",new TokenSafeCode(){ UsernameCode = "TERMINAL_USERNAME", PasswordCode = "TERMINAL_PASSWORD" } },
                {"server_credentials",new TokenSafeCode(){ UsernameCode = "SERVER_USERNAME", PasswordCode = "SERVER_PASSWORD" } },
            };

            if (!string.IsNullOrEmpty(grantType) && safeCodes.ContainsKey(grantType))
            {
                return (true, safeCodes[grantType]);
            }

            return (false, new TokenSafeCode());
        }
    }
}
