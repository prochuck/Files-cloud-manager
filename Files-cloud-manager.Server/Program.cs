using Files_cloud_manager.Server.Domain;
using Files_cloud_manager.Server.Domain.Interfaces;
using Files_cloud_manager.Models;
using Files_cloud_manager.Server.Services;
using Files_cloud_manager.Server.Services.Interfaces;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using AutoMapper;
using Files_cloud_manager.Server.Mapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
    
// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDBContext>(e => e.UseSqlServer(@"Data Source=(local);Database=HyperVWebController2;Trusted_Connection=True;TrustServerCertificate=True;"));

builder.Services.AddTransient<IRoleRepository, RoleRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IFileInfoRepository, FileInfoRepository>();
builder.Services.AddTransient<IFileInfoGroupRepostiory, FileInfoGroupRepository>();
builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
builder.Services.AddTransient<HashAlgorithm, SHA512>(e => SHA512.Create());
builder.Services.AddScoped<IFilesSynchronizationService, FilesSynchronizationService>();
builder.Services.AddAutoMapper(typeof(FileInfoMapperProfile), typeof(FileInfoGroupMapperProfile));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o =>
{
    o.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    o.SlidingExpiration = true;
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
