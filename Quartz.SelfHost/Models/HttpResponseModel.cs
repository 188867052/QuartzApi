using System.Net;

namespace Quartz.SelfHost.Models
{
    public class HttpResponseModel
    {
        public HttpStatusCode Code { get; set; } = HttpStatusCode.OK;

        public string Message { get; set; }
    }
}
