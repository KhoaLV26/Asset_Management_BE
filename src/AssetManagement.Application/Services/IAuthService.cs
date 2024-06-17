using AssetManagement.Application.Models.Responses;
using AssetManagement.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace AssetManagement.Application.Services
{
    public interface IAuthService
    {
        //Task<User> RegisterAsync(string email, string password, int roleId);

        Task<(string token, string refreshToken, GetUserResponse userResponse)> LoginAsync(string email, string password);

        //Task<(string token, string refreshToken, string role, string userId)> RefreshTokenAsync(string refreshToken);

        //Task<int> LogoutAsync(Guid userId);

        Task<int> ResetPasswordAsync(string userName, string newPassword);
    }
}