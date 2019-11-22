using Entities;
using System;

namespace Quartz.Api.Models
{
    /// <summary>
    /// Trigger Info.
    /// </summary>
    public class TriggerInfo
    {
        public TriggerInfo(QrtzTriggers trigger)
        {
            SchedName = trigger.SchedName;
            TriggerName = trigger.TriggerName;
            TriggerGroup = trigger.TriggerGroup;
            TriggerState = trigger.TriggerState;
            Priority = trigger.Priority;
            PrevFireTime = trigger.PrevFireTime;
            NextFireTime = trigger.NextFireTime;
            TriggerType = trigger.TriggerType;
            StartTime = trigger.StartTime;
            EndTime = trigger.EndTime;
        }

        public string SchedName { get; set; }

        /// <summary>
        /// 触发器名称.
        /// </summary>
        public string TriggerName { get; set; }

        /// <summary>
        /// 触发器组.
        /// </summary>
        public string TriggerGroup { get; set; }

        /// <summary>
        /// Job名称
        /// </summary>
        public string JobName { get; set; }

        /// <summary>
        /// Job组.
        /// </summary>
        public string JobGroup { get; set; }

        /// <summary>
        /// 下次触发时间.
        /// </summary>
        public DateTime? NextFireTime { get; set; }

        /// <summary>
        /// 上次触发时间
        /// </summary>
        public DateTime? PrevFireTime { get; set; }

        /// <summary>
        /// 优先级.
        /// </summary>
        public int? Priority { get; set; }

        /// <summary>
        /// 触发器状态
        /// </summary>
        public string TriggerState { get; set; }

        /// <summary>
        /// 触发器类型
        /// </summary>
        public string TriggerType { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
    }
}
