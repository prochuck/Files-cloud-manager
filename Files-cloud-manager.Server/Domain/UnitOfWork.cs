using Files_cloud_manager.Server.Domain.Interfaces;
using Files_cloud_manager.Server.Models;
using System.Collections.ObjectModel;
using FileInfo = Files_cloud_manager.Server.Models.FileInfo;

namespace Files_cloud_manager.Server.Domain
{
    public class UnitOfWork : IUnitOfWork
    {
        private AppDBContext _context;
        public IUserRepository UserRepository { get; }
        public IRoleRepository RoleRepository { get; }
        public IFileInfoRepository FileInfoRepository { get; }
        public IFileInfoGroupRepostiory FileInfoGroupRepostiory { get; }

        private Dictionary<Type, object> _repositories;
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

            _repositories = new Dictionary<Type, object>()
            {
               {typeof(User),userRepository},
               {typeof(Role),roleRepository},
               {typeof(FileInfo), fileInfoRepository},
               {typeof(FileInfoGroup), fileInfoGroupRepostiory},
            };
        }

        public IBaseRepository<T> GetGenericRepository<T>()
        {
            if (_repositories.ContainsKey(typeof(T)))
            {
                return _repositories[typeof(T)] as IBaseRepository<T>;
            }
            return null;
        }

        public void Save()
        {
            _context.SaveChanges();
        }

    }
}
