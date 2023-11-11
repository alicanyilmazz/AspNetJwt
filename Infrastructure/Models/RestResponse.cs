using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Models
{
    public class RestResponse<T>
    {
        public T Body { get; set; }
        public WebHeaderCollection Header { get; set; }
    }
    public class RestResponse
    {
        public string Body { get; set; }
        public WebHeaderCollection Header { get; set; }
    }
}
