using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Quartz.SelfHost.Models;
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
        public async Task<HttpResponseModel> AddJob([FromBody] ScheduleModel entity)
        {
            return await scheduler.AddScheduleJobAsync(entity);
        }

        /// <summary>
        /// 暂停任务
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<HttpResponseModel> StopJob(string name, string group)
        {
            return await scheduler.PauseJobAsync(new JobKey(name, group));
        }

        /// <summary>
        /// 删除任务
        /// </summary> 
        /// <returns></returns>
        [HttpGet]
        public async Task<HttpResponseModel> RemoveJob(string name, string group)
        {
            return await scheduler.DeleteJobAsync(new JobKey(name, group));
        }

        /// <summary>
        /// 恢复运行暂停的任务
        /// </summary> 
        /// <returns></returns>
        [HttpGet]
        public async Task<HttpResponseModel> ResumeJob(string name, string group)
        {
            return await scheduler.ResumeJobAsync(group, name);
        }

        /// <summary>
        /// 查询任务
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ScheduleModel> QueryJob(string name, string group)
        {
            return await scheduler.QueryJobAsync(group, name);
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<HttpResponseModel> ModifyJob([FromBody]ScheduleModel entity)
        {
            await scheduler.DeleteJobAsync(new JobKey(entity.JobName, entity.JobGroup));
            await scheduler.AddScheduleJobAsync(entity);
            return new HttpResponseModel() { Message = "修改计划任务成功！" };
        }

        /// <summary>
        /// 立即执行
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<bool> TriggerJob(string name, string group)
        {
            await scheduler.TriggerJobAsync(new JobKey(name, group));
            return true;
        }

        /// <summary>
        /// 获取job日志
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<string>> GetJobLogs(string name, string group)
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
        public async Task<List<JobInfoModel>> GetAllJob()
        {
            try
            {
                return await scheduler.GetAllJobAsync();
            }
            catch (System.Exception ex)
            {
                List<JobInfoModel> list = new List<JobInfoModel>();
                list.Add(new JobInfoModel { GroupName = ex.Message + ex.StackTrace });
                return list;
            }
        }

        /// <summary>
        /// 获取所有Job信息（简要信息 - 刷新数据的时候使用）
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<JobBriefInfoModel>> GetAllJobBriefInfo()
        {
            return await scheduler.GetAllJobBriefInfoAsync();
        }

        /// <summary>
        /// 移除异常信息
        /// </summary>
        [HttpGet]
        public async Task<bool> RemoveErrLog(string name, string group)
        {
            return await scheduler.RemoveErrLog(group, name);
        }
    }
}
