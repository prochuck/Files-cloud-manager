using Files_cloud_manager.Server.Domain.Interfaces;
using Files_cloud_manager.Server.Models;

namespace Files_cloud_manager.Server.Domain
{
    public class FolderRepository : BaseRepository<Folder>, IFolderRepostiory
    {
        public FolderRepository(AppDBContext dbContext) : base(dbContext)
        {
        }
    }
}
