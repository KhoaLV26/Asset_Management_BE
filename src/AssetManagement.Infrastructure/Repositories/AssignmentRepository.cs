using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Interfaces;
using AssetManagement.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Cmp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Infrastructure.Repositories
{
    public class AssignmentRepository : GenericRepository<Assignment>, IAssignmentRepository
    {
        private readonly DBContext _context;
        public AssignmentRepository(DBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Assignment>> GetAllAssignmentAsync()
        {
            return await _context.Assignments
                .Include(a => a.Asset)
                .ThenInclude(a => a.AssetCode)
                .Include(a => a.Asset)
                .ThenInclude(a => a.AssetName)
                .Include(a => a.UserBy)
                .Include(a => a.UserTo)
                .ToListAsync();
        }

        public async Task<Assignment?> GetAssignmentDetailAsync(Guid id)
        {
            return await _context.Assignments
                .Include(a => a.Asset)
                .ThenInclude(a => a.AssetCode)
                .Include(a => a.Asset)
                .ThenInclude(a => a.AssetName)
                .Include(a => a.UserBy)
                .Include(a => a.UserTo)
                .FirstOrDefaultAsync(a => a.Id == id);
        }
    }
}