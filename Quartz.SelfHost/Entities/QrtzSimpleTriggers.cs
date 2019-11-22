using System;
using System.Collections.Generic;

namespace Entities
{
    public partial class QrtzSimpleTriggers
    {
        public string SchedName { get; set; }

        public string TriggerName { get; set; }

        public string TriggerGroup { get; set; }

        public int RepeatCount { get; set; }

        public long RepeatInterval { get; set; }

        public int TimesTriggered { get; set; }

        public virtual QrtzTriggers QrtzTriggers { get; set; }
    }
}
