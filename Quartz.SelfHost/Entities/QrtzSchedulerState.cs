﻿using System;
using System.Collections.Generic;

namespace Entities
{
    public partial class QrtzSchedulerState
    {
        public string SchedName { get; set; }

        public string InstanceName { get; set; }

        public long LastCheckinTime { get; set; }

        public long CheckinInterval { get; set; }
    }
}
