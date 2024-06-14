using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Interfaces;
using AssetManagement.Infrastructure.DataAccess;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AssetManagement.Domain.Constants;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;

namespace AssetManagement.Infrastructure.Repositories
{
    public class AssetRepository : GenericRepository<Asset>, IAssetRepository
    {
        private readonly DBContext _context;
        public AssetRepository(DBContext context) : base(context)
        {
            _context = context;
        }
        public async Task<(IEnumerable<Asset> items, int totalCount)> GetAllAsync(int page = 1, Expression<Func<Asset, bool>>? filter = null,
    Func<IQueryable<Asset>, IOrderedQueryable<Asset>>? orderBy = null, string includeProperties = "")
        {
            IQueryable<Asset> query = _context.Set<Asset>();
            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                         (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            var totalCount = query.Count();

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            query = query.Skip((page - 1) * PageSizeConstant.PAGE_SIZE).Take(PageSizeConstant.PAGE_SIZE);

            var items = await query.ToListAsync();
            return (items, totalCount);
        }

        public async Task<Asset?> GetAssetDetail(Guid id)
        {
            return await _context.Assets.Include(x => x.Assignments).ThenInclude(x => x.UserBy).Include(x => x.Assignments)
                .ThenInclude(x => x.UserTo).FirstOrDefaultAsync(x=>x.Id == id);
        }
    }
}