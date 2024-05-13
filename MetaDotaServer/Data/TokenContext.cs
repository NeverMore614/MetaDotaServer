using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MetaDotaServer.Entity;

namespace MetaDotaServer.Data
{
    public class TokenContext : DbContext
    {
        public TokenContext (DbContextOptions<TokenContext> options)
            : base(options)
        {
        }

        public DbSet<MetaDotaServer.Entity.Token> Token { get; set; } = default!;
    }
}
