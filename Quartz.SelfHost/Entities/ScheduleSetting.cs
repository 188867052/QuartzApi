using System;
using System.Collections.Generic;

namespace Entities
{
    public partial class ScheduleSetting
    {
        public int Id { get; set; }

        public string JobName { get; set; }

        public string JobGroup { get; set; }

        public DateTime? BeginTime { get; set; }

        public string Cron { get; set; }

        public string RequestUrl { get; set; }

        public string Description { get; set; }
    }
}
