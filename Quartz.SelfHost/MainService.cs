using System.Diagnostics;
using System.IO;
using System.Linq;
using Entities;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Quartz.SelfHost.Common;
using Quartz.SelfHost.Jobs;
using Serilog;
using Serilog.Events;

namespace Quartz.SelfHost
{
    public class MainService
    {
        private string[] args;
        public MainService(string[] vs)
        {
            args = vs;
        }

        public void Start()
        {
            var isService = !(Debugger.IsAttached || args.Contains("--console"));
            var builder = CreateWebHostBuilder(args.Where(arg => arg != "--console").ToArray());
            ConfigureLogger();

            ScheduleJob<HelloJob>(new JobKey(typeof(HelloJob).Name, "default"), new TriggerKey("triggerName2", "triggerGroup2"));

            if (isService)
            {
                var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                var pathToContentRoot = Path.GetDirectoryName(pathToExe);
                builder.UseContentRoot(pathToContentRoot);
            }

            var host = builder.Build();
            host.Start();
        }

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

        public void Stop()
        {
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var config = new ConfigurationBuilder()
                            .AddJsonFile("config.json", optional: true, reloadOnChange: true)
                            .Build();

            return WebHost.CreateDefaultBuilder(args)
                    .UseKestrel()
                    .UseConfiguration(config)
                    .UseStartup<Startup>();
        }
    }
}
