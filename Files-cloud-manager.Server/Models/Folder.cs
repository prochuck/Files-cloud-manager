using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Files_cloud_manager.Server.Models
{
    public class Folder
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public List<File> Files { get; set; }

        [Required]
        [ForeignKey(nameof(Owner))]
        public int OwnerId { get; set; }
        public User Owner { get; set; }

    }
}
