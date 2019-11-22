using System;
using System.Collections.Generic;
using Quartz.SelfHost.Enums;

namespace Entities
{
    public partial class Tasks
    {
        public int Id { get; set; }

        public string Url { get; set; }

        public string Cmd { get; set; }

        public TaskTypeEnum Type { get; set; }
    }
}
