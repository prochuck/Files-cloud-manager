using Files_cloud_manager.Models;
using System.Linq.Expressions;

namespace Files_cloud_manager.Server.Domain.Interfaces
{
    public interface IBaseRepository<T>
    {
        /// <summary>
        /// Получить элементы из базы данных.
        /// </summary>
        /// <param name="filter">Фильтр для получаемых значений</param>
        /// <param name="includeProperties">Значения для быстрой загрузки</param>
        /// <returns></returns>
        IEnumerable<T> Get(Expression<Func<T, bool>> filter = null, string[] includeProperties = null);
        void Create(T item);
        void Update(T item);
        void Delete(T item);
    }
}
