using Files_cloud_manager.Server.Models;
using File = Files_cloud_manager.Server.Models.File;

namespace Files_cloud_manager.Server.Domain.Interfaces
{
    public interface IFileRepository : IBaseRepository<File>
    {
    }
}
