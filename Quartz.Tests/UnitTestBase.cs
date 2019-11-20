using Entities;
using Newtonsoft.Json;
using QuartzApi.Controllers;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Xunit;

namespace Quartz.Tests
{
    public class UnitTestBase
    {
        internal readonly Uri uri = new Uri("https://localhost:44395/");
        private readonly QuartzDbContext dbContext = new QuartzDbContext();
        private readonly string token;

        internal string Token
        {
            get
            {
                if (!string.IsNullOrEmpty(token))
                {
                    return token;
                }

                var user = dbContext.User.FirstOrDefault(o => o.LoginName == nameof(o.LoginName));
                if (user == null)
                {
                    user = new User()
                    {
                        LoginName = nameof(User.LoginName),
                        Password = nameof(User.Password),
                        CreateTime = DateTime.Now,
                        UpdateTime = DateTime.Now,
                        IsDeleted = false,
                        IsEnable = true,
                    };
                    dbContext.User.Add(user);
                }

                dbContext.SaveChanges();

                var client = new HttpClient() { BaseAddress = uri };
                var res = client.GetAsync($"/api/Auth/SignIn?username={user.LoginName}&password={user.Password}");
                var json = res.Result.Content.ReadAsStringAsync().Result;
                var t = JsonConvert.DeserializeObject<dynamic>(json).token;
                Assert.Equal(HttpStatusCode.OK, res.Result.StatusCode);
                return t;
            }
        }
    }
}
