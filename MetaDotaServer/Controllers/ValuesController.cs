using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MetaDotaServer.Data;
using MetaDotaServer.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace MetaDotaServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {

        private readonly IConfiguration _configuration;

        public ValuesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public string Get()
        {
            var options = new DbContextOptionsBuilder<UserContext>()
.UseSqlServer(_configuration.GetConnectionString("UserContext") ?? throw new InvalidOperationException("Connection string 'UserContext' not found."))
.Options;
            UserContext userContext = new UserContext(options);

            userContext.User.ToList();
            return "success";
        }
    }
}
