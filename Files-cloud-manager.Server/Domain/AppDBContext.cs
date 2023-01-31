﻿using Files_cloud_manager.Server.Models;
using Files_cloud_manager.Server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using FileInfo = Files_cloud_manager.Server.Models.FileInfo;

namespace Files_cloud_manager.Server.Domain
{
    public class AppDBContext : DbContext
    {
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<FileInfo> FileInfos { get; set; }
        public DbSet<FileInfoGroup> FilesInfoGroups { get; set; }


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
