using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;

namespace Quartz
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var config = new HttpSelfHostConfiguration("http://localhost:8080");

                config.Routes.MapHttpRoute(
                    "API Default", "api/{controller}/{id}",
                    new { id = RouteParameter.Optional });


                using (HttpSelfHostServer server = new HttpSelfHostServer(config))
                {
                    server.OpenAsync().Wait();
                    System.Console.WriteLine("Press Enter to quit.");
                    System.Console.ReadLine();
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }

        }
    }
}
