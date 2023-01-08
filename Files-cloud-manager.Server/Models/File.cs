using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Files_cloud_manager.Server.Models
{
    public class File
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string RelativePath { get; set; }

        [Required]
        public byte[] Hash { get; set; }

        [Required]
        [ForeignKey(nameof(Folder))]
        public int FolderId { get; set; }
        public Folder Folder { get; set; }

    }
}

