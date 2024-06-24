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

        public async Task<Asset?> GetAssetDetail(Guid id)
        {
            return await _context.Assets
                .Include(x => x.Category)
                .Include(x => x.Assignments)
                .ThenInclude(x => x.UserBy)
                .Include(x => x.Assignments)
                .ThenInclude(x => x.UserTo)
                .FirstOrDefaultAsync(x=>x.Id == id && !x.IsDeleted);
        }
    }
}
