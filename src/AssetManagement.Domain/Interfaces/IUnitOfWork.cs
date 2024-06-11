using System.Threading.Tasks;

namespace AssetManagement.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        IAssetRepository AssetRepository { get; }
        IAssignmentRepository AssignmentRepository { get; }
        ICategoryRepository CategoryRepository { get; }
        ILocationRepository LocationRepository { get; }
        IRefreshTokenRepository RefreshTokenRepository { get; }
        IReturnRequestRepository ReturnRequestRepository { get; }
        IRoleRepository RoleRepository { get; }
        IUserRepository UserRepository { get; }

        Task<int> CommitAsync();

        int Commit();
    }
}