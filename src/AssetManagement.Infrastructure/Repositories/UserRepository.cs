using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Interfaces;
using AssetManagement.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Infrastructure.Repositories
{
    internal class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly DBContext _context;

        public UserRepository(DBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<User> Items, int TotalCount)> GetFilteredAsync(
            string location,
            Expression<Func<User, bool>> filter = null,
            Func<IQueryable<User>, IOrderedQueryable<User>> orderBy = null,
            string includeProperties = "",
            string? searchTerm = "",
            int pageNumber = 1,
            int pageSize = 15)
        {
            IQueryable<User> query = _context.Users;

            query = query.Where(u => !u.IsDeleted);

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(u => u.LocationId.ToString() == location);
            }

            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(e => EF.Functions.Like(EF.Property<string>(e, "FirstName") + " " + EF.Property<string>(e, "LastName"), $"%{searchTerm}%")
                                          || EF.Functions.Like(EF.Property<string>(e, "StaffCode"), $"%{searchTerm}%") 
                                          || EF.Functions.Like(EF.Property<string>(e, "Username"), $"%{searchTerm}%"));
            }

            // Calculate the total count before applying pagination
            var totalCount = await query.CountAsync();

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            // Apply pagination first
            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            // Apply Include after pagination
            query = query.Include(u => u.Location).Include(u => u.Role);

            // Execute the query and get the paginated results
            var items = await query.ToListAsync();

            return (items, totalCount);
        }

    }
}
