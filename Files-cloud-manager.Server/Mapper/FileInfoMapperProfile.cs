using AutoMapper;
using Files_cloud_manager.Server.Models.DTO;
using Files_cloud_manager.Server.Models;
using FileInfo = Files_cloud_manager.Server.Models.FileInfo;

namespace Files_cloud_manager.Server.Mapper
{
    public class FileInfoMapperProfile : Profile
    {
        public FileInfoMapperProfile()
        {
            CreateMap<FileInfo, FileInfoDTO>();
            CreateMap<FileInfo, FileInfoDTO>().ReverseMap();
        }
    }
}
