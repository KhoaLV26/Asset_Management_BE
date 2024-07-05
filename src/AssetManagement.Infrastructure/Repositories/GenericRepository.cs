using AssetManagement.Domain.Constants;
using AssetManagement.Domain.Interfaces;
using AssetManagement.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace AssetManagement.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly DBContext _context;

        public GenericRepository(DBContext context)
        {
            _context = context;
        }

        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }

        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        public void SoftDelete(T entity)
        {
            //_context.Set<T>().Remove(entity);
            PropertyInfo propertyInfo = entity.GetType().GetProperty("IsDeleted");
            propertyInfo.SetValue(entity, true);
            _context.Set<T>().Update(entity);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _context.Set<T>();
            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }
            }
            if (expression != null)
            {
                query = query.Where(expression);
            }
            return await query.ToListAsync();
        }

        public async Task<(IEnumerable<T> items, int totalCount)> GetAllAsync(int page = 1, Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, string includeProperties = "", Expression<Func<T, bool>>? prioritizeCondition = null, int pageSize = 10)
        {
            IQueryable<T> query = _context.Set<T>();

            foreach (var includeProperty in includeProperties.Split
                                     (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            List<T> prioritizedItems = new List<T>();
            List<T> nonPrioritizedItems = new List<T>();

            if (prioritizeCondition != null)
            {
                var notDeletedExpression = Expression.Lambda<Func<T, bool>>(
                    Expression.Not(Expression.Property(prioritizeCondition.Parameters[0], "IsDeleted")), 
                    prioritizeCondition.Parameters);
                
                var prioritizedQuery = query.Where(prioritizeCondition).Where(notDeletedExpression);
                var nonPrioritizedQuery = query.Where(Expression.Lambda<Func<T, bool>>(
                    Expression.Not(prioritizeCondition.Body), prioritizeCondition.Parameters));

                if (filter != null)
                {
                    nonPrioritizedQuery = nonPrioritizedQuery.Where(filter);
                }

                if (orderBy != null)
                {
                    nonPrioritizedQuery = orderBy(nonPrioritizedQuery);
                }

                prioritizedItems = await prioritizedQuery.ToListAsync();

                nonPrioritizedItems = await nonPrioritizedQuery.ToListAsync();
            }
            else
            {
                if (filter != null)
                {
                    query = query.Where(filter);
                }

                if (orderBy != null)
                {
                    query = orderBy(query);
                }

                nonPrioritizedItems = await query.ToListAsync();
            }

            var items = prioritizedItems.Concat(nonPrioritizedItems).ToList();
            var totalCount = items.Count();

            var paginatedItems = items.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return (paginatedItems, totalCount);
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _context.Set<T>();
            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }
            }
            if (expression != null)
            {
                query = query.Where(expression);
            }
            return await query.FirstOrDefaultAsync();
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>> expression)
        {
            IQueryable<T> query = _context.Set<T>();
            if (expression != null)
            {
                query = query.Where(expression);
            }
            return await query.FirstOrDefaultAsync();
        }

        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _context.Set<T>().RemoveRange(entities);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> expression = null)
        {
            IQueryable<T> query = _context.Set<T>();
            if (expression != null)
            {
                query = query.Where(expression);
            }
            return await query.CountAsync();
        }
    }
}
