using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MetaDotaServer.Entity;

namespace MetaDotaServer.Data
{
    public class UserContext : DbContext
    {
        public UserContext (DbContextOptions<UserContext> options)
            : base(options)
        {
        }

        public DbSet<MetaDotaServer.Entity.User> User { get; set; } = default!;
    }
}
