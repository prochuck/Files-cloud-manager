using Files_cloud_manager.Server.Domain.Interfaces;

namespace Files_cloud_manager.Server.Domain
{
    public class UnitOfWork : IUnitOfWork
    {
        private AppDBContext _context;
        public IUserRepository UserRepository { get; }
        public IRoleRepository RoleRepository { get; }
        public IFileRepository FileRepository { get; }
        public IFolderRepostiory FolderRepostiory { get; }

        public UnitOfWork(AppDBContext context,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IFileRepository fileRepository,
            IFolderRepostiory folderRepostiory)
        {
            _context = context;
            UserRepository = userRepository;
            RoleRepository = roleRepository;
            FileRepository = fileRepository;
            FolderRepostiory = folderRepostiory;
        }


        public void Save()
        {
            _context.SaveChanges();
        }

    }
}
