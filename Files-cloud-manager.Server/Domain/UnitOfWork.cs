using Files_cloud_manager.Server.Domain.Interfaces;

namespace Files_cloud_manager.Server.Domain
{
    public class UnitOfWork : IUnitOfWork
    {
        private AppDBContext _context;
        public IUserRepository UserRepository { get; }
        public IRoleRepository RoleRepository { get; }
        public IFileInfoRepository FileInfoRepository { get; }
        public IFileInfoGroupRepostiory FileInfoGroupRepostiory { get; }

        public UnitOfWork(AppDBContext context,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IFileInfoRepository fileInfoRepository,
            IFileInfoGroupRepostiory fileInfoGroupRepostiory)
        {
            _context = context;
            UserRepository = userRepository;
            RoleRepository = roleRepository;
            FileInfoRepository = fileInfoRepository;
            FileInfoGroupRepostiory = fileInfoGroupRepostiory;
        }


        public void Save()
        {
            _context.SaveChanges();
        }

    }
}
