using System.Threading.Tasks;

namespace Quartz.SelfHost.Repositories
{
    public interface ILogRepositorie
    {
        Task<bool> RemoveErrLogAsync(string jobGroup, string jobName);
    }
}
