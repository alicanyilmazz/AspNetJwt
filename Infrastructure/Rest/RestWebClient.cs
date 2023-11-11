using System;
using System.Configuration;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Infrastructure.Rest
{
    public class RestWebClient : WebClient
    {
        private const int DEFAULT_TIMEOUT = 60000;

        /// <summary>
        /// Time in milliseconds
        /// </summary>
        private readonly int timeout;
        private readonly X509Certificate2 clientCertificate;

        public RestWebClient(int timeoutInMilliseconds)
        {
            timeout = timeoutInMilliseconds;
            if (ConfigurationManager.AppSettings["IgnoreCertificates"] == "TRUE")
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            }
        }
        public RestWebClient() : this(DEFAULT_TIMEOUT)
        {

        }

        public RestWebClient(int timeoutInMilliseconds, X509Certificate2 clientCertificate) : this(timeoutInMilliseconds)
        {
            this.clientCertificate = clientCertificate;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = (HttpWebRequest)base.GetWebRequest(address);
            if (request != null)
            {
                request.Timeout = timeout;
                if (clientCertificate != null)
                {
                    request.ClientCertificates.Add(clientCertificate);
                }
            }
            return request;
        }
    }
}
