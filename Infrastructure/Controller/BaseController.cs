using Infrastructure.Controller.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Infrastructure.Controller
{
    public class BaseController : ApiController, IAuthorizationValidator
    {
        public bool Authenticate()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return false;
            }
            if (!identity.IsAuthenticated)
            {
                return false;
            }

            return CheckClaims(identity);
        }

        public virtual bool CheckClaims(ClaimsIdentity claims)
        {
            return true;
        }
    }
}
