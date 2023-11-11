using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Rest
{
    public interface ITokenProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        string GetToken();
        void ResetToken();
        void AddClientCertificate(string certificateThumbprint , StoreName storeName, StoreLocation location);
    }
}
