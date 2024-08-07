﻿using AssetManagement.Application.Models.Responses;
using AssetManagement.Application.Services.Implementations;
using AssetManagement.Domain.Constants;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Interfaces;
using AssetManagement.Infrastructure.Helpers;
using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace AssetManagement.Test.Unit.UserServiceTest
{
    [ExcludeFromCodeCoverage]
    public class GetFilteredUsersTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICryptographyHelper> _cryptographyHelperMock;
        private readonly Mock<IHelper> _helperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly UserService _userService;


        public GetFilteredUsersTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _cryptographyHelperMock = new Mock<ICryptographyHelper>();
            _helperMock = new Mock<IHelper>();
            _mapperMock = new Mock<IMapper>();
            _userService = new UserService(_unitOfWorkMock.Object, null, null, _mapperMock.Object);
        }

        [Fact]
        public async Task GetFilteredUsersAsync_ReturnsFilteredUsers()
        {
            // Arrange
            string adminId = Guid.NewGuid().ToString();
            string searchTerm = "search";
            Guid roleId = Guid.NewGuid();
            string sortBy = "StaffCode";
            string sortDirection = "asc";
            int pageNumber = 1;
            string newStaffCode = "NS001";

            var expectedUsers = new List<User>
    {
        new User { Id = Guid.NewGuid(), StaffCode = "NS001", FirstName = "John", LastName = "Doe", Username = "johndoe", Role = new Role { Id = roleId, Name = "Admin" }, Location = new Location { Id = Guid.Parse(adminId) } },
        new User { Id = Guid.NewGuid(), StaffCode = "NS002", FirstName = "Jane", LastName = "Doe", Username = "janedoe", Role = new Role { Id = roleId, Name = "User" }, Location = new Location { Id = Guid.Parse(adminId) } }
    };

            var expectedUserResponses = new List<GetUserResponse>
    {
        new GetUserResponse { StaffCode = "NS001", FirstName = "John", LastName = "Doe", Username = "johndoe", RoleName = "Admin" },
        new GetUserResponse { StaffCode = "NS002", FirstName = "Jane", LastName = "Doe", Username = "janedoe", RoleName = "User" }
    };

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(r => r.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<int>()
            )).ReturnsAsync((expectedUsers, expectedUsers.Count));

            userRepositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(expectedUsers.First());

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(u => u.UserRepository).Returns(userRepositoryMock.Object);

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<IEnumerable<GetUserResponse>>(expectedUsers))
                .Returns(expectedUserResponses);

            var userService = new UserService(unitOfWorkMock.Object, null, null, mapperMock.Object);

            // Act
            var result = await userService.GetFilteredUsersAsync(adminId, searchTerm, roleId.ToString(), sortBy, sortDirection, pageNumber, newStaffCode);

            // Assert
            Assert.Equal(expectedUserResponses, result.Items);
            Assert.Equal(expectedUsers.Count, result.TotalCount);
        }

        [Fact]
        public async Task GetLocation_ReturnsUserLocationId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expectedLocationId = Guid.NewGuid();

            var user = new User { Id = userId, LocationId = expectedLocationId };

            _unitOfWorkMock.Setup(u => u.UserRepository.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.GetLocation(userId);

            // Assert
            Assert.Equal(expectedLocationId, result);
        }

        [Fact]
        public async Task GetUserFilterQuery_ReturnsCorrectFilterExpression()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var role = "Admin";
            var search = "john";

            var locationId = Guid.NewGuid();

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(new User { Id = adminId, LocationId = locationId });

            _unitOfWorkMock.Setup(u => u.UserRepository).Returns(userRepositoryMock.Object);

            // Act
            var filterExpressionTask = _userService.GetType()
                .GetMethod("GetUserFilterQuery", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_userService, new object[] { adminId, role, search }) as Task<Expression<Func<User, bool>>>;

            var filterExpression = filterExpressionTask.Result;

            // Assert
            Assert.NotNull(filterExpression);
        }

        [Theory]
        [InlineData(SortConstants.User.SORT_BY_STAFF_CODE, true)]
        [InlineData(SortConstants.User.SORT_BY_STAFF_CODE, false)]
        [InlineData(SortConstants.User.SORT_BY_JOINED_DATE, true)]
        [InlineData(SortConstants.User.SORT_BY_JOINED_DATE, false)]
        [InlineData(SortConstants.User.SORT_BY_ROLE, true)]
        [InlineData(SortConstants.User.SORT_BY_ROLE, false)]
        [InlineData(SortConstants.User.SORT_BY_USERNAME, true)]
        [InlineData(SortConstants.User.SORT_BY_USERNAME, false)]
        [InlineData("InvalidSortBy", true)]
        public async Task GetFilteredUsersAsync_SortingScenarios_ReturnsSortedUsers(string sortBy, bool ascending)
        {
            // Arrange
            string adminId = Guid.NewGuid().ToString();
            var roleId = Guid.NewGuid();
            var sortDirection = ascending ? "asc" : "desc";

            var users = new List<User>
    {
        new User { Id = Guid.NewGuid(), StaffCode = "NS001", FirstName = "John", LastName = "Doe", Username = "johndoe", DateJoined = DateOnly.FromDateTime(DateTime.Now), Role = new Role { Id = roleId, Name = "Admin" }, Location = new Location { Id = Guid.Parse(adminId) } },
        new User { Id = Guid.NewGuid(), StaffCode = "NS001", FirstName = "John", LastName = "John", Username = "johndoe", DateJoined = DateOnly.FromDateTime(DateTime.Now), Role = new Role { Id = roleId, Name = "Admin" }, Location = new Location { Id = Guid.Parse(adminId) } }
    };

            var userResponses = users.Select(u => new GetUserResponse
            {
                StaffCode = u.StaffCode,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Username = u.Username,
                DateJoined = u.DateJoined,
                RoleName = u.Role.Name
            }).ToList();

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(r => r.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<int>()
            )).ReturnsAsync((users, users.Count));

            userRepositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(users.First());

            _unitOfWorkMock.Setup(u => u.UserRepository).Returns(userRepositoryMock.Object);

            _mapperMock.Setup(m => m.Map<IEnumerable<GetUserResponse>>(It.IsAny<IEnumerable<User>>()))
                .Returns(userResponses);

            // Act
            var result = await _userService.GetFilteredUsersAsync(adminId, null, null, sortBy, sortDirection);

            // Assert
            Assert.Equal(users.Count, result.TotalCount);

            // Verify sorting
            var sortedResponses = result.Items.ToList();
            switch (sortBy)
            {
                case SortConstants.User.SORT_BY_STAFF_CODE:
                    Assert.Equal(ascending ? userResponses.OrderBy(u => u.StaffCode) : userResponses.OrderByDescending(u => u.StaffCode), sortedResponses);
                    break;
                case SortConstants.User.SORT_BY_JOINED_DATE:
                    Assert.Equal(ascending ? userResponses.OrderBy(u => u.DateJoined) : userResponses.OrderByDescending(u => u.DateJoined), sortedResponses);
                    break;
                case SortConstants.User.SORT_BY_ROLE:
                    Assert.Equal(ascending ? userResponses.OrderBy(u => u.RoleName) : userResponses.OrderByDescending(u => u.RoleName), sortedResponses);
                    break;
                case SortConstants.User.SORT_BY_USERNAME:
                    Assert.Equal(ascending ? userResponses.OrderBy(u => u.Username) : userResponses.OrderByDescending(u => u.Username), sortedResponses);
                    break;
                default:
                    Assert.Equal(ascending
                        ? userResponses.OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
                        : userResponses.OrderBy(u => u.FirstName).ThenBy(u => u.LastName),
                        sortedResponses);
                    break;
            }
        }
    }
}
