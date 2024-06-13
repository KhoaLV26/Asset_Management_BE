using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AssetManagement.Domain.Entities;

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