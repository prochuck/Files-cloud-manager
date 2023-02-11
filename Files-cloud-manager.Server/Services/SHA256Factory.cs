using Files_cloud_manager.Server.Services.Interfaces;
using System.Security.Cryptography;

namespace Files_cloud_manager.Server.Services
{
    public class SHA256Factory : IHashAlgorithmFactory
    {
        public HashAlgorithm Create()
        {
            return SHA256.Create();
        }
    }
}
