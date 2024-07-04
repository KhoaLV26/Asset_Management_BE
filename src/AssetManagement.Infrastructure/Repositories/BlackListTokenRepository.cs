using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Interfaces;
using AssetManagement.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Infrastructure.Repositories
{
    public class BlackListTokenRepository : GenericRepository<BlackListToken>, IBlackListTokenRepository
    {
        private readonly DBContext _context;

        public BlackListTokenRepository(DBContext context) : base(context)
        {
            _context = context;
        }
    }
}