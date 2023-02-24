using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Files_cloud_manager.Models.DTO
{
    public class FileInfoDTO
    {
        public int id { get; set; }
        public string RelativePath { get; set; }
        public byte[] Hash { get; set; }
    }
}
