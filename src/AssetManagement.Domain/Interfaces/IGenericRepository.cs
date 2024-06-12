using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AssetManagement.Domain.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();

        Task<IEnumerable<T>> Get(int pageSize = 1, Expression<Func<T, bool>>? filter = null,
              Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
              string includeProperties = "");

        Task<T> GetAsync(Expression<Func<T, bool>> expression);

        Task<T> GetAsync(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includeProperties);

        Task AddAsync(T entity);

        void Update(T entity);

        void Delete(T entity);

        void RemoveRange(IEnumerable<T> entities);
    }
}