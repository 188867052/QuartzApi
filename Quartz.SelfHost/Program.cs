using System.IO;
using Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Quartz.SelfHost.Common;
using Quartz.SelfHost.Jobs;
using Serilog;
using Serilog.Events;

namespace Quartz.SelfHost
{
    public class Program
    {
        private static void ConfigureLogger()
        {
            var fileSize = 1024 * 1024 * 10;//10M
            var fileCount = 2;
            Log.Logger = new LoggerConfiguration().Enrich.FromLogContext()
                                 .MinimumLevel.Debug()
                                 .MinimumLevel.Override("System", LogEventLevel.Information)
                                 .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                                 .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Information).WriteTo.Async(a => a.RollingFile("File/logs/log-{Date}-Information.txt", fileSizeLimitBytes: fileSize, retainedFileCountLimit: fileCount)))
                                 .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Debug).WriteTo.Async(a => a.RollingFile("File/logs/log-{Date}-Debug.txt", fileSizeLimitBytes: fileSize, retainedFileCountLimit: fileCount)))
                                 .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Warning).WriteTo.Async(a => a.RollingFile("File/logs/log-{Date}-Warning.txt", fileSizeLimitBytes: fileSize, retainedFileCountLimit: fileCount)))
                                 .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Error).WriteTo.Async(a => a.RollingFile("File/logs/log-{Date}-Error.txt", fileSizeLimitBytes: fileSize, retainedFileCountLimit: fileCount)))
                                 .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Fatal).WriteTo.Async(a => a.RollingFile("File/logs/log-{Date}-Fatal.txt", fileSizeLimitBytes: fileSize, retainedFileCountLimit: fileCount)))
                                 .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => true)).WriteTo.Async(a => a.RollingFile("File/logs/log-{Date}-All.txt", fileSizeLimitBytes: fileSize, retainedFileCountLimit: fileCount))
                                 .CreateLogger();
        }

        static void Main(string[] args)
        {
            ConfigureLogger();

            ScheduleJob<HelloJob>(new JobKey(typeof(HelloJob).Name, "default"), new TriggerKey("triggerName2", "triggerGroup2"));

            var config = new ConfigurationBuilder()
             .AddEnvironmentVariables(prefix: "ASPNETCORE_")
             .AddJsonFile("appsettings.json")
             .Build();

            var host = new WebHostBuilder()
                .UseConfiguration(config)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseKestrel().UseUrls("http://localhost:5001")
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
