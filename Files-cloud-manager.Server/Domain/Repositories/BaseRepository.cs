using Files_cloud_manager.Server.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Files_cloud_manager.Server.Domain
{
    public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        public AppDBContext DBContext { get; set; }

        public DbSet<T> DBset { get; set; }

        private ReaderWriterLockSlim ReaderWriterLockSlim = new ReaderWriterLockSlim();

        public BaseRepository(AppDBContext dbContext)
        {
            DBContext = dbContext;
            DBset = DBContext.Set<T>();
        }

        public virtual IEnumerable<T> Get(Expression<Func<T, bool>> filter = null, string[] includeProperties = null)
        {
            ReaderWriterLockSlim.EnterReadLock();
            try
            {
                IQueryable<T> query = DBset;

                if (filter is not null)
                {
                    query = query.Where(filter);
                }
                if (includeProperties is not null)
                {
                    foreach (var item in includeProperties)
                    {
                        query = query.Include(item);
                    }
                }

                return query.ToList();
            }
            finally
            {
                ReaderWriterLockSlim.ExitReadLock();
            }
        }

        public void Create(T item)
        {
            ReaderWriterLockSlim.EnterWriteLock();
            try
            {
                DBset.Add(item);
            }
            finally
            {
                ReaderWriterLockSlim.ExitWriteLock();
            }
        }
        public void Delete(T item)
        {
            ReaderWriterLockSlim.EnterWriteLock();
            try
            {
                DBset.Remove(item);
            }
            finally
            {
                ReaderWriterLockSlim.ExitWriteLock();
            }
        }
        public void Update(T item)
        {
            ReaderWriterLockSlim.EnterWriteLock();
            try
            {
                DBset.Update(item);
            }
            finally
            {
                ReaderWriterLockSlim.ExitWriteLock();
            }
        }
    }
}
