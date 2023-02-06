using Files_cloud_manager.Models;

namespace Files_cloud_manager.Server.Domain.Interfaces
{
    public interface IUserRepository : IBaseRepository<User>
    {
        public User? Find(int id);
        public User? Find(string login);
    }
}
