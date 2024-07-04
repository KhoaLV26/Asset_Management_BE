using AssetManagement.Domain.Interfaces;
using AssetManagement.Infrastructure.DataAccess;
using AssetManagement.Infrastructure.Repositories;
using System.Threading.Tasks;

namespace AssetManagement.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DBContext _context;
        private IAssetRepository _assetRepository;
        private IAssignmentRepository _assignmentRepository;
        private ICategoryRepository _categoryRepository;
        private ILocationRepository _locationRepository;
        private IRefreshTokenRepository _refreshTokenRepository;
        private IReturnRequestRepository _returnRequestRepository;
        private IRoleRepository _roleRepository;
        private IUserRepository _userRepository;
        private IBlackListTokenRepository _blackListTokenRepository;
        private ITokenRepository _tokenRepository;

        public UnitOfWork(DBContext context)
        {
            _context = context;
        }

        public IAssetRepository AssetRepository
            => _assetRepository ??= new AssetRepository(_context);

        public IAssignmentRepository AssignmentRepository
            => _assignmentRepository ??= new AssignmentRepository(_context);

        public ICategoryRepository CategoryRepository
            => _categoryRepository ??= new CategoryRepository(_context);

        public ILocationRepository LocationRepository
            => _locationRepository ??= new LocationRepository(_context);

        public IReturnRequestRepository ReturnRequestRepository
            => _returnRequestRepository ??= new ReturnRequestRepository(_context);

        public IRoleRepository RoleRepository
            => _roleRepository ??= new RoleRepository(_context);

        public IUserRepository UserRepository
            => _userRepository ??= new UserRepository(_context);

        public IRefreshTokenRepository RefreshTokenRepository
            => _refreshTokenRepository ??= new RefreshTokenRepository(_context);

        public IBlackListTokenRepository BlackListTokenRepository
            => _blackListTokenRepository ??= new BlackListTokenRepository(_context);

        public ITokenRepository TokenRepository
            => _tokenRepository ??= new TokenRepository(_context);

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public int Commit()
        {
            return _context.SaveChanges();
        }
    }
}