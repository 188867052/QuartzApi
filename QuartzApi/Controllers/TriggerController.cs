using System.Collections.Generic;
using System.Linq;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Quartz.Api.Models;

namespace QuartzApi.Controllers
{
    /// <summary>
    /// 触发器.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]/[Action]")]
    public class TriggerController : ControllerBase
    {
        private readonly QuartzDbContext dbContext;
        private readonly SignInManager<User> signInManager;

        public TriggerController(QuartzDbContext dbContext, SignInManager<User> signInManager)
        {
            this.dbContext = dbContext;
            this.signInManager = signInManager;
        }

        /// <summary>
        /// 根据 TriggerName 和 JobGroup 获取.
        /// </summary>
        /// <param name="name">TriggerName</param>
        /// <param name="group">JobGroup</param>
        /// <returns></returns>
        [HttpGet]
        public HttpReponseModel GetByKey(string name, string group)
        {
            var trigger = dbContext.QrtzTriggers.FirstOrDefault(o => o.TriggerName == name && o.JobGroup == group);
            if (trigger == null)
            {
                return new HttpReponseModel() { Message = "Trigger不存在" };
            }

            return HttpReponseModel.Success(new TriggerInfo(trigger));
        }

        /// <summary>
        /// 获取所有 Trigger 信息.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpReponseModel GetAll()
        {
            var triggers = dbContext.QrtzTriggers.ToList();
            var list = new List<TriggerInfo>();
            foreach (var trigger in triggers)
            {
                list.Add(new TriggerInfo(trigger));
            }

            return HttpReponseModel.Success(list);
        }
    }
}
