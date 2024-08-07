﻿using AssetManagement.Application.Models.Responses;
using AssetManagement.Domain.Constants;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Domain.Interfaces;
using AssetManagement.Infrastructure.Helpers;
using AssetManagement.Infrastructure.Services;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly ICryptographyHelper _cryptographyHelper;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public AuthService(IUnitOfWork unitOfWork, ITokenService tokenService, ICryptographyHelper cryptographyHelper, IEmailService emailService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _cryptographyHelper = cryptographyHelper;
            _emailService = emailService;
            _mapper = mapper;
        }

        public async Task<(string token, string refreshToken, GetUserResponse userResponse)> LoginAsync(string username, string password)
        {
            var user = await _unitOfWork.UserRepository.GetAsync(u => !u.IsDeleted && u.Username == username, u => u.Role, u => u.Location);
            if (user == null || !_cryptographyHelper.VerifyPassword(password, user.HashPassword, user.SaltPassword))
            {
                throw new UnauthorizedAccessException("Email or password incorrect!!!");
            }

            if (user.Status != EnumUserStatus.Active)
            {
                throw new UnauthorizedAccessException("Account is not activated");
            }

            var token = _tokenService.GenerateToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            refreshToken.UserId = user.Id;

            await _unitOfWork.RefreshTokenRepository.AddAsync(refreshToken);
            await _unitOfWork.TokenRepository.AddAsync(new Token
            {
                UserId = user.Id,
                HashToken = token,
            });
            await _unitOfWork.CommitAsync();

            return (token, refreshToken.TokenHash, _mapper.Map<GetUserResponse>(user));
        }

        public async Task<(string token, string refreshToken, GetUserResponse userResponse)> RefreshTokenAsync(string refreshToken)
        {
            var token = await _unitOfWork.RefreshTokenRepository.GetAsync(rt => rt.TokenHash == refreshToken)
                .ConfigureAwait(false);
            if (token == null || token.ExpiredAt <= DateTime.Now)
            {
                throw new SecurityTokenException("Invalid refresh token");
            }

            var user = await _unitOfWork.UserRepository.GetAsync(u => u.Id == token.UserId, u => u.Role, u => u.Location);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            var newJwtToken = _tokenService.GenerateToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            newRefreshToken.UserId = user.Id;

            _unitOfWork.RefreshTokenRepository.Delete(token);
            await _unitOfWork.RefreshTokenRepository.AddAsync(newRefreshToken);
            await _unitOfWork.TokenRepository.AddAsync(new Token
            {
                UserId = user.Id,
                HashToken = newJwtToken,
            });
            await _unitOfWork.CommitAsync();

            return (newJwtToken, newRefreshToken.TokenHash, _mapper.Map<GetUserResponse>(user));
        }

        public async Task<int> ResetPasswordAsync(string userName, string newPassword, string refreshToken)
        {
            var user = await _unitOfWork.UserRepository.GetAsync(u => u.Username == userName);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            var passwordSalt = _cryptographyHelper.GenerateSalt();
            var passwordHash = _cryptographyHelper.HashPassword(newPassword, passwordSalt);

            user.HashPassword = passwordHash;
            user.SaltPassword = passwordSalt;
            user.IsFirstLogin = false;
            _unitOfWork.UserRepository.Update(user);
            var tokens = await _unitOfWork.RefreshTokenRepository.GetAllAsync(rt => rt.TokenHash != refreshToken);
            _unitOfWork.RefreshTokenRepository.RemoveRange(tokens);
            return await _unitOfWork.CommitAsync();
        }

        public async Task<int> ChangePasswordAsync(string userName, string oldPassword, string newPasswrod, string refreshToken, string currentToken)
        {
            var user = await _unitOfWork.UserRepository.GetAsync(u => u.Username == userName);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            if (!_cryptographyHelper.VerifyPassword(oldPassword, user.HashPassword, user.SaltPassword))
            {
                throw new UnauthorizedAccessException("Password is incorrect");
            }

            var passwordSalt = _cryptographyHelper.GenerateSalt();
            var passwordHash = _cryptographyHelper.HashPassword(newPasswrod, passwordSalt);

            user.HashPassword = passwordHash;
            user.SaltPassword = passwordSalt;
            user.IsFirstLogin = false;
            _unitOfWork.UserRepository.Update(user);
            var tokens = await _unitOfWork.RefreshTokenRepository.GetAllAsync(rt => rt.TokenHash != refreshToken);
            _unitOfWork.RefreshTokenRepository.RemoveRange(tokens);
            var otherTokens = await _unitOfWork.TokenRepository.GetAllAsync(t => t.HashToken != currentToken);
            //add otherTokens to blacklist
            foreach (var t in otherTokens)
            {
                await _unitOfWork.BlackListTokenRepository.AddAsync(new BlackListToken
                {
                    Token = t.HashToken,
                });
            }
            return await _unitOfWork.CommitAsync();
        }

        public async Task<int> LogoutAsync(Guid userId)
        {
            var tokens = await _unitOfWork.RefreshTokenRepository.GetAllAsync(rt => rt.UserId == userId);
            _unitOfWork.RefreshTokenRepository.RemoveRange(tokens);
            return await _unitOfWork.CommitAsync();
        }
    }
}