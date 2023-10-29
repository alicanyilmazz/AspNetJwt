using Infrastructure.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.FakeCacheMechanisim
{
    public class ServerUserIpCache
    {
        public static (bool, List<string>) GetServerIp(string userCode)
        {
            if (EnvironmentSettings.Environment.Equals("DEV"))
            {
                return (false, new List<string>());
            }
            var ipList = new Dictionary<string, List<string>>()
            {
                {"testuser",new List<string>{"192.168.1.1", "192.168.1.2" } },
                {"terminaluser",new List<string>{ "192.168.1.3", "192.168.1.4","192.168.1.5" } },
                {"serveruser",new List<string>{ "192.168.1.6", "192.168.1.7" } },
            };

            if (!string.IsNullOrEmpty(userCode) && ipList.ContainsKey(userCode))
            {
                return (true, ipList[userCode]);
            }

            return (false, new List<string>());
        }
    }
}
