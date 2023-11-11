using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class SafeUserSafeService
    {
        public static NetworkCredential getSecurityKey(string key)
        {
           if (string.IsNullOrEmpty(key))
               throw new ArgumentNullException(nameof(key));
           if (key.Equals("ServiceAppTokenSecret")) 
                return new NetworkCredential(userName:"SecretKey",password: "1q2w3e4r5t*1q2w3e4r5t*1q2w3e4r5t*1q2w3e4r5t*");
           return new NetworkCredential();
        }
    }
}
