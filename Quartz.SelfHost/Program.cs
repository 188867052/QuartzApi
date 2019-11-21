using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Quartz.Console.Jobs;
using Quartz.Impl.AdoJobStore;
using Quartz.Impl.AdoJobStore.Common;

namespace Quartz.SelfHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ConfigJobs();

            var config = new ConfigurationBuilder()
             .AddCommandLine(args)
             .AddEnvironmentVariables(prefix: "ASPNETCORE_")
             .Build();

            var host = new WebHostBuilder()
                .UseConfiguration(config)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseKestrel()
                .UseStartup<Startup>()
                .Build();

            host.Run();

        }

        private static void ConfigJobs()
        {
            string connectionString = "Data Source=.;Initial Catalog=quartz;Integrated Security=True";
            var driverDelegateType = typeof(SqlServerDelegate).AssemblyQualifiedName;
            SchedulerCenter schedulerCenter = SchedulerCenter.Instance;
            schedulerCenter.Setting(new DbProvider("SqlServer", connectionString), driverDelegateType);

            var scheduler = schedulerCenter;
            IJobDetail job = JobBuilder.Create<ServerJob>()
                 .WithIdentity(nameof(ServerJob), "default")
                 .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("trigger", "group")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(1)
                    .RepeatForever())
                .Build();

            scheduler.StopOrDelScheduleJobAsync("default", nameof(ServerJob), true).Wait();
            scheduler.ScheduleJob(job, trigger).Wait();
        }
    }
}
