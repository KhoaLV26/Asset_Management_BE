﻿using AssetManagement.Application.Models.Responses;
using AssetManagement.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace AssetManagement.Application.Services
{
    public interface IAuthService
    {
        Task<(string token, string refreshToken, GetUserResponse userResponse)> LoginAsync(string email, string password);

        Task<(string token, string refreshToken, GetUserResponse userResponse)> RefreshTokenAsync(string refreshToken);

        Task<int> LogoutAsync(Guid userId);

        Task<int> ResetPasswordAsync(string userName, string newPassword, string refreshToken);

        Task<int> ChangePasswordAsync(string userName, string oldPassword, string newPassword, string refreshToken, string currentToken);
    }
}