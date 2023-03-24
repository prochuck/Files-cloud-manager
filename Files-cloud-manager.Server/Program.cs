using Files_cloud_manager.Server.Domain;
using Files_cloud_manager.Server.Domain.Interfaces;
using Files_cloud_manager.Server.Models;
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
using Files_cloud_manager.Server.Configs;
using Microsoft.Extensions.Configuration;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System;
using System.Net;

var builder = WebApplication.CreateBuilder(args);



builder.WebHost.ConfigureKestrel(
    e => e.Listen(IPAddress.Any, builder.Configuration.GetValue<int>("ListenPort"), e =>
        e.UseHttps(builder.Configuration.GetValue<string>("CertPath"),
        builder.Configuration.GetValue<string>("CertPassword"))
    ));
// todo Добавить identeti?
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDBContext>(e => e.UseSqlServer(builder.Configuration.GetConnectionString("Files-cloud-manager-db")));

builder.Services.AddTransient<IRoleRepository, RoleRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IFileInfoRepository, FileInfoRepository>();
builder.Services.AddTransient<IFileInfoGroupRepostiory, FileInfoGroupRepository>();
builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();

builder.Services.Configure<FilesSyncServiceConfig>(builder.Configuration.GetSection(nameof(FilesSyncServiceConfig)));

builder.Services.AddSingleton<IHashAlgorithmFactory, SHA256Factory>();
builder.Services.AddScoped<IFilesSynchronizationService, FilesSynchronizationService>();
builder.Services.AddSingleton<ISynchronizationContainerService, SynchronizationContainerService>();

builder.Services.AddAutoMapper(typeof(FileInfoMapperProfile), typeof(FileInfoGroupMapperProfile));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o =>
{
    o.ExpireTimeSpan = TimeSpan.FromHours(1);
    o.Cookie.Name = "FileCloudCoockie";
    o.Cookie.MaxAge = o.ExpireTimeSpan;
    o.SlidingExpiration = true;
    o.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
    o.Events.OnRedirectToLogin = (context) =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.CustomOperationIds(apiDesc =>
    {
        return apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null;
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(o =>
    {
        o.DisplayOperationId();
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();