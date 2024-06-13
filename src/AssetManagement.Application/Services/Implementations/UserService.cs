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
using AutoMapper;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Domain.Constants;
using System.Text.RegularExpressions;



namespace AssetManagement.Application.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICryptographyHelper _cryptographyHelper;
        private readonly IHelper _helper;
        private readonly IMapper _mapper;

        public UserService (IUnitOfWork unitOfWork, ICryptographyHelper cryptographyHelper, IHelper helper, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _cryptographyHelper = cryptographyHelper;
            _helper = helper;
            _mapper = mapper;
        }

        public async Task<UserRegisterResponse> AddUserAsync(UserRegisterRequest userRegisterRequest)
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

            var newStaffCode = await GenerateNewStaffCode();

            var firstName = userRegisterRequest.FirstName.ToLower();
            var lastName = userRegisterRequest.LastName.ToLower();
            var username = _helper.GetUsername(firstName, lastName);

            var usernames = await _unitOfWork.UserRepository.GetAllAsync(u => true);
            var existingUserCount = usernames.Count(u => u.Username.StartsWith(username) && (u.Username.Length > username.Length && Char.IsDigit(u.Username[username.Length])));
            var existingUserCount2 = usernames.Count(u => u.Username == username);
            if (existingUserCount > 0 || existingUserCount2 > 0)
            {
                username += ++existingUserCount;
            }

            var password = $"{username}@{userRegisterRequest.DateOfBirth:ddMMyyyy}";
            var salt = _cryptographyHelper.GenerateSalt();
            var hashedPassword = _cryptographyHelper.HashPassword(password, salt);
            var adminUser = await _unitOfWork.UserRepository.GetAsync(u => u.Id == userRegisterRequest.CreateBy,
                u => u.Location);
            var createByAdmin = userRegisterRequest.CreateBy;

            var role = await _unitOfWork.RoleRepository.GetAsync(r => r.Id == userRegisterRequest.RoleId);

            var user = new User
            {
                StaffCode = newStaffCode.ToString(),
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
                CreatedBy = createByAdmin,
                Role = role
            };

            await _unitOfWork.UserRepository.AddAsync(user);
            if (await _unitOfWork.CommitAsync() < 1)
            {
                throw new InvalidOperationException("An error occurred while registering the user.");
            }
            else
            {
                return _mapper.Map<UserRegisterResponse>(user);
            }

        }

        private async Task<string> GenerateNewStaffCode()
        {
            var lastUser = await _unitOfWork.UserRepository.GetAllAsync();
            var lastStaffCode = lastUser.OrderByDescending(u => u.StaffCode).FirstOrDefault()?.StaffCode ?? StaffCode.DEFAULT_STAFF_CODE;
            var newStaffCode = $"SD{(int.Parse(lastStaffCode.Substring(2)) + 1):D4}";
            return  newStaffCode;

        }
    }
}
