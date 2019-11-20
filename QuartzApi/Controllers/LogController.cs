using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QuartzApi.Controllers
{
    /// <summary>
    /// 日志.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]/[Action]")]
    public class LogController : ControllerBase
    {
        private readonly QuartzDbContext dbContext;

        public LogController(QuartzDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
    }
}
