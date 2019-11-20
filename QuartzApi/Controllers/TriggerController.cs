﻿using System.Collections.Generic;
using System.Linq;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Quartz.Api.Models;

namespace QuartzApi.Controllers
{
    /// <summary>
    /// 触发器.
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[Action]")]
    public class TriggerController : ControllerBase
    {
        private readonly QuartzDbContext dbContext;

        public TriggerController(QuartzDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// 根据 TriggerName 和 JobGroup 获取.
        /// </summary>
        /// <param name="name">TriggerName</param>
        /// <param name="group">JobGroup</param>
        /// <returns></returns>
        [HttpGet]
        public TriggerInfo GetByKey(string name, string group)
        {
            var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IjEiLCJJZCI6IjEiLCJMb2dpbk5hbWUiOiIxIiwiUGFzc3dvcmQiOiIxIiwiSXNFbmFibGUiOiJUcnVlIiwibmJmIjoxNTc0MjIzNzQwLCJleHAiOjE1NzQ4Mjg1MzgsImlhdCI6MTU3NDIyMzc0MH0.0D8QPjd3xp0EvUPzn6pFmPeurvwC8Vwjo9sW96NhoBI";
            var trigger = dbContext.QrtzTriggers.FirstOrDefault(o => o.TriggerName == name && o.JobGroup == group);

            return trigger == null ? null : new TriggerInfo(trigger);
        }

        /// <summary>
        /// 获取所有 Trigger 信息.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<TriggerInfo> GetAll()
        {
            var triggers = dbContext.QrtzTriggers.ToList();
            var list = new List<TriggerInfo>();
            foreach (var trigger in triggers)
            {
                list.Add(new TriggerInfo(trigger));
            }
            return list;
        }
    }
}
