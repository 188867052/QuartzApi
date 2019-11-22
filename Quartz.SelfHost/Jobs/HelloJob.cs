using Serilog;
using System;
using System.Threading.Tasks;

namespace Quartz.SelfHost.Jobs
{
    public class HelloJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await System.Console.Out.WriteLineAsync($"{DateTime.Now}  Greetings from HelloJob!");
            Log.Information($"Greetings from HelloJob!");
        }
    }
}
