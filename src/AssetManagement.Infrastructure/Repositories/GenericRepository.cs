using AssetManagement.Domain.Interfaces;
using AssetManagement.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using AssetManagement.Domain.Constants;

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
            //_context.Set<T>().Remove(entity);
            PropertyInfo propertyInfo = entity.GetType().GetProperty("IsDeleted");
            propertyInfo.SetValue(entity, true);
            _context.Set<T>().Update(entity);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<IEnumerable<T>> Get(int page = 1, Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, string includeProperties = "")
        {
            IQueryable<T> query = _context.Set<T>();
            //if (filter != null)
            //{
            //    query = query.Where(filter);
            //}

            //var total = query.ToList();
            //query = query.Skip((page - 1) * 15).Take(15);
            //foreach (var includeProperty in includeProperties.Split
            //             (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            //{
            //    query = query.Include(includeProperty);
            //}

            //if (orderBy != null)
            //{
            //    return orderBy(query).ToList();
            //}

            return await GetAllAsync();
        }

        public async Task<(IEnumerable<T> items,int totalCount)> GetAllAsync(int page = 1, Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, string includeProperties = "")
        {
            IQueryable<T> query = _context.Set<T>();
            if (filter != null)
            {
                query = query.Where(filter);
            }

            var totalCount = query.Count();
            query = query.Skip((page - 1) * PageSizeConstant.PAGE_SIZE).Take(PageSizeConstant.PAGE_SIZE);
            foreach (var includeProperty in includeProperties.Split
                         (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return (await orderBy(query).ToListAsync(),totalCount);
            }

            return (await query.ToListAsync(),totalCount);
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