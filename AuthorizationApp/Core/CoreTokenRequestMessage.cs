using Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AuthorizationApp.Core
{
    public class CoreTokenRequestMessage : BasicTokenRequestMessage
    {
        public string IpAddress { get; set; }
    }
}