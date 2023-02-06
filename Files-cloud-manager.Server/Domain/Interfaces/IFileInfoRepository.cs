using Files_cloud_manager.Models;
using FileInfo = Files_cloud_manager.Models.FileInfo;

namespace Files_cloud_manager.Server.Domain.Interfaces
{
    public interface IFileInfoRepository : IBaseRepository<FileInfo>
    {
    }
}
