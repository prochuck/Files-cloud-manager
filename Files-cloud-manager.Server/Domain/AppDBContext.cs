using Files_cloud_manager.Server.Models;
using Files_cloud_manager.Server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using File = Files_cloud_manager.Server.Models.File;

namespace Files_cloud_manager.Server.Domain
{
    public class AppDBContext : DbContext
    {
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<Folder> Folders { get; set; }


        public AppDBContext(DbContextOptions options,IHashService hashService) : base(options)
        {
            if (Database.EnsureCreated())
            {
                Roles.Add(new Role() { RoleName = "Admin" });
                Roles.Add(new Role() { RoleName = "User" });
                Users.Add(new User() { RoleId = 1, Login = "admin", PasswordHash = hashService.GetHash("123"), UserFoldersPath = "123" });
                this.SaveChanges();
            }
            this.SaveChanges();

        }
    }
}
