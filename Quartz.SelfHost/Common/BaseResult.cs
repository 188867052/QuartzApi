﻿namespace Quartz.SelfHost.Common
{
    public class BaseResult
    {
        public int Code { get; set; } = 200;
        public string RequestData { get; set; }
        public string Msg { get; set; }
    }
}
