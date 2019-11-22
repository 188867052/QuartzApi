using System;
using System.Collections.Generic;

namespace Entities
{
    public partial class TaskLogs
    {
        public int Id { get; set; }

        public string Message { get; set; }

        public DateTime? CreateTime { get; set; }
    }
}
