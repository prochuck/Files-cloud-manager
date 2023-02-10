using AutoMapper;
using Files_cloud_manager.Models.DTO;
using Files_cloud_manager.Models;
using FileInfo = Files_cloud_manager.Models.FileInfo;

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
