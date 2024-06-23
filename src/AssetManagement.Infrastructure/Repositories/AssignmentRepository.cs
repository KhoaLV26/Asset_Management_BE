using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Interfaces;
using AssetManagement.Infrastructure.DataAccess;
using AssetManagement.Infrastructure.UnitOfWork;
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
                .Include(a => a.UserBy)
                .ThenInclude(a => a.Username)
                .Include(a => a.UserTo)
                .ThenInclude(a => a.Username)

                .Select(a => new Assignment
                {
                    Id = a.Id,
                    AssetId = a.AssetId,
                    Asset = new Asset
                    {
                        Id = a.Asset.Id,
                        AssetCode = a.Asset.AssetCode,
                        AssetName = a.Asset.AssetName,
                    },
                    AssignedBy = a.AssignedBy,
                    AssignedTo = a.AssignedTo,
                    UserBy = new User
                    {
                        Id = a.UserBy.Id,
                        Username = a.UserBy.Username
                    },
                    UserTo = new User
                    {
                        Id = a.UserTo.Id,
                        Username = a.UserTo.Username
                    },
                    Note = a.Note
                })
                .ToListAsync();
        }
    }
}