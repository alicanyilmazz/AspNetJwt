using Infrastructure.Common;
using Infrastructure.Controller.Manages;
using Infrastructure.GlobalVariables.Constants;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Infrastructure.Filters
{
    public class HostAuthorizeAttribute : AuthorizeAttribute
    {
        public static string HostAuthorizationSettingKey { get; set; }
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (actionContext.ControllerContext.RequestContext.Principal is ClaimsPrincipal claimsPrincipal)
            {
                var claim = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Typ);
                var tokenType = claim?.Value;

                var manager = ServiceLocator.Create<TokenManager>();
                if (string.IsNullOrEmpty(tokenType) || tokenType != manager.Options.TokenTyp)
                {
                    return false;
                }
            }
            return base.IsAuthorized(actionContext);
        }
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            bool hasHostAuthorization = string.IsNullOrEmpty(HostAuthorizationSettingKey) || HostConfiguration.GetSetting(HostAuthorizationSettingKey,"A").Equals("A");
            if (!hasHostAuthorization)
            {
                return; // Skips Authorization
            }
            base.OnAuthorization(actionContext);
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            if (actionContext is null)
            {
                throw new ArgumentNullException(nameof(actionContext));
            }
            actionContext.Response = Helper.GenerateUnAuthorizedHttpMessage("Authentication failed.", AuthorizationErrorCodes.UNAUTHORIZED_ERROR_CODE);
            //base.HandleUnauthorizedRequest(actionContext);
        }
    }
}
