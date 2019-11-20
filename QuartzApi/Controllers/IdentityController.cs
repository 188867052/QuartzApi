﻿using EFCore.Scaffolding.Extension;
using Entities;
using JWT.Algorithms;
using JWT.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz.Api.Models;
using System.Linq;
using System.Net;
using System.Security.Claims;

namespace QuartzApi.Controllers
{
    //https://github.com/jwt-dotnet/jwt
    /// <summary>
    /// 权限控制.
    /// </summary>
    [Route("api/[controller]/[Action]")]
    public class IdentityController : ControllerBase
    {
        private readonly QuartzDbContext dbContext;
        private readonly AppAuthenticationSettings appSettings;

        public IdentityController(QuartzDbContext dbContext, IOptions<AppAuthenticationSettings> appSettings)
        {
            this.dbContext = dbContext;
            this.appSettings = appSettings.Value;
        }

        /// <summary>
        /// Auth.
        /// </summary>
        /// <param name="username">username.</param>
        /// <param name="password">password.</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Auth(string username, string password)
        {
            using (this.dbContext)
            {
                User user = this.dbContext.User.AsNoTracking().FirstOrDefault(x => x.LoginName == username.Trim());
                if (user == null || !user.IsEnable)
                {
                    return this.FailResponse("UserNotExist");
                }

                if (user.Password != password.Trim())
                {
                    return this.FailResponse("PasswordWrong");
                }

                if (user.IsLocked)
                {
                    return this.FailResponse("Locked");
                }

                if (!user.IsEnable)
                {
                    return this.FailResponse("UserDisable");
                }

                var claimsIdentity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(nameof(Entities.User.Id), user.Id.ToString()),
                    new Claim(nameof(Entities.User.LoginName), user.LoginName),
                    new Claim(nameof(Entities.User.Password), user.Password),
                    new Claim(nameof(Entities.User.IsEnable),  user.IsEnable.ToString()),
                });
              

                return this.Ok(new
                {
                    token = AuthenticationConfiguration.GetJwtAccessToken(appSettings, claimsIdentity),
                    code = (int)HttpStatusCode.OK,
                    message = "OperateSuccess",
                });
            }
        }

        /// <summary>
        /// 创建用户.
        /// </summary>
        /// <param name="model">用户视图实体.</param>
        /// <returns>IActionResult.</returns>
        [HttpPost]
        public JsonResult Create(UserCreatePostModel model)
        {
            if (model.LoginName.Length == 0)
            {
                return new JsonResult("用户名不能为空");
            }
            if (model.Password.Length == 0)
            {
                return new JsonResult("密码不能为空");
            }

            using (this.dbContext)
            {
                if (dbContext.User.Any(x => x.LoginName == model.LoginName))
                {
                    return new JsonResult("User is Exist");
                }

                User entity = model.MapTo();
                this.dbContext.User.Add(entity);
                this.dbContext.SaveChanges();

                return new JsonResult("Success");
            }
        }

        private OkObjectResult FailResponse(string message)
        {
            return this.Ok(new
            {
                code = (int)HttpStatusCode.OK,
                message
            });
        }
    }
}
