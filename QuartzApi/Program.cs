using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting.WindowsServices;

namespace QuartzApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var config = new ConfigurationBuilder()
            .AddCommandLine(args)
            .AddEnvironmentVariables(prefix: "ASPNETCORE_")
            .Build();

                var host = new WebHostBuilder()
                    .UseConfiguration(config)
                    .UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseIISIntegration()
                    .UseStartup<Startup>()
                    .Build();

                host.Run();
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }
    }
}
