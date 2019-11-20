using Quartz.Console.Jobs;
using Quartz.Impl;
using Quartz.Logging;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Quartz.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            LogProvider.SetCurrentLogProvider(new ConsoleLogProvider());
            RunProgramRunExample().GetAwaiter().GetResult();
        }

        public static async Task RunProgramRunExample()
        {
            try
            {
                StdSchedulerFactory factory = new StdSchedulerFactory(Properties);
                IScheduler scheduler = await factory.GetScheduler();

                // and start it off
                await scheduler.Start();

                IJobDetail job = JobBuilder.Create<HelloJob>()
                    .WithIdentity("job1", "group1")
                    .Build();
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("trigger1", "group1")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(1)
                        .RepeatForever())
                    .Build();
                await scheduler.ScheduleJob(job, trigger);

                ITrigger trigger2 = TriggerBuilder.Create()
                  .WithIdentity("trigger2", "group2")
                  .StartNow()
                  .WithSimpleSchedule(x => x
                      .WithIntervalInSeconds(1)
                      .RepeatForever())
                  .Build();
                IJobDetail job2 = JobBuilder.Create<ServerJob>()
                  .WithIdentity("job2", "group2")
                  .Build();
                await scheduler.ScheduleJob(job2, trigger2);

                // some sleep to show what's happening
                await Task.Delay(TimeSpan.FromSeconds(60));

                // and last shut down the scheduler when you are ready to close your program
                await scheduler.Shutdown();
            }
            catch (SchedulerException se)
            {
                System.Console.WriteLine(se);
            }
        }

        public static NameValueCollection Properties => new NameValueCollection
                {
                    { "quartz.dataSource.Quartz.provider", "SqlServer" },
                    { "quartz.serializer.type", "json" },
                    { "quartz.jobStore.tablePrefix", "QRTZ_" },
                    { "quartz.dataSource.Quartz.connectionString", "Data Source=.;Initial Catalog=quartz;Integrated Security=True" },
                    { "quartz.jobStore.type", "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz" },
                    { "quartz.jobStore.dataSource", "Quartz" },
                    { "quartz.jobStore.driverDelegateType", "Quartz.Impl.AdoJobStore.StdAdoDelegate, Quartz" }
                };
    }
}
