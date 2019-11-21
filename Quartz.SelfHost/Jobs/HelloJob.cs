using System.Threading.Tasks;

namespace Quartz.Console.Jobs
{
    public class HelloJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await System.Console.Out.WriteLineAsync("Greetings from HelloJob!");
        }
    }

}
