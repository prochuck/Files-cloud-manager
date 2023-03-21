using FileCloudAPINameSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Files_cloud_manager.Client.Models
{
    public class FileDifferenceModel
    {
        public FileInfoDTO File { get; set; }
        public FileState State { get; set; }

    }
    public enum FileState
    {
        /// <summary>
        /// Существует только на клиенте.
        /// </summary>
        ClientOnly,
        /// <summary>
        /// Существует и на клиенте и на сервере, но файлы разные.
        /// </summary>
        Modified,
        /// <summary>
        /// Существует только на сервере.
        /// </summary>
        ServerOnly
    }
}
