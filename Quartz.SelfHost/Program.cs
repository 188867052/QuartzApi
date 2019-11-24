using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Topshelf;

namespace Quartz.SelfHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var rc = HostFactory.Run(x =>
            {
                x.Service<MainService>(s =>
                {
                    s.ConstructUsing(name => new MainService(args));
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();

                x.SetDescription("Topshelf_JwtAPIService");
                x.SetDisplayName("Topshelf_JwtAPIService");
                x.SetServiceName("Topshelf_JwtAPIService");
            });
        }
    }
}
