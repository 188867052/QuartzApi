using System;
using System.IO;
using Autofac.Core;
using Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Quartz.SelfHost.Common;
using Quartz.SelfHost.Jobs;
using Topshelf;

namespace Quartz.SelfHost
{
    public class Program
    {
        static void Main(string[] args)
        {
            ScheduleJob<HelloJob>(new JobKey(typeof(HelloJob).Name, "default"), new TriggerKey("triggerName2", "triggerGroup2"));

            var config = new ConfigurationBuilder()
             .AddEnvironmentVariables(prefix: "ASPNETCORE_")
             .AddJsonFile("appsettings.json")
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
            var scheduler = SchedulerCenter.Instance;
            var job = JobBuilder.Create<T>()
                 .WithIdentity(jobKey)
                 .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity(triggerKey)
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(2).RepeatForever())
                .Build();

            var context = new QuartzDbContext();
            var entity = context.QrtzTriggers.Find("bennyScheduler", triggerKey.Name, triggerKey.Group);
            if (entity != null)
            {
                context.QrtzTriggers.Remove(entity);
                context.SaveChanges();
            }

            scheduler.DeleteJobAsync(jobKey).Wait();
            scheduler.ScheduleJob(job, trigger).Wait();
        }
    }
}
