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
using static MetaDotaServer.Controllers.ReplayBuilderController;
using Microsoft.AspNetCore.Cors;

namespace MetaDotaServer.Controllers
{
    [EnableCors]
    [Route("api/[controller]")]
    [ApiController]
    public class ReplayBuilderController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly MDSDbContextFactory _contextFactory;
        public ReplayBuilderController(MDSDbContextFactory contextFactory, IConfiguration configuration)
        {
            _contextFactory = contextFactory;
            _configuration = configuration;
        }

        [HttpGet]
        public string GetAuth()
        {
            Claim[] claims = new Claim[] { };
            // 2. 从 appsettings.json 中读取SecretKey
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt2:SecretKey"]));

            // 3. 选择加密算法
            var algorithm = SecurityAlgorithms.HmacSha256;

            // 4. 生成Credentials
            var signingCredentials = new SigningCredentials(secretKey, algorithm);

            // 5. 根据以上，生成token
            var jwtSecurityToken = new JwtSecurityToken(
                _configuration["Jwt2:Issuer"],    //Issuer
                _configuration["Jwt2:Audience"],  //Audience
                claims,                          //Claims,
                DateTime.Now,                    //notBefore
                DateTime.Now.AddSeconds(60 * 60 * 24 * 365 * 20),     //expires
                signingCredentials               //Credentials
            );

            // 6. 将token变为string
            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }

        [Authorize(AuthenticationSchemes = "BearerReplayBuilder")]
        [HttpGet("GetMatchRequest")]
        public async Task<ActionResult<MDSDbContextFactory.MatchRequest>> GetMatchRequest()
        {
            MDSDbContextFactory.MatchRequest matchRequest = _contextFactory.GetMatchRequest();
            if (matchRequest.isEmpty())
                return NotFound("no match");

            User user = await _contextFactory.GetUser(matchRequest.Id);
            if (user == null)
            {
                return NotFound("no user");
            }
            if (!user.RequestMatch.Equals(matchRequest.RequestStr))
            {
                return NotFound("user update request");
            }
            if (!user.StartGenerate())
            {
                return BadRequest("User Start Generate Fail");
            }
            if (!await _contextFactory.SaveUser(user))
            {
                return BadRequest("Save User Fail");
            }
            return Ok(matchRequest);

        }

        [Authorize(AuthenticationSchemes = "BearerReplayBuilder")]
        [HttpPost("GenerateOver")]
        public async Task<ActionResult<bool>> GenerateOver(int id, string state, string message)
        {
            User user = await _contextFactory.GetUser(id);
            if (user == null)
            {
                return NotFound();
            }
            if (state.Equals("success"))
            {
                user.GenerateSuccess(message);
            }
            else if (state.Equals("fail"))
            {
                user.GenerateFail(message);
            }
            if (!await _contextFactory.SaveUser(user))
            {
                return Problem();
            }
            return Ok(true);
        }
    }
}
