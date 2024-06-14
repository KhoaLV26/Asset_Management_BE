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
using System.Linq.Expressions;



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

        public async Task<Guid> GetLocation(Guid id)
        {
            var user =  await _unitOfWork.UserRepository.GetAsync(x => x.Id == id);
            return user.LocationId;
        }

        private async Task<string> GenerateNewStaffCode()
        {
            var lastUser = await _unitOfWork.UserRepository.GetAllAsync();
            var lastStaffCode = lastUser.OrderByDescending(u => u.StaffCode).FirstOrDefault()?.StaffCode ?? StaffCode.DEFAULT_STAFF_CODE;
            var newStaffCode = $"SD{(int.Parse(lastStaffCode.Substring(2)) + 1):D4}";
            return  newStaffCode;

        }

        public async Task<(IEnumerable<GetUserResponse> Items, int TotalCount)> GetFilteredUsersAsync(
            string location,
            string? searchTerm,
            string? role = null,
            string sortBy = "StaffCode",
            string sortDirection = "asc",
            int pageNumber = 1,
            int pageSize = 15)
        {
            Expression<Func<User, bool>> filter = null;

            if (!string.IsNullOrEmpty(role))
            {
                filter = u => u.Role.Name == role;
            }

            Func<IQueryable<User>, IOrderedQueryable<User>> orderBy = null;

            bool ascending = sortDirection.ToLower() == "asc";

            switch (sortBy)
            {
                case "StaffCode":
                    orderBy = q => ascending ? q.OrderBy(u => u.StaffCode) : q.OrderByDescending(u => u.StaffCode);
                    break;
                case "JoinedDate":
                    orderBy = q => ascending ? q.OrderBy(u => u.DateJoined) : q.OrderByDescending(u => u.DateJoined);
                    break;
                case "Role":
                    orderBy = q => ascending ? q.OrderBy(u => u.Role.Name) : q.OrderByDescending(u => u.Role.Name);
                    break;
                default:
                    orderBy = q => ascending
                        ? q.OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
                        : q.OrderByDescending(u => u.FirstName).ThenByDescending(u => u.LastName);
                    break;
            }

            var getUsers = await _unitOfWork.UserRepository.GetFilteredAsync(location, filter, orderBy, "Role", searchTerm, pageNumber, pageSize);

            var userResponses = _mapper.Map<IEnumerable<GetUserResponse>>(getUsers.Items);
            var totalCount = getUsers.TotalCount;

            return (userResponses, totalCount);
        }
    }
}
