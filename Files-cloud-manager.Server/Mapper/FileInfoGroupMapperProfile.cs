using AutoMapper;
using Files_cloud_manager.Server.Models;
using Files_cloud_manager.Server.Models.DTO;

namespace Files_cloud_manager.Server.Mapper
{
    public class FileInfoGroupMapperProfile: Profile
    {
        public FileInfoGroupMapperProfile()
        {
            CreateMap<FileInfoGroup, FileInfoGroupDTO>();
            CreateMap<FileInfoGroup, FileInfoGroupDTO>().ReverseMap();
        }
    }
}
