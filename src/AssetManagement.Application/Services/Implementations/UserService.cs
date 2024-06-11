using AssetManagement.Application.Models.Requests;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Domain.Interfaces;
using Org.BouncyCastle.Crypto.Generators;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;

namespace AssetManagement.Application.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserService (IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AddUserAsync(UserRegisterRequest userRegisterRequest)
        {
            
            if (userRegisterRequest.DateOfBirth >= DateOnly.FromDateTime(DateTime.Now.AddYears(-18)))
            {
                throw new ArgumentException("User must be at least 18 years old");
            }

            if (userRegisterRequest.DateJoined < userRegisterRequest.DateOfBirth)
            {
                throw new ArgumentException("Joined date must be later than date of birth");
            }

            var lastUser = await _unitOfWork.UserRepository.GetAllAsync();
            var lastStaffCode = lastUser.OrderByDescending(u => u.StaffCode).FirstOrDefault()?.StaffCode ?? "SD0000";
            var newStaffCode = $"SD{(int.Parse(lastStaffCode.Substring(2)) + 1):D4}";

            var firstName = userRegisterRequest.FirstName.ToLower();
            var lastName = userRegisterRequest.LastName.ToLower();
            var username = $"{firstName}{lastName.Substring(0, 1)}";
            var usernameCount = await _unitOfWork.UserRepository.CountAsync(u => u.Username.StartsWith(username));
            if (usernameCount > 0)
            {
                username += usernameCount;
            }

            var password = $"{username}@{userRegisterRequest.DateOfBirth:ddMMyyyy}";

            var user = new User
            {
                StaffCode = newStaffCode,
                Username = username,
                FirstName = userRegisterRequest.FirstName,
                LastName = userRegisterRequest.LastName,
                Gender = userRegisterRequest.Gender,
                HashPassword = BCrypt.Net.BCrypt.HashPassword(password),
                SaltPassword = BCrypt.Net.BCrypt.GenerateSalt(),
                DateOfBirth = userRegisterRequest.DateOfBirth,
                DateJoined = userRegisterRequest.DateJoined,
                Status = EnumUserStatus.Active,
                LocationId = _unitOfWork.LocationRepository.GetAsync(l => l.Name == "Admin Location").Result.Id,
                RoleId = userRegisterRequest.RoleId,
                IsFirstLogin = true
            };

            await _unitOfWork.UserRepository.AddAsync(user);
            await _unitOfWork.CommitAsync();
        }
    }
}
