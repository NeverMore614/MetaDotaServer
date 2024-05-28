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
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class UsersController : ControllerBase
    {

        private readonly MDSDbContextFactory _contextFactory;
        public UsersController(MDSDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }



        [HttpGet]
        public async Task<ActionResult<LoginController.AccountInfo>> Get()
        {
            string idStr = "";
            if  (!MDSCommonTool.GetAuthValue(HttpContext,"id", ref idStr))
                return Unauthorized();

            int id = int.Parse(idStr);

            User user = await _contextFactory.GetUser(id);
            if (user == null)
            {
                return NotFound("User Not Found");
            }

            return Ok(MDSCommonTool.CreateAccount("", user));
        }


        [HttpGet("RequestMatch")]
        public async Task<ActionResult<LoginController.AccountInfo>> RequestMatch(string matchRequest)
        {
            string idStr = "";
            if (!MDSCommonTool.GetAuthValue(HttpContext, "id", ref idStr))
                return Unauthorized();

            int id = int.Parse(idStr);

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

            return Ok(MDSCommonTool.CreateAccount("", user));

        }


        [HttpGet("Pay")]
        public async Task<ActionResult<LoginController.AccountInfo>> Pay(string payload)
        {
            if (MDSPaymentValidator.Validate(payload))
            {
                string idStr = "";
                if (!MDSCommonTool.GetAuthValue(HttpContext, "id", ref idStr))
                    return Unauthorized();

                int id = int.Parse(idStr);

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

                return Ok(MDSCommonTool.CreateAccount("", user));
            }
            else
            { 
                return BadRequest("Invalid Payload");
            }
        }
    }
}
