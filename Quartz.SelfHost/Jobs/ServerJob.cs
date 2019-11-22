using System;
using System.Threading.Tasks;

namespace Quartz.SelfHost.Jobs
{
    public class ServerJob : IJob
    {
        private const string Count = "count";
        public async Task Execute(IJobExecutionContext context)
        {
            await Console.Out.WriteLineAsync($"{DateTime.Now}  Greetings from ServerJob!");
        }
    }
}
