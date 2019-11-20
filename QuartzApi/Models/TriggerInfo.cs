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
        /// TriggerName.
        /// </summary>
        public string TriggerName { get; set; }

        /// <summary>
        /// TriggerGroup
        /// </summary>
        public string TriggerGroup { get; set; }

        public string JobName { get; set; }

        public string JobGroup { get; set; }

        public DateTime? NextFireTime { get; set; }

        public DateTime? PrevFireTime { get; set; }

        public int? Priority { get; set; }

        public string TriggerState { get; set; }

        public string TriggerType { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }
    }
}
