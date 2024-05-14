using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MetaDotaServer.Data;
using MetaDotaServer.Entity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MetaDotaServer.Tool;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Identity.Client;
using NuGet.Common;

namespace MetaDotaServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {

        private readonly DbContextFactory _contextFactory;
        public UsersController(DbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }



        [HttpGet]
        [Authorize]
        public async Task<ActionResult<LoginController.AccountInfo>> Get()
        {
            int id = 0;
            if  (!CommonTool.GetID(HttpContext, ref id))
                return Unauthorized();

            User user = await _contextFactory.GetUser(id);
            if (user == null)
            {
                return NotFound("User Not Found");
            }

            return Ok(CommonTool.CreateAccount("", user));
        }


        [HttpGet("RequestMatch")]
        [Authorize]
        public async Task<ActionResult<LoginController.AccountInfo>> RequestMatch(string matchRequest)
        {
            int id = 0;
            if (!CommonTool.GetID(HttpContext, ref id))
                return Unauthorized();

            User user = await _contextFactory.GetUser(id);
            if (user == null)
            {
                return NotFound("User Not Found");
            }

            if (!user.Request(matchRequest))
            {
                return BadRequest("Request Match Fail");
            }

            if (!await _contextFactory.SaveUser(user))
            { 
                return BadRequest("Save User Fail");
            }

            return Ok(CommonTool.CreateAccount("", user));

        }


        [HttpGet("Pay")]
        [Authorize]
        public async Task<ActionResult<LoginController.AccountInfo>> Pay(string payload)
        {
            if (PaymentValidator.Validate(payload))
            {
                int id = 0;
                if (!CommonTool.GetID(HttpContext, ref id))
                    return Unauthorized();

                User user = await _contextFactory.GetUser(id);
                if (user == null)
                {
                    return NotFound("User Not Found");
                }

                if (!user.Pay())
                {
                    return NotFound("Replay Url Not Fount");
                }

                if (!await _contextFactory.SaveUser(user))
                {
                    return BadRequest("Save User Fail");
                }

                return Ok(CommonTool.CreateAccount("", user));
            }
            else
            { 
                return BadRequest("Invalid Payload");
            }
        }
    }
}
