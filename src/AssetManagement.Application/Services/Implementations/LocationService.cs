using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Domain.Constants;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Domain.Interfaces;
using AssetManagement.Infrastructure.Helpers;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.Services.Implementations
{
    public class LocationService : ILocationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICryptographyHelper _cryptographyHelper;

        public LocationService(IUnitOfWork unitOfWork, IMapper mapper, ICryptographyHelper cryptographyHelper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cryptographyHelper = cryptographyHelper;
        }

        public async Task<(IEnumerable<LocationResponse> data, int totalCount)> GetAllLocationAsync(int pageNumber, string? search, string? sortOrder = "asc", string? sortBy = "Name", string? newLocationCode = "", int pageSize = 10)
        {
            Func<IQueryable<Location>, IOrderedQueryable<Location>> orderBy = null;
            bool ascending = string.IsNullOrEmpty(sortOrder) || sortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase);

            switch (sortBy)
            {
                case SortConstants.Location.SORT_BY_NAME:
                    orderBy = q => ascending ? q.OrderBy(u => u.Name) : q.OrderByDescending(u => u.Name);
                    break;

                case SortConstants.Location.SORT_BY_CODE:
                    orderBy = q => ascending ? q.OrderBy(u => u.Code) : q.OrderByDescending(u => u.Code);
                    break;
            }
            Expression<Func<Location, bool>> prioritizeCondition = null;

            if (!string.IsNullOrEmpty(newLocationCode))
            {
                prioritizeCondition = u => u.Code == newLocationCode;
            }

            var locations = await _unitOfWork.LocationRepository
                .GetAllAsync(pageNumber,
                x => !x.IsDeleted &&
                (string.IsNullOrEmpty(search) || x.Name.Contains(search) || x.Code.Contains(search)),
                orderBy,
                "",
                prioritizeCondition,
                pageSize);

            return (_mapper.Map<IEnumerable<LocationResponse>>(locations.items), locations.totalCount);
        }

        public async Task<LocationCreateResponse> CreateLocationAsync(LocationCreateRequest request)
        {
            var locationNameExisted = await _unitOfWork.LocationRepository.GetAsync(x => x.Name == request.Name);
            if (locationNameExisted != null)
            {
                throw new Exception("Location name already existed");
            }
            var locationCodeExisted = await _unitOfWork.LocationRepository.GetAsync(x => x.Code == request.Code);
            if (locationCodeExisted != null)
            {
                throw new Exception("Location code already existed");
            }

            Location mewLocation = new Location
            {
                Name = request.Name,
                Code = request.Code.ToUpper()
            };

            await _unitOfWork.LocationRepository.AddAsync(mewLocation);

            var infoUser = $"admin{mewLocation.Code.ToLower()}";
            var usernames = await _unitOfWork.UserRepository.GetAllAsync();
            var existingUserCount = usernames.Count(u => u.Username.StartsWith(infoUser) && (u.Username.Length > infoUser.Length && Char.IsDigit(u.Username[infoUser.Length])));
            var existingUserCount2 = usernames.Count(u => u.Username == infoUser);
            if (existingUserCount > 0 || existingUserCount2 > 0)
            {
                infoUser += ++existingUserCount;
            }
            var salt = _cryptographyHelper.GenerateSalt();
            var hashedPassword = _cryptographyHelper.HashPassword(infoUser, salt);

            var newStaffCode = await GenerateNewStaffCode();

            var newUser = new User
            {
                LastName = "Admin",
                FirstName = mewLocation.Code,
                Username = infoUser,
                HashPassword = hashedPassword,
                SaltPassword = salt,
                Gender = EnumGender.Male,
                DateOfBirth = new DateOnly(1990, 1, 1),
                DateJoined = DateOnly.FromDateTime(DateTime.Now),
                RoleId = RoleAdminId.ROLE_ADMIN_ID,
                Status = EnumUserStatus.Active,
                LocationId = mewLocation.Id,
                StaffCode = newStaffCode
            };

            await _unitOfWork.UserRepository.AddAsync(newUser);

            if (await _unitOfWork.CommitAsync() > 0)
            {
                return new LocationCreateResponse
                {
                    Id = mewLocation.Id,
                    Name = mewLocation.Name,
                    Code = mewLocation.Code,
                    UserName = infoUser,
                    Password = infoUser
                };
            }

            throw new Exception("Failed to create location");
        }

        private async Task<string> GenerateNewStaffCode()
        {
            var lastUser = await _unitOfWork.UserRepository.GetAllAsync(u => true);
            var lastStaffCode = lastUser.OrderByDescending(u => u.StaffCode).FirstOrDefault()?.StaffCode ?? StaffCode.DEFAULT_STAFF_CODE;
            var newStaffCodeNumber = int.Parse(lastStaffCode.Substring(StaffCode.STAFF_CODE_PREFIX.Length)) + 1;
            var newStaffCode = string.Format(StaffCode.STAFF_CODE_FORMAT, newStaffCodeNumber);
            return newStaffCode;
        }

        public async Task<LocationResponse> UpdateLocationAsync(Guid locationId, LocationUpdateRequest request)
        {
            var location = await _unitOfWork.LocationRepository.GetAsync(x => x.Id == locationId);
            if (location == null)
            {
                throw new KeyNotFoundException("Location not found");
            }
            var locationNameExisted = await _unitOfWork.LocationRepository.GetAsync(x => x.Name == request.Name);
            var locationCodeExisted = await _unitOfWork.LocationRepository.GetAsync(x => x.Code == request.Code);
            if (locationNameExisted != null && locationNameExisted.Id != locationId)
            {
                throw new Exception("Location name already existed");
            }
            if (locationCodeExisted != null && locationCodeExisted.Id != locationId)
            {
                throw new Exception("Location code already existed");
            }

            location.Name = request.Name ?? location.Name;
            location.Code = string.Empty.Equals(request.Code) ? location.Code : request.Code.ToUpper();

            _unitOfWork.LocationRepository.Update(location);
            if (await _unitOfWork.CommitAsync() > 0)
            {
                return _mapper.Map<LocationResponse>(location);
            }

            throw new Exception("Failed to update location");
        }

        public async Task<LocationResponse> GetLocationByIdAsync(Guid locationId)
        {
            var location = await _unitOfWork.LocationRepository.GetAsync(x => x.Id == locationId);
            if (location == null)
            {
                throw new KeyNotFoundException("Location not found");
            }

            return _mapper.Map<LocationResponse>(location);
        }
    }
}