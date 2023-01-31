using Files_cloud_manager.Server.Domain.Interfaces;
using Files_cloud_manager.Server.Models;

namespace Files_cloud_manager.Server.Domain
{
    public class FileInfoGroupRepository : BaseRepository<FileInfoGroup>, IFileInfoGroupRepostiory
    {
        public FileInfoGroupRepository(AppDBContext dbContext) : base(dbContext)
        {
        }
    }
}
