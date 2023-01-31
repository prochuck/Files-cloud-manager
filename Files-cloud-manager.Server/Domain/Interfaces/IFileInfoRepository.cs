using Files_cloud_manager.Server.Models;
using FileInfo = Files_cloud_manager.Server.Models.FileInfo;

namespace Files_cloud_manager.Server.Domain.Interfaces
{
    public interface IFileInfoRepository : IBaseRepository<FileInfo>
    {
    }
}
