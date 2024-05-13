using MetaDotaServer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MetaDotaServer.Tool
{
    public class DbContextFactory
    {

        private readonly IConfiguration _configuration;
        public DbContextFactory(IConfiguration configuration) {
            _configuration = configuration;
        }

        public TokenContext CreateTokenDb()
        {
            var options = new DbContextOptionsBuilder<TokenContext>()
           .UseSqlServer(_configuration.GetConnectionString("TokenContext") ?? throw new InvalidOperationException("Connection string 'UserContext' not found."))
           .Options;

            return new TokenContext(options);
        }

        public UserContext CreateUserDb()
        {
            var options = new DbContextOptionsBuilder<UserContext>()
           .UseSqlServer(_configuration.GetConnectionString("UserContext") ?? throw new InvalidOperationException("Connection string 'UserContext' not found."))
           .Options;

            return new UserContext(options);
        }
    }
}
