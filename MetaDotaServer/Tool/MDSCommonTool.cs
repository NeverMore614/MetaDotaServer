using MetaDotaServer.Entity;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using static MetaDotaServer.Controllers.LoginController;
using System.Text.RegularExpressions;
using System.Drawing.Imaging;
using System.Drawing;
using Microsoft.IdentityModel.Tokens;
using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace MetaDotaServer.Tool
{
    public class MDSCommonTool
    {
        // Unix时间戳是从1970年1月1日开始计算
        public static DateTime UnixStartTime = new DateTime(1970, 1, 1);

        public static AccountInfo CreateAccount(string jwt, User user)
        {
            UserInfo userInfo = new UserInfo();
            userInfo.Name = user.Name;
            userInfo.VideoUrl = user.VideoUrl;
            userInfo.RequestMatch = user.RequestMatch;
            userInfo.MatchState = user.MatchRequestState;
            userInfo.message = user.ErrorMessage;

            return new AccountInfo()
            {
                Vaild = true,
                Jwt = jwt,
                UserInfo = userInfo
            };
        }

        public static bool GetAuthValue(HttpContext httpContext, string claimKey,  ref string value)
        {
            try
            {
                var claim = (ClaimsIdentity)httpContext.User.Identity;
                value = (claim.Claims.Where(x => x.Type.Contains(claimKey)).FirstOrDefault().Value);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        static string _email_regex = "^[a-zA-Z0-9_-]+@[a-zA-Z0-9_-]+(\\.[a-zA-Z0-9_-]+)+$";
        public static bool CheckEmail(string address)
        {
            return Regex.IsMatch(address, _email_regex);
        }

        public static string GenerateRandomCode()
        {
            Random random = new Random();
            char[] letters = new char[4];

            for (int i = 0; i < letters.Length; i++)
            {
                // 生成一个随机的小写字母（a-z）
                letters[i] = (char)(random.Next(97, 123));
            }

            return new string(letters);
        }

        public static string GenerateRandomCodeImage(string code)
        {
            Bitmap bmp = new Bitmap(70, 30);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            // 绘制验证码
            g.DrawString(code, new System.Drawing.Font("Arial", 20), Brushes.Black, 0, 0);

            // 可以添加噪声线等复杂因素
            var random = new Random();
            for (var i = 0; i < 25; i++)
            {
                var x1 = random.Next(100);
                var x2 = random.Next(100);
                var y1 = random.Next(30);
                var y2 = random.Next(30);

                g.DrawLine(new Pen(Color.Silver), x1, y1, x2, y2);
            }
            // 将图片转换为内存流
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            return Convert.ToBase64String(ms.ToArray());
        }
    }
}
