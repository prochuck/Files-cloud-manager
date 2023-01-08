using Files_cloud_manager.Server.Services.Interfaces;

namespace Files_cloud_manager.Server.Services
{
    public class HashService : IHashService
    {
        public string GetHash(string s)
        {
            return s;
        }
    }
}
