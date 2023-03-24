using Files_cloud_manager.Client.Configs;
using Files_cloud_manager.Client.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Files_cloud_manager.Client.Extentions
{
    internal static class ConfigurationExtentions
    {
        public static T GetConfig<T>(this IConfiguration config)
        {
            IConfigurationSection section = config.GetSection(typeof(T).Name)
                    ?? throw new Exception($"В конфигурации отсутсвует секция{typeof(T).Name}");

            return section.Get<T>(e =>
            {
                e.ErrorOnUnknownConfiguration = true;
                e.BindNonPublicProperties = true;
            }) ?? throw new Exception($"Конфигурация не верна для {typeof(T).Name}");
        }

        public static ProgramListCaretakerConfig GetProgramListCaretakerConfig(this IConfiguration config)
        {
            return config.GetConfig<ProgramListCaretakerConfig>();
        }
        public static ServerConnectionConfig GetServerConnectionConfig(this IConfiguration config)
        {
            return config.GetConfig<ServerConnectionConfig>();
        }
    }
}
