using ConsumeAPI.Utilities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace ConsumeAPI.Filters
{
    public class HostAuthorizeAttribute : AuthorizeAttribute
    {
        public static string HostAuthorizationSettingKey { get; set; }
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            ClaimsPrincipal claimsPrincipal = actionContext.ControllerContext.RequestContext.Principal as ClaimsPrincipal;
            if (claimsPrincipal != null)
            {
                var claim = claimsPrincipal.Claims.FirstOrDefault(x=>x.Type == JwtRegisteredClaimNames.Typ);
                var tokenType = claim?.Value;

                var manager = ServiceLocator.Create<>
            }
            return base.IsAuthorized(actionContext);
        }
    }
}