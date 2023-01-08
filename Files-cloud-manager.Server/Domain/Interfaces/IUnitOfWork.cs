namespace Files_cloud_manager.Server.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        IFileRepository FileRepository { get; }
        IFolderRepostiory FolderRepostiory { get; }
        IRoleRepository RoleRepository { get; }
        IUserRepository UserRepository { get; }

        void Save();
    }
}