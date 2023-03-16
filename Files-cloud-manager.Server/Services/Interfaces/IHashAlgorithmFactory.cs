using System.Security.Cryptography;

namespace Files_cloud_manager.Server.Services.Interfaces
{
    /// <summary>
    /// Абстрактная фабрика хэш алгоритмов
    /// </summary>
    public interface IHashAlgorithmFactory
    {
        HashAlgorithm Create();
    }
}