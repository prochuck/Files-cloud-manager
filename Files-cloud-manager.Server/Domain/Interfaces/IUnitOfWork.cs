namespace Files_cloud_manager.Server.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        IFileInfoRepository FileInfoRepository { get; }
        IFileInfoGroupRepostiory FileInfoGroupRepostiory { get; }
        IRoleRepository RoleRepository { get; }
        IUserRepository UserRepository { get; }
        public IBaseRepository<T> GetGenericRepository<T>();
        void Save();
    }
}