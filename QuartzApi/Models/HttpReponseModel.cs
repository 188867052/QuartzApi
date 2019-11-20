namespace Quartz.Api.Models
{
    /// <summary>
    /// 表示http返回结果
    /// </summary>
    public class HttpReponseModel
    {
        /// <summary>
        /// 初始化一个<see cref="HttpReponseModel"/>类型的新实例
        /// </summary>
        public HttpReponseModel()
            : this(null)
        {
        }

        /// <summary>
        /// 初始化一个<see cref="HttpReponseModel"/>类型的新实例
        /// </summary>
        public HttpReponseModel(string content, HttpReponseCode type = HttpReponseCode.Success, object data = null)
            : this(content, data, type)
        {
        }

        /// <summary>
        /// 初始化一个<see cref="HttpReponseModel"/>类型的新实例
        /// </summary>
        public HttpReponseModel(string content, object data, HttpReponseCode type = HttpReponseCode.Success)
        {
            Code = type;
            Content = content;
            Data = data;
        }

        /// <summary>
        /// 获取或设置 Ajax操作结果类型
        /// </summary>
        public HttpReponseCode Code { get; set; }

        /// <summary>
        /// 获取或设置 消息内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 获取或设置 返回数据
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// 返回消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 成功的AjaxResult
        /// </summary>
        public static HttpReponseModel Success(object data = null)
        {
            return new HttpReponseModel("操作执行成功", HttpReponseCode.Success, data);
        }
    }
}