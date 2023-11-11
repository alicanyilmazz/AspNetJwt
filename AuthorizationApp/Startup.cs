using AuthorizationApp;
using Infrastructure;
using Infrastructure.Controller.Manages;
using Microsoft.Owin;
using Owin;

[assembly:OwinStartup(typeof(Startup))]
namespace AuthorizationApp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var manager = ServiceLocator.Create<TokenManager>();
            var tokenValidationParameters = manager.Options.GenerateBaseValidationParameters();
            tokenValidationParameters.ValidateIssuerSigningKey = true;
            app.UseJwtBearerAuthentication(new Microsoft.Owin.Security.Jwt.JwtBearerAuthenticationOptions
            {
                AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Active,
                TokenValidationParameters = tokenValidationParameters
            });
        }
    }
}