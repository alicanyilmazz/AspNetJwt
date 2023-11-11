using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Infrastructure.Common
{
    public class Helper
    {
        public static HttpResponseException GenerateUnAuthorizeException(string message, int code)
        {
            HttpResponseMessage unauthorizedHttpResponse = GenerateUnAuthorizedHttpMessage(message, code);
            return new HttpResponseException(unauthorizedHttpResponse);
        }
        public static HttpResponseMessage GenerateUnAuthorizedHttpMessage(string message, int code)
        {
            const string responseCode = "authorizationerror";
            var responseContent = JsonConvert.SerializeObject(new UnAuthorizedErrorMessage { Message = message, Code = code });
            var unauthorizedHttpResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                ReasonPhrase = responseCode,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            };
            //Logger.WarnFormat("authorizationerror {0}",message);
            return unauthorizedHttpResponse;
        }

        public static X509Certificate2 FindCertificate(string certificateThumbprint, StoreName storeName, StoreLocation location)
        {
            X509Certificate2 certificate = null;
            X509Store store = new X509Store(storeName, location);
            try
            {
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                X509Certificate2Collection certificates = store.Certificates.Find(X509FindType.FindByThumbprint, certificateThumbprint, false);
                if (certificates.Count > 0)
                {
                    certificate = certificates[0];
                }
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                store.Close();
            }
            return certificate;
        }

    }
}
