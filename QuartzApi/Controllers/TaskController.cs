using EFCore.Scaffolding.Extension;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quartz.Api.Enums;
using Quartz.Api.Models;

namespace QuartzApi.Controllers
{
    /// <summary>
    /// 任务.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]/[Action]")]
    public class TaskController : ControllerBase
    {
        private readonly QuartzDbContext dbContext;

        public TaskController(QuartzDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public HttpReponseModel AddUrlTask(string url)
        {
            Check.NotEmpty(url, nameof(url));
            var task = new Tasks()
            {
                Url = url,
                Type = TaskTypeEnum.Url,
            };

            this.dbContext.Tasks.Add(task);
            this.dbContext.SaveChanges();

            return HttpReponseModel.Success();
        }

        [HttpGet]
        public HttpReponseModel AddCmdTask(string cmd)
        {
            Check.NotEmpty(cmd, nameof(cmd));
            var task = new Tasks()
            {
                Cmd = cmd,
                Type = TaskTypeEnum.Cmd,
            };

            this.dbContext.Tasks.Add(task);
            this.dbContext.SaveChanges();

            return HttpReponseModel.Success();
        }
    }
}
