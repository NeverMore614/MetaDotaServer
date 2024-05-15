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

namespace MetaDotaServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ReplayBuilderController : ControllerBase
    {


        private readonly DbContextFactory _contextFactory;
        public ReplayBuilderController(DbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;

        }




        [HttpGet("GetMatchRequest")]
        public async Task<ActionResult<DbContextFactory.MatchRequest>> GetMatchRequest()
        {
            DbContextFactory.MatchRequest matchRequest = _contextFactory.GetMatchRequest();
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

        [HttpPost("GenerateOver")]
        public async Task<ActionResult<bool>> GenerateOver(int id, string state, string message)
        {
            User user = await _contextFactory.GetUser(id);
            if (user == null)
            {
                return false;
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
                return false;
            }
            return true;
        }
    }
}
