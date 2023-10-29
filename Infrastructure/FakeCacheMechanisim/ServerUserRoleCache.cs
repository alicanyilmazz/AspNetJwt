using Infrastructure.Common;
using Infrastructure.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.FakeCacheMechanisim
{
    public class ServerUserRoleCache
    {
        public static (bool, List<string>) GetUserRoles(string userCode)
        {
            if (EnvironmentSettings.Environment.Equals("DEV"))
            {
                return (false, new List<string>());
            }
            var userRoles = new Dictionary<string, List<string>>()
            {
                {"testuser",new List<string>{"terminalmanager","servermanager"} },
                {"terminaluser",new List<string>{"terminalmanager"} },
                {"serveruser",new List<string>{"servermanager"} },
            };

            if (!string.IsNullOrEmpty(userCode) && userRoles.ContainsKey(userCode))
            {
                return (true, userRoles[userCode]);
            }

            return (false, new List<string>());
        }
    }
}
