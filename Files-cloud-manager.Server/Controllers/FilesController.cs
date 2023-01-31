using Files_cloud_manager.Models.DTO;
using Files_cloud_manager.Server.Domain.Interfaces;
using Files_cloud_manager.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using FileInfo = Files_cloud_manager.Server.Models.FileInfo;

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
        [HttpGet]
        [ProducesResponseType(typeof(IList<FileInfoGroupDTO>), 201)]
        public IActionResult GetFoldersList()
        {
            return Ok();
        }
        /// <summary>
        /// Получение списка файлов и их хэшей
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(IList<FileInfoDTO>), 201)]
        [ProducesResponseType(typeof(IList<FileInfoDTO>), 400)]
        public IActionResult GetFolderContent()
        {
            return Ok();
        }

        /// <summary>
        /// Создание папки.
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>\
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public IActionResult CreateFolder(string folderName)
        {
            return Ok();
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 10L * 1024L * 1024L * 1024L)]
        [RequestSizeLimit(10L * 1024L * 1024L * 1024L)]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public IActionResult AddFileToFolder(string fileGroupName, string filePath, IFormFile uploadedFile)
        {
            return Ok();
        }
    }
}
