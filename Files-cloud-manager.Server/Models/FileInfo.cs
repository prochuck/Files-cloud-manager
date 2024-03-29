﻿using Files_cloud_manager.Server.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Files_cloud_manager.Server.Models
{
    public class FileInfo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string RelativePath { get; set; }

        [Required]
        public byte[] Hash { get; set; }

        [Required]
        [ForeignKey(nameof(FileInfoGroup))]
        public int FileInfoGroupId { get; set; }
        public FileInfoGroup FileInfoGroup { get; set; }

    }
}

