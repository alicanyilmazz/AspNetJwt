using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Models
{
    public class RefreshTokenRequestMessage
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
