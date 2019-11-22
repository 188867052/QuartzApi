using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Quartz.SelfHost.Model;
using System.Linq;
using System.Net;
using System.Security.Claims;

namespace Quartz.SelfHost.Controllers
{
    /// <summary>
    /// 权限控制.
    /// </summary>
    [Route("api/[controller]/[Action]")]
    public class AuthController : ControllerBase
    {
        private readonly QuartzDbContext dbContext;
        private readonly AuthenticationSettings appSettings;

        public AuthController(QuartzDbContext dbContext, IOptions<AuthenticationSettings> appSettings)
        {
            this.dbContext = dbContext;
            this.appSettings = appSettings.Value;
        }

        /// <summary>
        /// 登陆并返回token.
        /// </summary>
        /// <param name="username">username.</param>
        /// <param name="password">password.</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult SignIn(string username, string password)
        {
            using (dbContext)
            {
                User user = dbContext.User.FirstOrDefault(x => x.LoginName == username.Trim());
                if (user == null || !user.IsEnable)
                {
                    return FailResponse("UserNotExist");
                }

                if (user.Password != password.Trim())
                {
                    return FailResponse("PasswordWrong");
                }

                if (user.IsLocked)
                {
                    return FailResponse("Locked");
                }

                if (!user.IsEnable)
                {
                    return FailResponse("UserDisable");
                }

                var claimsIdentity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(nameof(Entities.User.Id), user.Id.ToString()),
                    new Claim(nameof(Entities.User.LoginName), user.LoginName),
                    new Claim(nameof(Entities.User.Password), user.Password),
                    new Claim(nameof(Entities.User.IsEnable),  user.IsEnable.ToString()),
                });

                return Ok(new
                {
                    token = AuthenticationConfiguration.GetJwtAccessToken(appSettings, claimsIdentity),
                    code = (int)HttpStatusCode.OK,
                    message = "成功",
                });
            }
        }

        /// <summary>
        /// 创建用户.
        /// </summary>
        /// <param name="model">model.</param>
        /// <returns>JsonResult.</returns>
        [HttpPost]
        public JsonResult Create(UserCreateModel model)
        {
            if (model.LoginName.Length == 0)
            {
                return new JsonResult("用户名不能为空");
            }
            if (model.Password.Length == 0)
            {
                return new JsonResult("密码不能为空");
            }

            using (dbContext)
            {
                if (dbContext.User.Any(x => x.LoginName == model.LoginName))
                {
                    return new JsonResult("User is Exist");
                }

                User entity = model.MapTo();
                dbContext.User.Add(entity);
                dbContext.SaveChanges();

                return new JsonResult("Success");
            }
        }

        private OkObjectResult FailResponse(string message)
        {
            return Ok(new
            {
                code = (int)HttpStatusCode.OK,
                message
            });
        }
    }
}
