using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Entities
{
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
