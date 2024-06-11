using AssetManagement.Domain.Entities;

namespace AssetManagement.Application.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);

        RefreshToken GenerateRefreshToken();
    }
}