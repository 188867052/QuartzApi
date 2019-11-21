using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Quartz.SelfHost.Common;
using Quartz.SelfHost.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quartz.SelfHost.Controllers
{
    /// <summary>
    /// 任务调度
    /// </summary>
    //[Authorize]
    [Route("api/[controller]/[Action]")]
    public class JobController : Controller
    {
        private SchedulerCenter scheduler;

        /// <summary>
        /// 任务调度对象
        /// </summary>
        /// <param name="schedulerCenter"></param>
        public JobController(SchedulerCenter schedulerCenter)
        {
            scheduler = schedulerCenter;
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<BaseResult> AddJob([FromBody] ScheduleEntity entity)
        {
            return await scheduler.AddScheduleJobAsync(entity);
        }

        /// <summary>
        /// 暂停任务
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<BaseResult> StopJob(string group, string name)
        {
            return await scheduler.StopOrDelScheduleJobAsync(group, name);
        }

        /// <summary>
        /// 删除任务
        /// </summary> 
        /// <returns></returns>
        [HttpGet]
        public async Task<BaseResult> RemoveJob(string group, string name)
        {
            return await scheduler.StopOrDelScheduleJobAsync(group, name, true);
        }

        /// <summary>
        /// 恢复运行暂停的任务
        /// </summary> 
        /// <returns></returns>
        [HttpGet]
        public async Task<BaseResult> ResumeJob(string group, string name)
        {
            return await scheduler.ResumeJobAsync(group, name);
        }

        /// <summary>
        /// 查询任务
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ScheduleEntity> QueryJob(string group, string name)
        {
            return await scheduler.QueryJobAsync(group, name);
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<BaseResult> ModifyJob([FromBody]ScheduleEntity entity)
        {
            await scheduler.StopOrDelScheduleJobAsync(entity.JobGroup, entity.JobName, true);
            await scheduler.AddScheduleJobAsync(entity);
            return new BaseResult() { Msg = "修改计划任务成功！" };
        }

        /// <summary>
        /// 立即执行
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<bool> TriggerJob(string group, string name)
        {
            await scheduler.TriggerJobAsync(new JobKey(name, group));
            return true;
        }

        /// <summary>
        /// 获取job日志
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<string>> GetJobLogs(string group, string name)
        {
            return await scheduler.GetJobLogsAsync(new JobKey(name, group));
        }

        /// <summary>
        /// 启动调度
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<bool> StartSchedule()
        {
            return await scheduler.StartScheduleAsync();
        }

        /// <summary>
        /// 停止调度
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<bool> StopSchedule()
        {
            return await scheduler.StopScheduleAsync();
        }

        /// <summary>
        /// 获取所有任务
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<JobInfoEntity>> GetAllJob()
        {
            try
            {
                return await scheduler.GetAllJobAsync();
            }
            catch (System.Exception ex)
            {
                List<JobInfoEntity> list = new List<JobInfoEntity>();
                list.Add(new JobInfoEntity { GroupName = ex.Message + ex.StackTrace });
                return list;
            }
        }

        /// <summary>
        /// 获取所有Job信息（简要信息 - 刷新数据的时候使用）
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<JobBriefInfoEntity>> GetAllJobBriefInfo()
        {
            return await scheduler.GetAllJobBriefInfoAsync();
        }

        /// <summary>
        /// 移除异常信息
        /// </summary>
        [HttpGet]
        public async Task<bool> RemoveErrLog(string group, string name)
        {
            return await scheduler.RemoveErrLog(group, name);
        }
    }
}
