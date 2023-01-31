using Files_cloud_manager.Server.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Files_cloud_manager.Server.Domain
{
    public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        public AppDBContext DBContext { get; set; }

        public DbSet<T> DBset { get; set; }

        public BaseRepository(AppDBContext dbContext)
        {
            DBContext = dbContext;
            DBset = DBContext.Set<T>();
        }
        public virtual IEnumerable<T> Get(Expression<Func<T, bool>> filter = null, string[] includeProperties = null)
        {
            IQueryable<T> query = DBset;

            if (filter is not null)
            {
                query = query.Where(filter);
            }

            foreach (var item in includeProperties)
            {
                query = query.Include(item);
            }

            return query.ToList();
        }

        public void Create(T item)
        {
            DBset.Add(item);
        }
        public void Delete(T item)
        {
            DBset.Remove(item);
        }
        public void Update(T item)
        {
            DBset.Update(item);
        }
    }
}
