using AuthorizationApp.Configs;
using Infrastructure;
using Infrastructure.Controller.Manages;
using Infrastructure.Controller.Options;
using Infrastructure.Utilities;
using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace AuthorizationApp
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            FileInfo fileInfo = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config"));
            XmlConfigurator.ConfigureAndWatch(fileInfo);

            GlobalConfiguration.Configure(ApiConfig.Register); //GlobalConfiguration.Configure(WebApiConfig.Register);
            var logService = new LogService(LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType));
            ServiceLocator.Register<ILogService>(logService);
            RegisterTokenManager();
        }
        private static void RegisterTokenManager()
        {
            var tokenOptions = new TokenOptions(() => new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidIssuer = "https://localhost:44387",
                ValidAudience = "https://localhost:44387",
            },"ServiceAppTokenSecret","HostCoretMaxRefreshTokenCount","UseHostCoreRefreshToken","HostCoreTokenExpireSeconds","HostCoreRefreshTokenExpireSeconds");

            var tokenManager = new TokenManager(tokenOptions);
            ServiceLocator.Register<TokenManager>(tokenManager);
        }
    }
}
