﻿using AutoMapper;
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
        [HttpGet(Name = "getFolderContent")]
        [ProducesResponseType(typeof(IList<FileInfoDTO>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetFolderContentAsync(int syncId)
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

        [HttpPost(Name = "createOrUpdateFileInFileInfoGroup")]
        [RequestFormLimits(MultipartBodyLengthLimit = 10L * 1024L * 1024L * 1024L)]
        [RequestSizeLimit(10L * 1024L * 1024L * 1024L)]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateOrUpdateFileInFileInfoGroupAsync(int syncId, string filePath, IFormFile uploadedFile)
        {
            using (Stream readStream = uploadedFile.OpenReadStream())
            {
                if (await _syncContainer.CreateOrUpdateFileInFileInfoGroupAsync(GetUserId(), syncId, filePath, readStream))
                {
                    return Ok();
                }
            }
            return BadRequest();
        }

        [HttpPost(Name = "deleteFileInFileInfoGroup")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> DeleteFileInFileInfoGroupAsync(int syncId, string filePath)
        {
            if (await _syncContainer.DeleteFileInFileInfoGroupAsync(GetUserId(), syncId, filePath))
            {
                return Ok();
            }
            return BadRequest();
        }

        [HttpGet(Name = "getFile")]
        [ProducesResponseType(200)]
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
