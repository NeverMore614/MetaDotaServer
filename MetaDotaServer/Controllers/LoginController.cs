using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MetaDotaServer.Data;
using MetaDotaServer.Entity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using MetaDotaServer.Tool;
using NuGet.Common;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Swashbuckle.Swagger;
using Microsoft.Identity.Client;
using MetaDotaServer.Migrations.User;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.AspNetCore.Identity;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;
using static System.Net.Mime.MediaTypeNames;
using System.Security.Policy;
using System.Collections;
using Org.BouncyCastle.Crypto;

namespace MetaDotaServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private const string EmailSubject = "MetaDota邮件登录";
        private const string EmailBody = "请点击以下链接完成登录：\n";
        private readonly IConfiguration _configuration;
        private readonly MDSDbContextFactory _contextFactory;
        private readonly MDSEmailSender _smtpSender;

        private AccountInfo _invaildAccount;
        public LoginController(IConfiguration configuration, MDSDbContextFactory contextFactory, MDSEmailSender smtpSender)
        {
            _configuration = configuration;
            _invaildAccount = new AccountInfo()
            {
                Vaild = false
            };
            _contextFactory = contextFactory;
            _smtpSender = smtpSender;
        }


        public struct UserInfo
        {
            public string Name { get; set; }
            public string VideoUrl { get; set; }
            public string RequestMatch { get; set; }
            public MetaDotaServer.Entity.MatchRequestState MatchState { get; set; }
            public string message { get; set; }
        }
        public struct AccountInfo
        {
            public bool Vaild { get; set; }
            public string Jwt { get; set; }
            public UserInfo UserInfo { get; set; }
        }
        [HttpGet("PreLogin")]
        public async Task<ActionResult> PreLogin()
        {


            // 生成随机验证码
            string code = MDSCommonTool.GenerateRandomCode();
            string md5Img = MDSCommonTool.GenerateRandomCodeImage(code);

            //jwt
            string jwt = CreateToken(new Hashtable() { ["code"] = code }, 15, "Jwt3");


            var response = new
            {
                Img = md5Img,
                Jwt = jwt
            };

            return Ok(response);
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "BearerLogin")]
        public async Task<ActionResult<bool>> EmailLogin(string email, string redicUrl, string authcode)
        {
            //验证码校验
            string code = "";
            if (!MDSCommonTool.GetAuthValue(HttpContext, "code", ref code))
                return Unauthorized();

            if (!authcode.Equals(code))
                return Unauthorized();


            if (string.IsNullOrEmpty(email) || !MDSCommonTool.CheckEmail(email))
            {
                return Ok(false);
            }

            Entity.User user = CreatrOrGetUser(email).Result;

            string jwt = CreateToken(new Hashtable() { ["id"] = user.Id }, 2592000, "Jwt");

            return Ok(_smtpSender.Send(email, EmailSubject, EmailBody + redicUrl + jwt).Result);
        }

        public async Task<Entity.User> CreatrOrGetUser(string accountId)
        {
            Entity.User user;
            using (MetaDotaServer.Data.TokenContext tokenContext = _contextFactory.CreateTokenDb())
            {
                var info = await tokenContext.Token.FindAsync(accountId);
                if (info == null)
                {
                    user = NewUser();
                    using (MetaDotaServer.Data.UserContext userContext = _contextFactory.CreateUserDb())
                    {
                        EntityEntry<Entity.User> newUser = await userContext.User.AddAsync(user);
                        await userContext.SaveChangesAsync();
                        user.Id = newUser.Entity.Id;
                    }
                    info = new Entity.Token()
                    {
                        TokenStr = accountId,
                        Id = user.Id
                    };
                    await tokenContext.Token.AddAsync(info);
                    await tokenContext.SaveChangesAsync();
                }
                else
                {
                    using (MetaDotaServer.Data.UserContext userContext = _contextFactory.CreateUserDb())
                    {
                        user = await userContext.User.FindAsync(info.Id);
                    }
                }

            }
            return user;
        }

        [Obsolete]
        public async Task<ActionResult<AccountInfo>> Get(string token)
        {
            //验证登录token
            string accountId;
            int expireTime;
            if (MDSTokenValidator.Validate(token, out accountId, out expireTime))
            {
                Entity.User user = CreatrOrGetUser(accountId).Result;

                string jwt = CreateToken(new Hashtable() { ["id"] = user.Id }, expireTime, "Jwt");
                return Ok(MDSCommonTool.CreateAccount(jwt, user));

            }

            return Ok(_invaildAccount);
        }

        private string CreateToken(Hashtable kv, int expireTime, string jwtName)
        {
            // 1. 定义需要使用到的Claims
            Claim[] claims = new Claim[kv.Count];
            int i = 0;
            foreach (string key in kv.Keys)
            {
                claims[i++] = new Claim(key, kv[key].ToString());
            }

            // 2. 从 appsettings.json 中读取SecretKey
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration[$"{jwtName}:SecretKey"]));

            // 3. 选择加密算法
            var algorithm = SecurityAlgorithms.HmacSha256;

            // 4. 生成Credentials
            var signingCredentials = new SigningCredentials(secretKey, algorithm);

            // 5. 根据以上，生成token
            var jwtSecurityToken = new JwtSecurityToken(
                _configuration[$"{jwtName}:Issuer"],    //Issuer
                _configuration[$"{jwtName}:Audience"],  //Audience
                claims,                          //Claims,
                DateTime.Now,                    //notBefore
                DateTime.Now.AddSeconds(expireTime),     //expires
                signingCredentials               //Credentials
            );

            // 6. 将token变为string
            var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            return token;
        }

        private Entity.User NewUser()
        {
            return new Entity.User()
            {
                Name = new Guid().ToString(),
                VideoUrl = "",
                RequestMatch = "",
                ErrorMessage = "",
                MatchRequestState = MatchRequestState.None,
                GenerateUrl = ""
            };
        }


    }
}
