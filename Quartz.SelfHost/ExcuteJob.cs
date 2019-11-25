﻿using Newtonsoft.Json;
using Quartz;
using Quartz.SelfHost.Common;
using Quartz.SelfHost.Controllers;
using Quartz.SelfHost.Enums;
using Quartz.SelfHost.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Talk.Extensions;
using HttpMethod = Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpMethod;

namespace Quartz.SelfHost
{
    [DisallowConcurrentExecution]
    [PersistJobDataAfterExecution]
    public class ExcuteJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var isExcuteCmd = context.JobDetail.JobDataMap.GetString(Constant.IsExcuteCmd);
            if (isExcuteCmd == bool.TrueString)
            {
                ExecuteCmdJob(context);
            }
            else
            {
                await ExecuteHttpJob(context);
            }
        }

        public void ExecuteCmdJob(IJobExecutionContext context)
        {
            using Process proc = new Process();
            proc.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
            proc.StartInfo.FileName = context.JobDetail.JobDataMap.GetString(Constant.CmdPath);
            proc.Start();
            proc.WaitForExit();
            Log.Information(proc.StartInfo.FileName);
        }

        public async Task ExecuteHttpJob(IJobExecutionContext context)
        {
            var maxLogCount = 20;//最多保存日志数量
            var warnTime = 20;//接口请求超过多少秒记录警告日志         
            //获取相关参数
            var requestUrl = context.JobDetail.JobDataMap.GetString(Constant.RequestUrl);
            requestUrl = requestUrl?.IndexOf("http") == 0 ? requestUrl : "http://" + requestUrl;
            var requestParameters = context.JobDetail.JobDataMap.GetString(Constant.RequestParameters);
            var headersString = context.JobDetail.JobDataMap.GetString(Constant.Headers);
            var mailMessage = (MailMessageEnum)int.Parse(context.JobDetail.JobDataMap.GetString(Constant.MailMessage) ?? "0");
            var headers = headersString != null ? JsonConvert.DeserializeObject<Dictionary<string, string>>(headersString?.Trim()) : null;
            var requestType = (HttpMethod)int.Parse(context.JobDetail.JobDataMap.GetString(Constant.RequestType));

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Restart(); //  开始监视代码运行时间
            HttpResponseMessage response = new HttpResponseMessage();

            var loginfo = new LogInfoModel
            {
                Url = requestUrl,
                BeginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                RequestType = requestType,
                Parameters = requestParameters,
                JobName = $"{context.JobDetail.Key.Group}.{context.JobDetail.Key.Name}"
            };

            var logs = context.JobDetail.JobDataMap[Constant.LogList] as List<string> ?? new List<string>();
            if (logs.Count >= maxLogCount)
            {
                logs.RemoveRange(0, logs.Count - maxLogCount);
            }

            try
            {
                var http = HttpHelper.Instance;
                switch (requestType)
                {
                    case HttpMethod.Get:
                        response = await http.GetAsync(requestUrl, headers);
                        break;
                    case HttpMethod.Post:
                        response = await http.PostAsync(requestUrl, requestParameters, headers);
                        break;
                    case HttpMethod.Put:
                        response = await http.PutAsync(requestUrl, requestParameters, headers);
                        break;
                    case HttpMethod.Delete:
                        response = await http.DeleteAsync(requestUrl, headers);
                        break;
                    default:
                        throw new InvalidOperationException("暂不支持");
                }

                var result = HttpUtility.HtmlEncode(await response.Content.ReadAsStringAsync());
                stopwatch.Stop(); //  停止监视            
                double seconds = stopwatch.Elapsed.TotalSeconds;  //总秒数                                
                loginfo.EndTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                loginfo.Seconds = seconds;
                loginfo.Result = $"<span class='result'>{result.MaxLeft(1000)}</span>";
                if (!response.IsSuccessStatusCode)
                {
                    loginfo.ErrorMsg = $"<span class='error'>{result.MaxLeft(3000)}</span>";
                    await ErrorAsync(loginfo.JobName, new Exception(result.MaxLeft(3000)), JsonConvert.SerializeObject(loginfo), mailMessage);
                    context.JobDetail.JobDataMap[Constant.Exception] = JsonConvert.SerializeObject(loginfo);
                }
                else
                {
                    try
                    {
                        //这里需要和请求方约定好返回结果约定为HttpResultModel模型
                        var httpResult = JsonConvert.DeserializeObject<HttpResultModel>(HttpUtility.HtmlDecode(result));
                        if (!httpResult.IsSuccess)
                        {
                            loginfo.ErrorMsg = $"<span class='error'>{httpResult.ErrorMsg}</span>";
                            await ErrorAsync(loginfo.JobName, new Exception(httpResult.ErrorMsg), JsonConvert.SerializeObject(loginfo), mailMessage);
                            context.JobDetail.JobDataMap[Constant.Exception] = JsonConvert.SerializeObject(loginfo);
                        }
                        else
                            await InformationAsync(loginfo.JobName, JsonConvert.SerializeObject(loginfo), mailMessage);
                    }
                    catch (Exception)
                    {
                        await InformationAsync(loginfo.JobName, JsonConvert.SerializeObject(loginfo), mailMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop(); //  停止监视            
                double seconds = stopwatch.Elapsed.TotalSeconds;  //总秒数
                loginfo.ErrorMsg = $"<span class='error'>{ex.Message} {ex.StackTrace}</span>";
                context.JobDetail.JobDataMap[Constant.Exception] = JsonConvert.SerializeObject(loginfo);
                loginfo.Seconds = seconds;
                await ErrorAsync(loginfo.JobName, ex, JsonConvert.SerializeObject(loginfo), mailMessage);
            }
            finally
            {
                logs.Add($"<p>{JsonConvert.SerializeObject(loginfo)}</p>");
                context.JobDetail.JobDataMap[Constant.LogList] = logs;
                double seconds = stopwatch.Elapsed.TotalSeconds;  //总秒数
                if (seconds >= warnTime)//如果请求超过20秒，记录警告日志    
                {
                    await WarningAsync(loginfo.JobName, "耗时过长 - " + JsonConvert.SerializeObject(loginfo), mailMessage);
                }
            }
        }

        public async Task WarningAsync(string title, string msg, MailMessageEnum mailMessage)
        {
            Log.Logger.Warning(msg);
            if (mailMessage == MailMessageEnum.All)
            {
                await new SetingController().SendMail(new SendMailModel()
                {
                    Title = $"任务调度-{title}【警告】消息",
                    Content = msg
                });
            }
        }

        public async Task InformationAsync(string title, string msg, MailMessageEnum mailMessage)
        {
            Log.Logger.Information(msg);
            if (mailMessage == MailMessageEnum.All)
            {
                await new SetingController().SendMail(new SendMailModel()
                {
                    Title = $"任务调度-{title}消息",
                    Content = msg
                });
            }
        }

        public async Task ErrorAsync(string title, Exception ex, string msg, MailMessageEnum mailMessage)
        {
            Log.Logger.Error(ex, msg);
            if (mailMessage == MailMessageEnum.Err || mailMessage == MailMessageEnum.All)
            {
                await new SetingController().SendMail(new SendMailModel()
                {
                    Title = $"任务调度-{title}【异常】消息",
                    Content = msg
                });
            }
        }
    }
}
