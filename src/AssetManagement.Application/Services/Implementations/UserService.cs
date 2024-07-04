using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Domain.Constants;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Domain.Interfaces;
using AssetManagement.Infrastructure.Helpers;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AssetManagement.Application.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICryptographyHelper _cryptographyHelper;
        private readonly IHelper _helper;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, ICryptographyHelper cryptographyHelper, IHelper helper, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _cryptographyHelper = cryptographyHelper;
            _helper = helper;
            _mapper = mapper;
        }

        public async Task<UserRegisterResponse> AddUserAsync(UserRegisterRequest userRegisterRequest)
        {
            if (userRegisterRequest.DateJoined.AddYears(AgeConstants.REQUIRED_AGE) <= userRegisterRequest.DateOfBirth)
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

            var usernames = await _unitOfWork.UserRepository.GetAllAsync();
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
            var lastUser = await _unitOfWork.UserRepository.GetAllAsync(u => true);
            var lastStaffCode = lastUser.OrderByDescending(u => u.StaffCode).FirstOrDefault()?.StaffCode ?? StaffCode.DEFAULT_STAFF_CODE;
            var newStaffCodeNumber = int.Parse(lastStaffCode.Substring(StaffCode.STAFF_CODE_PREFIX.Length)) + 1;
            var newStaffCode = string.Format(StaffCode.STAFF_CODE_FORMAT, newStaffCodeNumber);
            return newStaffCode;
        }

        public async Task<(IEnumerable<GetUserResponse> Items, int TotalCount)> GetFilteredUsersAsync(
            string adminId,
            string? searchTerm,
            string? role = null,
            string sortBy = "StaffCode",
            string sortDirection = "asc",
            int pageNumber = 1,
            string? newStaffCode = "")
        {
            Expression<Func<User, bool>> filter = null;
            Guid adminGuid = Guid.Parse(adminId);

            filter = await GetUserFilterQuery(adminGuid, role, searchTerm);

            Expression<Func<User, bool>> prioritizeCondition = null;

            if (!string.IsNullOrEmpty(newStaffCode))
            {
                prioritizeCondition = u => u.StaffCode == newStaffCode;
            }

            Func<IQueryable<User>, IOrderedQueryable<User>> orderBy = null;

            bool ascending = sortDirection.ToLower() == "asc";

            switch (sortBy)
            {
                case SortConstants.User.SORT_BY_STAFF_CODE:
                    orderBy = q => ascending ? q.OrderBy(u => u.StaffCode) : q.OrderByDescending(u => u.StaffCode);
                    break;

                case SortConstants.User.SORT_BY_JOINED_DATE:
                    orderBy = q => ascending ? q.OrderBy(u => u.DateJoined) : q.OrderByDescending(u => u.DateJoined);
                    break;

                case SortConstants.User.SORT_BY_ROLE:
                    orderBy = q => ascending ? q.OrderBy(u => u.Role.Name) : q.OrderByDescending(u => u.Role.Name);
                    break;

                case SortConstants.User.SORT_BY_USERNAME:
                    orderBy = q => ascending ? q.OrderBy(u => u.Username) : q.OrderByDescending(u => u.Username);
                    break;

                default:
                    orderBy = q => ascending
                        ? q.OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
                        : q.OrderByDescending(u => u.FirstName).ThenByDescending(u => u.LastName);
                    break;
            }

            var getUsers = await _unitOfWork.UserRepository.GetAllAsync(pageNumber, filter, orderBy, "Role,Location", prioritizeCondition);

            var userResponses = _mapper.Map<IEnumerable<GetUserResponse>>(getUsers.items);
            var totalCount = getUsers.totalCount;

            return (userResponses, totalCount);
        }

        private async Task<Expression<Func<User, bool>>>? GetUserFilterQuery(Guid adminId, string? role, string? search)
        {
            var locationId = await GetLocation(adminId);
            var nullableLocationId = (Guid)locationId;

            Expression<Func<User, bool>>? filter = null;
            var parameter = Expression.Parameter(typeof(User), "x");
            var conditions = new List<Expression>();

            var locationCondition = Expression.Equal(Expression.Property(parameter, nameof(User.LocationId)),
                Expression.Constant(nullableLocationId, typeof(Guid)));
            conditions.Add(locationCondition);

            // Add role condition
            if (!string.IsNullOrEmpty(role))
            {
                if (role.ToLower() != "all")
                {
                    var roleProperty = Expression.Property(parameter, nameof(User.Role));
                    var roleNameProperty = Expression.Property(roleProperty, nameof(Role.Name));
                    var roleCondition = Expression.Equal(
                        roleNameProperty,
                        Expression.Constant(role, typeof(string))
                    );
                    conditions.Add(roleCondition);
                }
            }

            var isDeletedCondition = Expression.Equal(Expression.Property(parameter, nameof(User.IsDeleted)),
                Expression.Constant(false));
            conditions.Add(isDeletedCondition);

            // Add search condition
            if (!string.IsNullOrEmpty(search))
            {
                // Combine FirstName and LastName
                var firstNameProperty = Expression.Property(parameter, nameof(User.FirstName));
                var lastNameProperty = Expression.Property(parameter, nameof(User.LastName));
                var fullNameExpression = Expression.Call(
                    typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) }),
                    Expression.Call(
                        typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) }),
                        firstNameProperty,
                        Expression.Constant(" ")
                    ),
                    lastNameProperty
                );

                // MethodInfo for EF.Functions.Like
                var likeMethod = typeof(DbFunctionsExtensions).GetMethod(
                    nameof(DbFunctionsExtensions.Like),
                    new[] { typeof(DbFunctions), typeof(string), typeof(string) }
                );

                // Ensure the method is not null
                if (likeMethod == null)
                {
                    throw new InvalidOperationException("EF.Functions.Like method not found.");
                }

                var efFunctionsProperty = Expression.Constant(EF.Functions);

                var staffCodeLike = Expression.Call(
                    likeMethod,
                    efFunctionsProperty,
                    Expression.Property(parameter, nameof(User.StaffCode)),
                    Expression.Constant($"%{search}%")
                );

                var fullNameLike = Expression.Call(
                    likeMethod,
                    efFunctionsProperty,
                    fullNameExpression,
                    Expression.Constant($"%{search}%")
                );

                var usernameLike = Expression.Call(
                    likeMethod,
                    efFunctionsProperty,
                    Expression.Property(parameter, nameof(User.Username)),
                    Expression.Constant($"%{search}%")
                );

                // Combine like expressions with OR
                var searchCondition = Expression.OrElse(
                    Expression.OrElse(staffCodeLike, fullNameLike),
                    usernameLike
                );
                conditions.Add(searchCondition);
            }

            // Combine all conditions
            if (conditions.Any())
            {
                var combinedCondition = conditions.Aggregate((left, right) => Expression.AndAlso(left, right));
                filter = Expression.Lambda<Func<User, bool>>(combinedCondition, parameter);
            }

            return filter;
        }

        public async Task<Guid> GetLocation(Guid id)
        {
            var user = await _unitOfWork.UserRepository.GetAsync(x => x.Id == id);
            return user.LocationId;
        }

        public async Task<bool> DisableUser(Guid id)
        {
            var user = await _unitOfWork.UserRepository.GetAsync(u => !u.IsDeleted && u.Id == id);
            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            var assignments = await _unitOfWork.AssignmentRepository.GetAllAsync(
                a => a.AssignedTo == id && !a.IsDeleted);

            if (assignments.Any(a => a.Status == EnumAssignmentStatus.Accepted || a.Status == EnumAssignmentStatus.WaitingForAcceptance))
            {
                return false;
            }

            user.IsDeleted = true;
            _unitOfWork.UserRepository.Update(user);

            var tokens = await _unitOfWork.TokenRepository.GetAllAsync(rt => rt.UserId == id);
            foreach (var token in tokens)
            {
                await _unitOfWork.BlackListTokenRepository.AddAsync(new BlackListToken
                {
                    Token = token.HashToken
                });
            }

            var refreshTokens = await _unitOfWork.RefreshTokenRepository.GetAllAsync(rt => rt.UserId == id);
            _unitOfWork.RefreshTokenRepository.RemoveRange(refreshTokens);

            var result = await _unitOfWork.CommitAsync();
            return result > 0;
        }

        public async Task<UserDetailResponse> GetUserDetailAsync(Guid id)
        {
            var user = await _unitOfWork.UserRepository.GetAsync(u => u.Id == id && u.IsDeleted == false);
            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            return new UserDetailResponse
            {
                DateOfBirth = user.DateOfBirth,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Gender = user.Gender,
                DateJoined = user.DateJoined,
                RoleId = user.RoleId
            };
        }

        public async Task<UpdateUserResponse> UpdateUserAsync(Guid id, EditUserRequest request, Guid currentUserId)
        {
            if (currentUserId == id)
            {
                throw new ArgumentException("Can't edit yourself");
            }
            var user = await _unitOfWork.UserRepository.GetAsync(x => x.IsDeleted == false && x.Id == id, includeProperties: x => x.Role);
            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }
            var role = await _unitOfWork.RoleRepository.GetAsync(x => x.IsDeleted == false && x.Id == request.RoleId);
            if (role == null)
            {
                throw new ArgumentException("Role not found.");
            }
            var tokens = await _unitOfWork.TokenRepository.GetAllAsync(rt => rt.UserId == id);
            foreach (var token in tokens)
            {
                await _unitOfWork.BlackListTokenRepository.AddAsync(new BlackListToken
                {
                    Token = token.HashToken
                });
            }
            var refreshTokens = await _unitOfWork.RefreshTokenRepository.GetAllAsync(rt => rt.UserId == id);
            _unitOfWork.RefreshTokenRepository.RemoveRange(refreshTokens);
            user.DateJoined = request.DateJoined;
            user.Gender = request.Gender;
            user.DateOfBirth = request.DateOfBirth;
            user.RoleId = request.RoleId;
            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.CommitAsync();
            return new UpdateUserResponse
            {
                StaffCode = user.StaffCode
            };
        }
    }
}