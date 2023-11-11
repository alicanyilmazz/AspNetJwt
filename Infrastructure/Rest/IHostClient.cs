using Infrastructure.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Rest
{
    public interface IHostClient<RequestMessage, in ResponseMessage>
    {
        C Get<C>(NameValueCollection queryStringFields, string method, int timeoutInMilliseconds) where C : ResponseMessage;
        C Post<C>(RequestMessage message, string method, int timeoutInMilliseconds) where C : ResponseMessage;
        RestResponse<C> Send<C>(RequestMessage message, string verb, string endpoint, int timeoutInMilliseconds, bool throwTimeoutException = true, NameValueCollection headerFields = null, JsonSerializerSettings jsonSeralizeSettings = null);
        void AddClientCertificate(string certificateThumbprint,StoreName storeName,StoreLocation location);
        string PrepareQueryString(NameValueCollection queryStringFields);
    }
}
