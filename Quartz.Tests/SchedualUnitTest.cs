using Newtonsoft.Json;
using Quartz.SelfHost.Common;
using Quartz.SelfHost.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Quartz.Tests
{
    public class SchedualUnitTest
    {
        private static readonly HttpClient client = new HttpClient();

        [Fact]
        public async Task stop_job_not_exists_test_async()
        {
            var scheduler = SchedulerCenter.Instance;
            var jobKey = new JobKey(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var response = await scheduler.PauseJobAsync(jobKey);
            Assert.Equal(response.Message, $"jobKey: Name={jobKey.Name}, Group={jobKey.Group}不存在！");
        }

        [Fact]
        public async Task delete_job_not_exists_test_async()
        {
            var scheduler = SchedulerCenter.Instance;
            var jobKey = new JobKey(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var response = await scheduler.DeleteJobAsync(jobKey);
            Assert.Equal(response.Message, $"jobKey: Name={jobKey.Name}, Group={jobKey.Group}不存在！");
        }

        [Fact]
        public async Task add_http_job()
        {
            string json = "{\"jobGroup\":\"default\",\"jobName\":\"百度\",\"requestUrl\":\"https://www.baidu.com/\",\"beginTime\":\"2019-11-21T06:49:53.129Z\",\"endTime\":\"2019-11-24T06:49:57.142Z\",\"triggerType\":\"2\",\"requestType\":\"0\",\"headers\":null,\"requestParameters\":null,\"description\":null,\"cron\":null,\"intervalSecond\":5,\"mailMessage\":\"0\"}";
            ScheduleModel entity = JsonConvert.DeserializeObject<ScheduleModel>(json);
            var scheduler = SchedulerCenter.Instance;
            await scheduler.AddScheduleJobAsync(entity);
        }

        [Fact]
        public async Task add_cmd_job()
        {
            var entity = new ScheduleModel()
            {
                JobGroup = "default",
                JobName = "cmd_test",
                BeginTime = DateTime.Now,
                EndTime = DateTime.Now.AddDays(1),
                IntervalSecond = 5,
                IsExcuteCmd = true,
                CmdPath = "./test.cmd",
            };
            var scheduler = SchedulerCenter.Instance;
            await scheduler.AddScheduleJobAsync(entity);
        }

        [Fact]
        public async Task get_all_job()
        {
            SchedulerCenter scheduler = SchedulerCenter.Instance;
            var jobs = await scheduler.GetAllJobAsync();
        }
    }
}
