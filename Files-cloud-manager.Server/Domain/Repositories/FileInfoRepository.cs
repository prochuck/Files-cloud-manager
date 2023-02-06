using Files_cloud_manager.Server.Domain.Interfaces;
using Files_cloud_manager.Models;
using FileInfo = Files_cloud_manager.Models.FileInfo;

namespace Files_cloud_manager.Server.Domain
{
    public class FileInfoRepository : BaseRepository<FileInfo>, IFileInfoRepository
    {
        public FileInfoRepository(AppDBContext dbContext) : base(dbContext)
        {
        }
    }
}
