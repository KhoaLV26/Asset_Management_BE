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
using AssetManagement.Infrastructure.Helpers;


namespace AssetManagement.Application.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICryptographyHelper _cryptographyHelper;
        private readonly IHelper _helper;

        public UserService (IUnitOfWork unitOfWork, ICryptographyHelper cryptographyHelper, IHelper helper)
        {
            _unitOfWork = unitOfWork;
            _cryptographyHelper = cryptographyHelper;
            _helper = helper;
        }

        public async Task<User> AddUserAsync(UserRegisterRequest userRegisterRequest)
        {
            
            if (userRegisterRequest.DateOfBirth >= DateOnly.FromDateTime(DateTime.Now.AddYears(-18)))
            {
                throw new ArgumentException("User must be at least 18 years old");
            }

            if (userRegisterRequest.DateJoined < userRegisterRequest.DateOfBirth)
            {
                throw new ArgumentException("Joined date must be later than date of birth");
            }

            if (userRegisterRequest.DateJoined.DayOfWeek == DayOfWeek.Saturday || userRegisterRequest.DateJoined.DayOfWeek == DayOfWeek.Sunday)
            {
                throw new ArgumentException("Joined date cannot be Saturday or Sunday");
            }

            var lastUser = await _unitOfWork.UserRepository.GetAllAsync();
            var lastStaffCode = lastUser.OrderByDescending(u => u.StaffCode).FirstOrDefault()?.StaffCode ?? "SD0000";
            var newStaffCode = $"SD{(int.Parse(lastStaffCode.Substring(2)) + 1):D4}";

            var firstName = userRegisterRequest.FirstName.ToLower();
            var lastName = userRegisterRequest.LastName.ToLower();
            var username = _helper.GetUsername(firstName, lastName);

            var existingUser = await _unitOfWork.UserRepository.GetAsync(u => u.Username == username);
            if (existingUser != null)
            {
                int count = 1;
                while (existingUser != null)
                {
                    username = $"{username}{count}";
                    existingUser = await _unitOfWork.UserRepository.GetAsync(u => u.Username == username);
                    count++;
                }
            }

            var password = $"{username}@{userRegisterRequest.DateOfBirth:ddMMyyyy}";
            var salt = _cryptographyHelper.GenerateSalt();
            var hashedPassword = _cryptographyHelper.HashPassword(password, salt);
            var adminUser = await _unitOfWork.UserRepository.GetAsync(u => u.Id == userRegisterRequest.CreateBy);
            var CreateBy = userRegisterRequest.CreateBy;
            var user = new User
            {
                StaffCode = newStaffCode,
                Username = username,
                FirstName = userRegisterRequest.FirstName,
                LastName = userRegisterRequest.LastName,
                Gender = userRegisterRequest.Gender,
                HashPassword = hashedPassword,
                SaltPassword = salt,
                DateOfBirth = userRegisterRequest.DateOfBirth,
                DateJoined = userRegisterRequest.DateJoined,
                Status = EnumUserStatus.Active,
                LocationId = adminUser.LocationId,
                RoleId = userRegisterRequest.RoleId,
                IsFirstLogin = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = CreateBy
            };

            await _unitOfWork.UserRepository.AddAsync(user);
            await _unitOfWork.CommitAsync();
            return user;
        }
    }
}
