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

        private IHashAlgorithmFactory _hashAlgorithmFactory { get; set; }

        public AppDBContext(DbContextOptions options, IHashAlgorithmFactory hashFactory) : base(options)
        {
            _hashAlgorithmFactory = hashFactory;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Role>().HasData(
                new Role() { Id = 1, RoleName = "Admin" },
                new Role() { Id = 2, RoleName = "User" });
            modelBuilder.Entity<User>().HasData(
                new User()
                {
                    Id = 1, 
                    UserFoldersPath="admin",
                    Login = "admin",
                    PasswordHash = _hashAlgorithmFactory.Create().ComputeHash("123".Select(e => (byte)e).ToArray()),
                    RoleId = 1,
                });
        }
    }
}
