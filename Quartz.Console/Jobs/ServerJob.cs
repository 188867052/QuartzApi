using System;
using System.Threading.Tasks;

namespace Quartz.Console.Jobs
{
    public class ServerJob : IJob
    {
        private const string Count = "count";
        public async Task Execute(IJobExecutionContext context)
        {
            await System.Console.Out.WriteLineAsync("Greetings from ServerJob!");
        }
    }

}
