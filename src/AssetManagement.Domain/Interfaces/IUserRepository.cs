using AssetManagement.Domain.Entities;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace AssetManagement.Domain.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<(IEnumerable<User> Items, int TotalCount)> GetFilteredAsync(
            string location,
            Expression<Func<User, bool>> filter = null,
            Func<IQueryable<User>, IOrderedQueryable<User>> orderBy = null,
            string includeProperties = "",
            string? searchTerm = "",
            int pageNumber = 1,
            int pageSize = 15);
    }
}