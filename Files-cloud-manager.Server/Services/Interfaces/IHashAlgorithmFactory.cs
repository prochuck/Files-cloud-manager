using System.Security.Cryptography;

namespace Files_cloud_manager.Server.Services.Interfaces
{
    public interface IHashAlgorithmFactory
    {
        HashAlgorithm Create();
    }
}