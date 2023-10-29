using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Infrastructure.Common
{
    public class ExceptionHelper
    {
        public static HttpResponseException GenerateUnAuthorizeException(string message , int code)
        {
            HttpResponseMessage unauthorizedHttpResponse = GenerateUnAuthorizedHttpMessage(message, code);
            return new HttpResponseException(unauthorizedHttpResponse);
        }
        public static HttpResponseMessage GenerateUnAuthorizedHttpMessage(string message , int code)
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

    }
}
