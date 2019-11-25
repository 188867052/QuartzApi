using Entities;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Xunit;

namespace Quartz.Tests
{
    public class AuthUnitTests
    {
        internal readonly Uri uri = new Uri("http://localhost:6006");
        private readonly string _token = null;
        private readonly QuartzDbContext dbContext = new QuartzDbContext();

        [Fact]
        public void get_token_test()
        {
            Assert.NotEmpty(this.Token);
        }

        [Fact]
        public void get_with_token_test()
        {
            var client = new HttpClient() { BaseAddress = uri };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", this.Token);
            var res = client.GetAsync("/api/Job/GetAllJob");
            var json = res.Result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, res.Result.StatusCode);
        }

        [Fact]
        public void TestGetWithoutToken()
        {
            var client = new HttpClient() { BaseAddress = uri };
            var res = client.GetAsync("/api/Job/GetAllJob");
            Assert.Equal(HttpStatusCode.Unauthorized, res.Result.StatusCode);
        }

        private string Token
        {
            get
            {
                if (!string.IsNullOrEmpty(_token))
                {
                    return _token;
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
                var res = client.GetAsync($"/api/Auth/Login?username={user.LoginName}&password={user.Password}");
                var json = res.Result.Content.ReadAsStringAsync().Result;
                var t = JsonConvert.DeserializeObject<dynamic>(json).token;
                Assert.Equal(HttpStatusCode.OK, res.Result.StatusCode);
                return t;
            }
        }
    }
}
