﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EFCore.Scaffolding.Extension;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Configuration;
using Quartz.Impl;
using Quartz.Impl.AdoJobStore;
using Quartz.Impl.AdoJobStore.Common;
using Quartz.Impl.Matchers;
using Quartz.Impl.Triggers;
using Quartz.SelfHost.Enums;
using Quartz.SelfHost.Models;
using Quartz.SelfHost.Repositories;
using Quartz.Simpl;
using Quartz.Util;
using Serilog;

namespace Quartz.SelfHost.Common
{
    /// <summary>
    /// 调度中心
    /// </summary>
    public class SchedulerCenter
    {
        private static readonly IDbProvider dbProvider;
        private static readonly string driverDelegateType;
        private static IScheduler _scheduler;
        public static SchedulerCenter Instance;

        private SchedulerCenter()
        {
        }

        static SchedulerCenter()
        {
            Instance = new SchedulerCenter();
            string connectionString = Connection.ConnectionString;
            driverDelegateType = typeof(SqlServerDelegate).AssemblyQualifiedName;
            dbProvider = new DbProvider("SqlServer", connectionString);
        }

        /// <summary>
        /// 返回任务计划（调度器）
        /// </summary>
        /// <returns></returns>
        private IScheduler Scheduler
        {
            get
            {
                if (_scheduler != null)
                {
                    return _scheduler;
                }

                DBConnectionManager.Instance.AddConnectionProvider("default", dbProvider);
                var serializer = new JsonObjectSerializer();
                serializer.Initialize();
                var jobStore = new JobStoreTX
                {
                    DataSource = "default",
                    TablePrefix = "QRTZ_",
                    InstanceId = "AUTO",
                    DriverDelegateType = driverDelegateType,
                    ObjectSerializer = serializer,
                };
                DirectSchedulerFactory.Instance.CreateScheduler("benny" + "Scheduler", "AUTO", new DefaultThreadPool(), jobStore);
                _scheduler = SchedulerRepository.Instance.Lookup("benny" + "Scheduler").Result;

                _scheduler.Start();
                return _scheduler;
            }
        }

        /// <summary>
        /// 添加一个工作调度
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<HttpResponseModel> AddScheduleJobAsync(ScheduleModel model)
        {
            var result = new HttpResponseModel();
            try
            {
                //检查任务是否已存在
                var jobKey = new JobKey(model.JobName, model.JobGroup);
                if (await Scheduler.CheckExists(jobKey))
                {
                    result.Code = HttpStatusCode.OK;
                    result.Message = "任务已存在";
                    return result;
                }

                //http请求配置
                var setting = new Dictionary<string, string>()
                {
                    { Constant.CmdPath,model.CmdPath.ToString()},
                    { Constant.IsExcuteCmd,model.IsExcuteCmd.ToString()},
                    { Constant.RequestUrl,model.RequestUrl},
                    { Constant.RequestParameters,model.RequestParameters},
                    { Constant.RequestType, ((int)model.RequestType).ToString()},
                    { Constant.Headers, model.Headers},
                    { Constant.MailMessage, ((int)model.MailMessage).ToString()},
                };

                // 定义这个工作，并将其绑定到我们的IJob实现类                
                var job = JobBuilder.CreateForAsync<ExcuteJob>()
                    .SetJobData(new JobDataMap(setting))
                    .WithDescription(model.Description)
                    .WithIdentity(model.JobName, model.JobGroup)
                    .Build();

                var trigger = model.TriggerType == TriggerTypeEnum.Cron ? CreateCronTrigger(model) : CreateSimpleTrigger(model);
                await Scheduler.ScheduleJob(job, trigger);
                result.Code = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                result.Code = HttpStatusCode.InternalServerError;
                result.Message = ex.Message + Environment.NewLine + ex.StackTrace;
            }
            return result;
        }

        public async Task ScheduleJob(IJobDetail jobDetail, ITrigger trigger)
        {
            await Scheduler.ScheduleJob(jobDetail, trigger);
        }

        /// <summary>
        /// 删除 指定的Job
        /// </summary>
        /// <param name="jobGroup">任务分组</param>
        /// <param name="jobName">任务名称</param>
        /// <returns></returns>
        public async Task<HttpResponseModel> DeleteJobAsync(JobKey jobKey)
        {
            try
            {
                var isExists = await Scheduler.CheckExists(jobKey);
                if (!isExists)
                {
                    return new HttpResponseModel
                    {
                        Code = HttpStatusCode.OK,
                        Message = $"jobKey: Name={jobKey.Name}, Group={jobKey.Group}不存在！"
                    };
                }

                await Scheduler.PauseJob(jobKey);
                await Scheduler.DeleteJob(jobKey);
                return new HttpResponseModel
                {
                    Code = HttpStatusCode.OK,
                    Message = "删除任务计划成功！"
                };
            }
            catch (Exception ex)
            {
                return new HttpResponseModel
                {
                    Code = HttpStatusCode.InternalServerError,
                    Message = "停止任务计划失败" + ex.Message
                };
            }
        }

        public async Task<HttpResponseModel> PauseJobAsync(JobKey jobKey)
        {
            try
            {
                var isExists = await Scheduler.CheckExists(jobKey);
                if (!isExists)
                {
                    return new HttpResponseModel
                    {
                        Code = HttpStatusCode.OK,
                        Message = $"jobKey: Name={jobKey.Name}, Group={jobKey.Group}不存在！"
                    };
                }

                await Scheduler.PauseJob(jobKey);
                return new HttpResponseModel
                {
                    Code = HttpStatusCode.OK,
                    Message = "停止任务计划成功！"
                };
            }
            catch (Exception ex)
            {
                return new HttpResponseModel
                {
                    Code = HttpStatusCode.InternalServerError,
                    Message = ex.ToString()
                };
            }
        }

        /// <summary>
        /// 恢复运行暂停的任务
        /// </summary>
        /// <param name="jobName">任务名称</param>
        /// <param name="jobGroup">任务分组</param>
        public async Task<HttpResponseModel> ResumeJobAsync(string jobGroup, string jobName)
        {
            HttpResponseModel result = new HttpResponseModel();
            try
            {
                //检查任务是否存在
                var jobKey = new JobKey(jobName, jobGroup);
                if (await Scheduler.CheckExists(jobKey))
                {
                    //任务已经存在则暂停任务
                    await Scheduler.ResumeJob(jobKey);
                    result.Message = "恢复任务计划成功！";
                    Log.Information(string.Format("任务“{0}”恢复运行", jobName));
                }
                else
                {
                    result.Message = "任务不存在";
                }
            }
            catch (Exception ex)
            {
                result.Message = "恢复任务计划失败！";
                result.Code = HttpStatusCode.InternalServerError;
                Log.Error(string.Format("恢复任务失败！{0}", ex));
            }
            return result;
        }

        /// <summary>
        /// 查询任务
        /// </summary>
        /// <param name="jobGroup"></param>
        /// <param name="jobName"></param>
        /// <returns></returns>
        public async Task<ScheduleModel> QueryJobAsync(string jobGroup, string jobName)
        {
            var entity = new ScheduleModel();
            var jobKey = new JobKey(jobName, jobGroup);
            var jobDetail = await Scheduler.GetJobDetail(jobKey);
            var triggersList = await Scheduler.GetTriggersOfJob(jobKey);
            var triggers = triggersList.AsEnumerable().FirstOrDefault();
            var intervalSeconds = (triggers as SimpleTriggerImpl)?.RepeatInterval.TotalSeconds;
            entity.RequestUrl = jobDetail.JobDataMap.GetString(Constant.RequestUrl);
            entity.BeginTime = triggers.StartTimeUtc.LocalDateTime;
            entity.EndTime = triggers.EndTimeUtc?.LocalDateTime;
            entity.IntervalSecond = intervalSeconds.HasValue ? Convert.ToInt32(intervalSeconds.Value) : 0;
            entity.JobGroup = jobGroup;
            entity.JobName = jobName;
            entity.Cron = (triggers as CronTriggerImpl)?.CronExpressionString;
            entity.RunTimes = (triggers as SimpleTriggerImpl)?.RepeatCount;
            entity.TriggerType = triggers is SimpleTriggerImpl ? TriggerTypeEnum.Simple : TriggerTypeEnum.Cron;
            entity.RequestType = (HttpMethod)int.Parse(jobDetail.JobDataMap.GetString(Constant.RequestType));
            entity.RequestParameters = jobDetail.JobDataMap.GetString(Constant.RequestParameters);
            entity.Headers = jobDetail.JobDataMap.GetString(Constant.Headers);
            entity.MailMessage = (MailMessageEnum)int.Parse(jobDetail.JobDataMap.GetString(Constant.MailMessage) ?? "0");
            entity.Description = jobDetail.Description;
            return entity;
        }

        /// <summary>
        /// 立即执行
        /// </summary>
        /// <param name="jobKey"></param>
        /// <returns></returns>
        public async Task<bool> TriggerJobAsync(JobKey jobKey)
        {
            await Scheduler.TriggerJob(jobKey);
            return true;
        }

        /// <summary>
        /// 获取job日志
        /// </summary>
        /// <param name="jobKey"></param>
        /// <returns></returns>
        public async Task<List<string>> GetJobLogsAsync(JobKey jobKey)
        {
            var jobDetail = await Scheduler.GetJobDetail(jobKey);
            return jobDetail.JobDataMap[Constant.LogList] as List<string>;
        }

        /// <summary>
        /// 获取所有Job（详情信息 - 初始化页面调用）
        /// </summary>
        /// <returns></returns>
        public async Task<List<JobInfoModel>> GetAllJobAsync()
        {
            List<JobKey> jboKeyList = new List<JobKey>();
            List<JobInfoModel> jobInfoList = new List<JobInfoModel>();
            var groupNames = await Scheduler.GetJobGroupNames();
            foreach (var groupName in groupNames.OrderBy(t => t))
            {
                jboKeyList.AddRange(await Scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(groupName)));
                jobInfoList.Add(new JobInfoModel() { GroupName = groupName });
            }
            foreach (var jobKey in jboKeyList.OrderBy(t => t.Name))
            {
                var jobDetail = await Scheduler.GetJobDetail(jobKey);
                var triggersList = await Scheduler.GetTriggersOfJob(jobKey);
                var triggers = triggersList.AsEnumerable().FirstOrDefault();

                var interval = string.Empty;
                if (triggers is SimpleTriggerImpl)
                    interval = (triggers as SimpleTriggerImpl)?.RepeatInterval.ToString();
                else
                    interval = (triggers as CronTriggerImpl)?.CronExpressionString;

                foreach (var jobInfo in jobInfoList)
                {
                    if (jobInfo.GroupName == jobKey.Group)
                    {
                        jobInfo.JobInfoList.Add(new JobInfo()
                        {
                            Name = jobKey.Name,
                            LastErrMsg = jobDetail.JobDataMap.GetString(Constant.Exception),
                            RequestUrl = jobDetail.JobDataMap.GetString(Constant.RequestUrl),
                            TriggerState = await Scheduler.GetTriggerState(triggers.Key),
                            PreviousFireTime = triggers.GetPreviousFireTimeUtc()?.LocalDateTime,
                            NextFireTime = triggers.GetNextFireTimeUtc()?.LocalDateTime,
                            BeginTime = triggers.StartTimeUtc.LocalDateTime,
                            Interval = interval,
                            EndTime = triggers.EndTimeUtc?.LocalDateTime,
                            Description = jobDetail.Description
                        });
                        continue;
                    }
                }
            }
            return jobInfoList;
        }

        /// <summary>
        /// 移除异常信息
        /// 因为只能在IJob持久化操作JobDataMap，所有这里直接暴力操作数据库。
        /// </summary>
        /// <param name="jobGroup"></param>
        /// <param name="jobName"></param>
        /// <returns></returns>          
        public async Task<bool> RemoveErrLog(string jobGroup, string jobName)
        {
            ILogRepositorie logRepositorie = LogRepositorieFactory.CreateLogRepositorie(driverDelegateType, dbProvider);

            if (logRepositorie == null) return false;

            await logRepositorie.RemoveErrLogAsync(jobGroup, jobName);

            var jobKey = new JobKey(jobName, jobGroup);
            var jobDetail = await Scheduler.GetJobDetail(jobKey);
            jobDetail.JobDataMap[Constant.Exception] = string.Empty;

            return true;
        }

        /// <summary>
        /// 获取所有Job信息（简要信息 - 刷新数据的时候使用）
        /// </summary>
        /// <returns></returns>
        public async Task<List<JobBriefInfoModel>> GetAllJobBriefInfoAsync()
        {
            List<JobKey> jboKeyList = new List<JobKey>();
            List<JobBriefInfoModel> jobInfoList = new List<JobBriefInfoModel>();
            var groupNames = await Scheduler.GetJobGroupNames();
            foreach (var groupName in groupNames.OrderBy(t => t))
            {
                jboKeyList.AddRange(await Scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(groupName)));
                jobInfoList.Add(new JobBriefInfoModel() { GroupName = groupName });
            }
            foreach (var jobKey in jboKeyList.OrderBy(t => t.Name))
            {
                var jobDetail = await Scheduler.GetJobDetail(jobKey);
                var triggersList = await Scheduler.GetTriggersOfJob(jobKey);
                var triggers = triggersList.AsEnumerable().FirstOrDefault();

                foreach (var jobInfo in jobInfoList)
                {
                    if (jobInfo.GroupName == jobKey.Group)
                    {
                        jobInfo.JobInfoList.Add(new JobBriefInfo()
                        {
                            Name = jobKey.Name,
                            LastErrMsg = jobDetail.JobDataMap.GetString(Constant.Exception),
                            TriggerState = await Scheduler.GetTriggerState(triggers.Key),
                            PreviousFireTime = triggers.GetPreviousFireTimeUtc()?.LocalDateTime,
                            NextFireTime = triggers.GetNextFireTimeUtc()?.LocalDateTime,
                        });
                        continue;
                    }
                }
            }
            return jobInfoList;
        }

        /// <summary>
        /// 开启调度器
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StartScheduleAsync()
        {
            //开启调度器
            if (Scheduler.InStandbyMode)
            {
                await Scheduler.Start();
                Log.Information("任务调度启动！");
            }
            return Scheduler.InStandbyMode;
        }

        /// <summary>
        /// 停止任务调度
        /// </summary>
        public async Task<bool> StopScheduleAsync()
        {
            //判断调度是否已经关闭
            if (!Scheduler.InStandbyMode)
            {
                //等待任务运行完成
                await Scheduler.Standby(); //TODO  注意：Shutdown后Start会报错，所以这里使用暂停。
                Log.Information("任务调度暂停！");
            }
            return !Scheduler.InStandbyMode;
        }

        /// <summary>
        /// 创建类型Simple的触发器
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private ITrigger CreateSimpleTrigger(ScheduleModel entity)
        {
            //作业触发器
            if (entity.RunTimes.HasValue && entity.RunTimes > 0)
            {
                return TriggerBuilder.Create()
               .WithIdentity(entity.JobName, entity.JobGroup)
               .StartAt(entity.BeginTime)//开始时间
               .EndAt(entity.EndTime)//结束数据
               .WithSimpleSchedule(x =>
               {
                   x.WithIntervalInSeconds(entity.IntervalSecond.Value)//执行时间间隔，单位秒
                        .WithRepeatCount(entity.RunTimes.Value)//执行次数、默认从0开始
                        .WithMisfireHandlingInstructionFireNow();
               })
               .ForJob(entity.JobName, entity.JobGroup)//作业名称
               .Build();
            }
            else
            {
                return TriggerBuilder.Create()
               .WithIdentity(entity.JobName, entity.JobGroup)
               .StartAt(entity.BeginTime)//开始时间
               .EndAt(entity.EndTime)//结束数据
               .WithSimpleSchedule(x =>
               {
                   x.WithIntervalInSeconds(entity.IntervalSecond.Value)//执行时间间隔，单位秒
                        .RepeatForever()//无限循环
                        .WithMisfireHandlingInstructionFireNow();
               })
               .ForJob(entity.JobName, entity.JobGroup)//作业名称
               .Build();
            }

        }

        /// <summary>
        /// 创建类型Cron的触发器
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private ITrigger CreateCronTrigger(ScheduleModel entity)
        {
            // 作业触发器
            return TriggerBuilder.Create()

                   .WithIdentity(entity.JobName, entity.JobGroup)
                   .StartAt(entity.BeginTime)//开始时间
                   .EndAt(entity.EndTime)//结束时间
                   .WithCronSchedule(entity.Cron, cronScheduleBuilder => cronScheduleBuilder.WithMisfireHandlingInstructionFireAndProceed())//指定cron表达式
                   .ForJob(entity.JobName, entity.JobGroup)//作业名称
                   .Build();
        }
    }
}

