using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Nancy.Json;
using Quartz.Api.Models;
using System;
using System.Buffers.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace QuartzApi.Controllers
{
    /// <summary>
    /// 权限控制.
    /// </summary>
    [Route("api/[controller]/[Action]")]
    public class AuthController : ControllerBase
    {
        private readonly QuartzDbContext dbContext;
        private readonly AppAuthenticationSettings appSettings;

        public AuthController(QuartzDbContext dbContext, IOptions<AppAuthenticationSettings> appSettings)
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
            using (this.dbContext)
            {
                User user = this.dbContext.User.FirstOrDefault(x => x.LoginName == username.Trim());
                if (user == null || !user.IsEnable)
                {
                    return this.FailResponse("UserNotExist");
                }

                if (Helper.Decrypt(user.Password) != password.Trim())
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
                    message = "成功",
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

    public static class Helper
    {
        private static string pwd = "sdsf343434234234";
        // AES 的 key 支持 128 位，最大支持 256 位。256 位需要 32 个字节。
        // 所以这里使用密钥的前 32 字节作为 key ,不足 32 补 0。
        public static byte[] GetKey(string pwd)
        {
            while (pwd.Length < 32)
            {
                pwd += '0';
            }
            pwd = pwd.Substring(0, 32);
            return Encoding.UTF8.GetBytes(pwd);
        }
        // AES 加密的初始化向量,加密解密需设置相同的值。
        // 这里设置为 16 个 0。
        public static byte[] AES_IV = Encoding.UTF8.GetBytes("0000000000000000");

        // 加密
        public static string Encrypt(string data)
        {
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = GetKey(pwd);
                aesAlg.IV = AES_IV;
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(data);
                        }
                        byte[] bytes = msEncrypt.ToArray();
                        return Convert.ToBase64String(bytes);
                    }
                }
            }
        }

        // 解密
        public static string Decrypt(string data)
        {
            byte[] inputBytes = Convert.FromBase64String(data);

            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = GetKey(pwd);
                aesAlg.IV = AES_IV;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream(inputBytes))
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srEncrypt = new StreamReader(csEncrypt))
                        {
                            return srEncrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
