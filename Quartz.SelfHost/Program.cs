using System.IO;
using Entities;
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
            ScheduleJob<ServerJob>(new JobKey(typeof(ServerJob).Name, "default"), new TriggerKey("triggerName", "triggerGroup"));
            ScheduleJob<HelloJob>(new JobKey(typeof(HelloJob).Name, "default"), new TriggerKey("triggerName2", "triggerGroup2"));

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

        private static void ScheduleJob<T>(JobKey jobKey, TriggerKey triggerKey) where T : IJob
        {
            string connectionString = "Data Source=.;Initial Catalog=quartz;Integrated Security=True";
            var driverDelegateType = typeof(SqlServerDelegate).AssemblyQualifiedName;
            SchedulerCenter schedulerCenter = SchedulerCenter.Instance;
            schedulerCenter.Setting(new DbProvider("SqlServer", connectionString), driverDelegateType);

            var scheduler = schedulerCenter;
            IJobDetail job = JobBuilder.Create<T>()
                 .WithIdentity(jobKey)
                 .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(triggerKey)
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(1).RepeatForever())
                .Build();

            var context = new QuartzDbContext();
            var entity = context.QrtzTriggers.Find("bennyScheduler", triggerKey.Name, triggerKey.Group);
            if (entity != null)
            {
                context.QrtzTriggers.Remove(entity);
                context.SaveChanges();
            }

            scheduler.StopOrDelScheduleJobAsync(jobKey, true).Wait();
            scheduler.ScheduleJob(job, trigger).Wait();
        }
    }
}
