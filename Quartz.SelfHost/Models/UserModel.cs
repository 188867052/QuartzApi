﻿using System;
using Entities;

namespace Quartz.SelfHost.Model
{
    public class UserModel
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        internal User MapTo()
        {
            return new User
            {
                LoginName = LoginName,
                Password = Password,
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                IsDeleted = false,
                IsEnable = true,
            };
        }
    }
}