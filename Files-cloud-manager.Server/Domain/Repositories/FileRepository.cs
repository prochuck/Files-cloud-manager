using Files_cloud_manager.Server.Domain.Interfaces;
using Files_cloud_manager.Server.Models;
using File = Files_cloud_manager.Server.Models.File;

namespace Files_cloud_manager.Server.Domain
{
    public class FileRepository : BaseRepository<File>, IFileRepository
    {
        public FileRepository(AppDBContext dbContext) : base(dbContext)
        {
        }
    }
}
