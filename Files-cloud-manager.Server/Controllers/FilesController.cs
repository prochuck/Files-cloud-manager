using AutoMapper;
using Files_cloud_manager.Models;
using Files_cloud_manager.Models.DTO;
using Files_cloud_manager.Server.Domain.Interfaces;
using Files_cloud_manager.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Files_cloud_manager.Server.Controllers
{
    /// <summary>
    /// Контролер для файлов
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/Files/[action]")]
    public class FilesController : Controller
    {
        private IMapper _mapper;
        private IUnitOfWork _unitOfWork;
        private SynchronizationContainerService _syncContainer;

        private int _userId;

        public FilesController(IMapper mapper, IUnitOfWork unitOfWork, SynchronizationContainerService syncContainer)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _syncContainer = syncContainer;
            _userId = int.Parse(User.Claims.First(e => e.Type == ClaimTypes.NameIdentifier).Value);
        }



        /// <summary>
        /// Начало синхронизации файлов.
        /// </summary>
        /// <param name="fileInfoGroupName"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(int), 201)]
        [ProducesResponseType(400)]
        public IActionResult StartSynchronization(string fileInfoGroupName)
        {
            int syncId = _syncContainer.StartNewSynchronization(_userId, fileInfoGroupName);
            if (syncId != -1)
            {
                return Ok(syncId);
            }
            return BadRequest();

        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public IActionResult EndSynchronization(int syncId)
        {
            if (_syncContainer.EndSynchronization(_userId, syncId))
            {
                return Ok();
            }
            return BadRequest();
        }

        [HttpGet]
        [ProducesResponseType(typeof(IList<FileInfoGroupDTO>), 201)]
        [ProducesResponseType(400)]
        public IActionResult GetFoldersList()
        {
            return Ok(_unitOfWork.FileInfoGroupRepostiory.Get(e => e.OwnerId == _userId)
                .Select(e => _mapper.Map<FileInfoGroup, FileInfoGroupDTO>(e)).ToList());
        }
        /// <summary>
        /// Получение списка файлов и их хэшей
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(IList<FileInfoDTO>), 201)]
        [ProducesResponseType(400)]
        public IActionResult GetFolderContent(int syncId)
        {
            List<FileInfoDTO> files = _syncContainer.GetFilesInfos(_userId, syncId);
            if (files is not null)
            {
                return Ok(files);
            }
            return BadRequest();
        }

        /// <summary>
        /// Создание папки.
        /// </summary>
        /// <param name="fileInfoGroupName"></param>
        /// <returns></returns>\
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public IActionResult CreateFileInfoGroup(string fileInfoGroupName)
        {
            int userId = int.Parse(User.Claims.First(e => e.Type == ClaimTypes.NameIdentifier).Value);
            if (_unitOfWork.FileInfoGroupRepostiory.Get(e => e.OwnerId == userId && e.Name == fileInfoGroupName).Any())
            {
                return BadRequest();

            }
            FileInfoGroup newGroup = new FileInfoGroup()
            {
                Name = fileInfoGroupName,
                OwnerId = userId
            };
            _unitOfWork.FileInfoGroupRepostiory.Create(newGroup);
            _unitOfWork.Save();
            return Ok();
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 10L * 1024L * 1024L * 1024L)]
        [RequestSizeLimit(10L * 1024L * 1024L * 1024L)]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public IActionResult CreateOrUpdateFileInFileInfoGroup(int syncId, string filePath, IFormFile uploadedFile)
        {
            using (Stream readStream = uploadedFile.OpenReadStream())
            {
                if (_syncContainer.CreateOrUpdateFileInFileInfoGroup(_userId, syncId, filePath, readStream))
                {
                    return Ok();
                }
            }
            return BadRequest();
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public IActionResult DeleteFileInFileInfoGroup(int syncId, string filePath)
        {
            if (_syncContainer.DeleteFileInFileInfoGroup(_userId, syncId, filePath))
            {
                return Ok();
            }
            return BadRequest();
        }

        [HttpGet]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public IActionResult GetFile(int syncId, string filePath)
        {
            Stream stream = _syncContainer.GetFile(_userId, syncId, filePath);
            if (stream is not null)
            {
                return File(stream, "application/octet-stream", Path.GetFileName(filePath));
            }
            return BadRequest();

        }
    }
}
