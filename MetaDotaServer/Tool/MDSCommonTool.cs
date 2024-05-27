using MetaDotaServer.Entity;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using static MetaDotaServer.Controllers.LoginController;
using System.Text.RegularExpressions;

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

        public static bool GetID(HttpContext httpContext, ref int id)
        {
            try
            {
                var claim = (ClaimsIdentity)httpContext.User.Identity;
                id = Convert.ToInt32(claim.Claims.Where(x => x.Type.Contains("id")).FirstOrDefault().Value);
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
    }
}
