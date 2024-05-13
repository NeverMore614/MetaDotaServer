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

namespace MetaDotaServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly DbContextFactory _contextFactory;

        private AccountInfo _invaildAccount;
        public LoginController(IConfiguration configuration, DbContextFactory contextFactory)
        {
            _configuration = configuration;
            _invaildAccount = new AccountInfo()
            {
                Vaild = false
            };
            _contextFactory = contextFactory;
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
        public async Task<ActionResult<string>> Get(string token)
        {
            //验证登录token
            string accountId;
            int expireTime;
            if (TokenValidator.Validate(token, out accountId, out expireTime))
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
                return Ok(CreateAccount(jwt, user));

            }

            return Ok(_invaildAccount);
        }

        [HttpGet("auth")]
        [Authorize]
        public async Task<ActionResult<string>> auth()
        {
            
            var user = HttpContext.User;
            var claim = (ClaimsIdentity)HttpContext.User.Identity;
            var Id = Convert.ToInt32(claim.Claims.Where(x => x.Type.Contains("id")).FirstOrDefault().Value);
            return Ok("你成功了！你的id是：" + Id);
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
                GenerateUrl = ""
            };
        }
        private AccountInfo CreateAccount(string jwt, User user)
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

    }
}
