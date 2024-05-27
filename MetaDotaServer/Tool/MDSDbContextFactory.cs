using MetaDotaServer.Data;
using MetaDotaServer.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Options;
using static MetaDotaServer.Tool.MDSDbContextFactory;

namespace MetaDotaServer.Tool
{
    public class MDSDbContextFactory
    {
        public struct MatchRequest
        {
            public int Id { get; set; }
            public string RequestStr { get; set; }
            public int time { get; set; }

            public bool isEmpty()
            { 
                return string.IsNullOrEmpty(RequestStr);
            }
        }

        private MatchRequest empty = new MatchRequest();

        private static MDSDbContextFactory _instance;

        private Queue<MatchRequest> _requestQueue;


        private readonly IConfiguration _configuration;
        public MDSDbContextFactory(IConfiguration configuration) {
            _configuration = configuration;

            _requestQueue = new Queue<MatchRequest>();
            var userList = GetUsers();
            userList.Sort((a, b) => { return a.RequestTime - b.RequestTime; });
            for (int i = 0; i < userList.Count; i++)
            {
                if (userList[i].MatchRequestState == MatchRequestState.Waiting || userList[i].MatchRequestState == MatchRequestState.Generating)
                {
                    _requestQueue.Enqueue(new MatchRequest
                    {
                        Id = userList[i].Id,
                        RequestStr = userList[i].RequestMatch,
                        time = userList[i].RequestTime,
                    });
                }
            }
            _instance = this;
        }

        public static void PutMatchRequest(User user)
        {

            if (user == null || user.MatchRequestState != MatchRequestState.Waiting || string.IsNullOrEmpty(user.RequestMatch)) return;

            _instance._requestQueue.Enqueue(new MatchRequest()
            {
                Id = user.Id,
                RequestStr = user.RequestMatch,
                time = user.RequestTime,
            });
        }

        public MatchRequest GetMatchRequest()
        {
            lock (_requestQueue)
            {
                if (_requestQueue.Count == 0)
                {
                    return empty;
                }
                return _requestQueue.Dequeue();
            }

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

        public List<User> GetUsers()
        {
            using (MetaDotaServer.Data.UserContext userContext = CreateUserDb())
            {
                return userContext.User.ToList();
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

        internal Task<User> GetUser(object id)
        {
            throw new NotImplementedException();
        }
    }
}
