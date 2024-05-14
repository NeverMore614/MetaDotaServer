using MetaDotaServer.Data;
using MetaDotaServer.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
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

        public async Task<User> GetUser(int id)
        {
            using (MetaDotaServer.Data.UserContext userContext = CreateUserDb())
            {
                return await userContext.User.FindAsync(id);
            }
        }

        public async Task<bool> SaveUser(User user)
        {
            using (MetaDotaServer.Data.UserContext userContext = CreateUserDb())
            {
                userContext.User.Entry(user).State = EntityState.Modified;
                try
                {
                    await userContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return false;
                }
                
                return true;
            }
        }
    }
}
