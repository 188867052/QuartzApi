using Newtonsoft.Json;
using Quartz.SelfHost;
using Quartz.SelfHost.Models;
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Quartz.Tests
{
    public class UnitTest
    {
        private static readonly HttpClient client = new HttpClient();

        [Fact]
        public void Test()
        {
            client.BaseAddress = new Uri("http://localhost:5000");
        }

        [Fact]
        public void SqliteConnectionTest()
        {
            using DbConnection conn = new SqlConnection();
            conn.ConnectionString = "Data Source=.;Initial Catalog=quartz;Integrated Security=True";
            conn.Open();
            DbCommand comm = conn.CreateCommand();
            comm.CommandText = "select * from QRTZ_JOB_DETAILS";
            comm.CommandType = CommandType.Text;
            using IDataReader reader = comm.ExecuteReader();
            Assert.Equal(ConnectionState.Open, conn.State);
        }

        [Fact]
        public async Task AddJob()
        {
            string json = "{\"jobGroup\":\"default\",\"jobName\":\"百度\",\"requestUrl\":\"https://www.baidu.com/\",\"beginTime\":\"2019-11-21T06:49:53.129Z\",\"endTime\":\"2019-11-24T06:49:57.142Z\",\"triggerType\":\"2\",\"requestType\":\"1\",\"headers\":null,\"requestParameters\":null,\"description\":null,\"cron\":null,\"intervalSecond\":1,\"mailMessage\":\"0\"}";
            ScheduleEntity entity = JsonConvert.DeserializeObject<ScheduleEntity>(json);
            var scheduler = SchedulerCenter.Instance;
            await scheduler.AddScheduleJobAsync(entity);
        }

        [Fact]
        public async Task AddJobByPost()
        {
            string json = "{\"jobGroup\":\"default\",\"jobName\":\"百度\",\"requestUrl\":\"https://www.baidu.com/\",\"beginTime\":\"2019-11-21T06:49:53.129Z\",\"endTime\":\"2019-11-24T06:49:57.142Z\",\"triggerType\":\"2\",\"requestType\":\"1\",\"headers\":null,\"requestParameters\":null,\"description\":null,\"cron\":null,\"intervalSecond\":1,\"mailMessage\":\"0\"}";
            var httpContent = new StringContent(json);
            var a = client.PostAsync("http://localhost:5000/api/Job/AddJob", httpContent);
            var b = a.Result.Content.ReadAsStringAsync().Result;
        }

        [Fact]
        public async Task GetAllJob()
        {
            SchedulerCenter scheduler = SchedulerCenter.Instance;
            var jobs = await scheduler.GetAllJobAsync();
        }

        [Fact]
        public async Task StopJob()
        {
            SchedulerCenter scheduler = SchedulerCenter.Instance;
            var jobs = await scheduler.DeleteJobAsync(new JobKey("sad", "asd")); ;
        }

        //[Fact]
        //public void TestGetToken()
        //{
        //    Assert.NotEmpty(this.Token);
        //}

        //[Fact]
        //public void TestGetWithToken()
        //{
        //    var client = new HttpClient() { BaseAddress = uri };
        //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", this.Token);
        //    var res = client.GetAsync("/api/Trigger/GetAll");
        //    var json = res.Result.Content.ReadAsStringAsync().Result;
        //    Assert.Equal(HttpStatusCode.OK, res.Result.StatusCode);
        //}

        //[Fact]
        //public void TestGetWithoutToken()
        //{
        //    var client = new HttpClient() { BaseAddress = uri };
        //    var res = client.GetAsync("/api/Trigger/GetAll");
        //    Assert.Equal(HttpStatusCode.Unauthorized, res.Result.StatusCode);
        //}
    }
}
