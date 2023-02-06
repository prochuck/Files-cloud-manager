using Files_cloud_manager.Server.Domain.Interfaces;
using Files_cloud_manager.Models;
using Microsoft.EntityFrameworkCore;

namespace Files_cloud_manager.Server.Domain
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(AppDBContext dbContext) : base(dbContext)
        {
        }


        

        public User? Find(int id)
        {
            return base.Get(e => e.Id == id, new string[] { nameof(User.Role) }).FirstOrDefault();
        }

        public User? Find(string login)
        {
            return base.Get(e => e.Login == login, new string[] { nameof(User.Role) }).FirstOrDefault();
        }
    }
}
