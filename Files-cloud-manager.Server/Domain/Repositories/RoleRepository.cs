using Files_cloud_manager.Server.Domain.Interfaces;
using Files_cloud_manager.Server.Models;

namespace Files_cloud_manager.Server.Domain
{
    public class RoleRepository : BaseRepository<Role>, IRoleRepository
    {
        public RoleRepository(AppDBContext dbContext) : base(dbContext)
        {
        }
    }
}
