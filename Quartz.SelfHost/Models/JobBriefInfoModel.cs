using System;
using System.Collections.Generic;

namespace Quartz.SelfHost.Models
{
    public class JobBriefInfoModel
    {
        /// <summary>
        /// 任务组名
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 任务信息
        /// </summary>
        public List<JobBriefInfo> JobInfoList { get; set; } = new List<JobBriefInfo>();
    }

    public class JobBriefInfo
    {
        /// <summary>
        /// 任务名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 下次执行时间
        /// </summary>
        public DateTime? NextFireTime { get; set; }

        /// <summary>
        /// 上次执行时间
        /// </summary>
        public DateTime? PreviousFireTime { get; set; }

        /// <summary>
        /// 上次执行的异常信息
        /// </summary>
        public string LastErrMsg { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public TriggerState TriggerState { get; set; }

        /// <summary>
        /// 显示状态
        /// </summary>
        public string DisplayState => TriggerState switch
        {
            TriggerState.Normal => "正常",
            TriggerState.Paused => "暂停",
            TriggerState.Complete => "完成",
            TriggerState.Error => "异常",
            TriggerState.Blocked => "阻塞",
            TriggerState.None => "不存在",
            _ => "未知",
        };
    }
}
