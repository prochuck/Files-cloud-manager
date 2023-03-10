using Files_cloud_manager.Server.Models;
using Files_cloud_manager.Server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using FileInfo = Files_cloud_manager.Server.Models.FileInfo;

namespace Files_cloud_manager.Server.Domain
{
    public class AppDBContext : DbContext
    {
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<FileInfo> FileInfos { get; set; }
        public DbSet<FileInfoGroup> FilesInfoGroups { get; set; }
        public enum RolesEnum
        {
            Admin,
            User
        }

        public AppDBContext(DbContextOptions options, IHashAlgorithmFactory hashFactory) : base(options)
        {
            if (Database.EnsureCreated())
            {
                foreach (var item in Enum.GetValues(typeof(RolesEnum)))
                {
                    Roles.Add(new Role() { RoleName = item.ToString() });
                }

                Users.Add(new User() { RoleId = 1, Login = "admin", PasswordHash = hashFactory.Create().ComputeHash("123".Select(e => (byte)e).ToArray()), UserFoldersPath = "admin" });
                this.SaveChanges();
            }
            this.SaveChanges();

        }
    }
}
