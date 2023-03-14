using AutoMapper;
using Files_cloud_manager.Server.Models;
using Files_cloud_manager.Server.Models.DTO;
using Files_cloud_manager.Server.Domain.Interfaces;
using Files_cloud_manager.Server.Services;
using Files_cloud_manager.Server.Services.Interfaces;
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
        private ISynchronizationContainerService _syncContainer;


        public FilesController(IMapper mapper, IUnitOfWork unitOfWork, ISynchronizationContainerService syncContainer)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _syncContainer = syncContainer;
        }



        /// <summary>
        /// Начало синхронизации файлов.
        /// </summary>
        /// <param name="fileInfoGroupName"></param>
        /// <returns></returns>
        [HttpPost(Name = "startSynchronization")]
        [ProducesResponseType(typeof(int), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> StartSynchronizationAsync(string fileInfoGroupName)
        {
            int syncId = await _syncContainer.StartNewSynchronizationAsync(GetUserId(), fileInfoGroupName);
            if (syncId != -1)
            {
                return Ok(syncId);
            }
            return BadRequest();

        }

        [HttpPost(Name = "endSynchronization")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> EndSynchronizationAsync(int syncId)
        {
            if (await _syncContainer.EndSynchronizationAsync(GetUserId(), syncId))
            {
                return Ok();
            }
            return BadRequest();
        }

        [HttpPost(Name = "rollBackSynchronization")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RollBackSynchronizationAsync(int syncId)
        {
            if (await _syncContainer.RollBackSynchronizationAsync(GetUserId(), syncId))
            {
                return Ok();
            }
            return BadRequest();
        }
        [HttpPost(Name = "rollBackSynchronizationByName")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RollBackSynchronizationByNameAsync(string fileGroupName)
        {
            if (await _syncContainer.RollBackSynchronizationAsync(GetUserId(), fileGroupName))
            {
                return Ok();
            }
            return BadRequest();
        }
        [HttpGet(Name = "getFoldersList")]
        [ProducesResponseType(typeof(IList<FileInfoGroupDTO>), 200)]
        [ProducesResponseType(400)]
        public IActionResult GetFoldersListAsync()
        {
            return Ok(_unitOfWork.FileInfoGroupRepostiory.Get(e => e.OwnerId == GetUserId())
                .Select(e => _mapper.Map<FileInfoGroup, FileInfoGroupDTO>(e)).ToList());
        }
        /// <summary>
        /// Получение списка файлов и их хэшей
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "getFileInfoGroupContentAsync")]
        [ProducesResponseType(typeof(IList<FileInfoDTO>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetFileInfoGroupContentAsync(int syncId)
        {
            List<FileInfoDTO> files = await _syncContainer.GetFilesInfosAsync(GetUserId(), syncId);
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
        [HttpPost(Name = "createFileInfoGroup")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public IActionResult CreateFileInfoGroup(string fileInfoGroupName)
        {

            if (fileInfoGroupName.Any(e => !char.IsLetterOrDigit(e)))
            {
                return BadRequest();
            }
            if (_unitOfWork.FileInfoGroupRepostiory.Get(e => e.OwnerId == GetUserId() && e.Name == fileInfoGroupName).Any())
            {
                return BadRequest();

            }
            FileInfoGroup newGroup = new FileInfoGroup()
            {
                Name = fileInfoGroupName,
                OwnerId = GetUserId()
            };
            _unitOfWork.FileInfoGroupRepostiory.Create(newGroup);
            _unitOfWork.Save();
            return Ok();
        }

        //transfer-encoding chunked asp.net
        [HttpPost(Name = "createOrUpdateFile")]
        [RequestFormLimits(MultipartBodyLengthLimit = 10L * 1024L * 1024L * 1024L)]
        [RequestSizeLimit(10L * 1024L * 1024L * 1024L)]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateOrUpdateFileAsync(int SyncId, string FilePath, [FromForm] FileUploadModel fileUploadModel)
        {
            using (Stream readStream = fileUploadModel.UploadedFile.OpenReadStream())
            {
                if (await _syncContainer.CreateOrUpdateFileAsync(GetUserId(), SyncId, FilePath, readStream))
                {
                    return Ok();
                }
            }
            return BadRequest();
        }

        [HttpPost(Name = "deleteFile")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> DeleteFileAsync(int syncId, string filePath)
        {
            if (await _syncContainer.DeleteFileAsync(GetUserId(), syncId, filePath))
            {
                return Ok();
            }
            return BadRequest();
        }


        [HttpGet(Name = "getFile")]
        [Produces("application/octet-stream")]
        [ProducesResponseType(typeof(FileStreamResult), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetFileAsync(int syncId, string filePath)
        {
            Stream stream = await _syncContainer.GetFileAsync(GetUserId(), syncId, filePath);
            if (stream is not null)
            {
                return File(stream, "application/octet-stream", Path.GetFileName(filePath));
            }
            return BadRequest();
        }



        [NonAction]
        private int GetUserId()
        {
            return int.Parse(User.Claims.First(e => e.Type == ClaimTypes.NameIdentifier).Value);
        }
    }
}
