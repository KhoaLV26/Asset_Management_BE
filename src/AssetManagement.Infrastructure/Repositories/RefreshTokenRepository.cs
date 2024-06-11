using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Interfaces;
using AssetManagement.Infrastructure.DataAccess;

namespace AssetManagement.Infrastructure.Repositories
{
    internal class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(DBContext context) : base(context)
        {
        }
    }
}