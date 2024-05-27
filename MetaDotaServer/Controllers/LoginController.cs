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

namespace MetaDotaServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {

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

        [HttpGet]
        public async Task<ActionResult<bool>> EmailLogin(string email)
        { 
            if (string.IsNullOrEmpty(email) || !MDSCommonTool.CheckEmail(email))
            {
                return Ok(false);
            }

            string jwt = CreateToken(user.Id, 2592000);

            var result = _smtpSender.Send(email, "21313", "12312332").Result;
            return Ok(result);
        }


        [Obsolete]
        public async Task<ActionResult<AccountInfo>> Get(string token)
        {
            //验证登录token
            string accountId;
            int expireTime;
            if (MDSTokenValidator.Validate(token, out accountId, out expireTime))
            {
                User user;
                using (MetaDotaServer.Data.TokenContext tokenContext = _contextFactory.CreateTokenDb())
                {
                    var info = await tokenContext.Token.FindAsync(accountId);
                    if (info == null)
                    {
                        user = NewUser();
                        using (MetaDotaServer.Data.UserContext userContext = _contextFactory.CreateUserDb())
                        {
                            EntityEntry<User> newUser = await userContext.User.AddAsync(user);
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
                string jwt = CreateToken(user.Id, expireTime);
                return Ok(MDSCommonTool.CreateAccount(jwt, user));

            }

            return Ok(_invaildAccount);
        }

        private string CreateToken(int id, int expireTime)
        {
            // 1. 定义需要使用到的Claims
            var claims = new[]
            {
            new Claim("id", id.ToString()),
            };

            // 2. 从 appsettings.json 中读取SecretKey
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));

            // 3. 选择加密算法
            var algorithm = SecurityAlgorithms.HmacSha256;

            // 4. 生成Credentials
            var signingCredentials = new SigningCredentials(secretKey, algorithm);

            // 5. 根据以上，生成token
            var jwtSecurityToken = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],    //Issuer
                _configuration["Jwt:Audience"],  //Audience
                claims,                          //Claims,
                DateTime.Now,                    //notBefore
                DateTime.Now.AddSeconds(expireTime),     //expires
                signingCredentials               //Credentials
            );

            // 6. 将token变为string
            var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            return token;
        }

        private User NewUser()
        {
            return new User()
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
