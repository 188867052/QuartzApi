using System.Linq;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Quartz.Api.Models;

namespace QuartzApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
        public TriggerInfo Get(string name, string group)
        {
            var trigger = dbContext.QrtzTriggers.FirstOrDefault(o => o.TriggerName == name && o.JobGroup == group);

            return trigger == null ? null : new TriggerInfo(trigger);
        }
    }
}
