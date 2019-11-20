using System.ComponentModel.DataAnnotations;

namespace Quartz.Api.Models
{
    /// <summary>
    /// Token请求DTO
    /// </summary>
    public class TokenDto
    {
        /// <summary>
        /// 获取或设置 授权类型
        /// </summary>
        [Required]
        public string GrantType { get; set; }

        /// <summary>
        /// 获取或设置 账户名
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 获取或设置 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 获取或设置 验证码
        /// </summary>
        public string VerifyCode { get; set; }

        /// <summary>
        /// 获取或设置 刷新Token
        /// </summary>
        public string RefreshToken { get; set; }
    }
}