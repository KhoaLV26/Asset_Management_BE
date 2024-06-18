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

        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includeProperties);

        Task<(IEnumerable<T> items, int totalCount)> GetAllAsync(int page = 1, Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, string includeProperties = "");

        Task<T> GetAsync(Expression<Func<T, bool>> expression);

        Task<T> GetAsync(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includeProperties);

        Task AddAsync(T entity);

        void Update(T entity);

        void Delete(T entity);

        void SoftDelete(T entity);

        void RemoveRange(IEnumerable<T> entities);

        Task<int> CountAsync(Expression<Func<T, bool>> expression = null);
    }
}