using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quartz.Api.Models;
using System.Threading.Tasks;

namespace QuartzApi.Controllers
{
    /// <summary>
    /// Job.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]/[Action]")]
    public class JobController : ControllerBase
    {
        private readonly QuartzDbContext dbContext;

        public JobController(QuartzDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        public void  AddJob([FromBody]ScheduleEntity entity)
        {
        }
    }
}
