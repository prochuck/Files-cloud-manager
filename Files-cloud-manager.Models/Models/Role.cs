using System.ComponentModel.DataAnnotations;

namespace Files_cloud_manager.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string RoleName { get; set; }
    }
}
