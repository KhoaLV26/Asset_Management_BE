using AssetManagement.Application.Models.Responses;
using AssetManagement.Application.Services.Implementations;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Interfaces;
using AssetManagement.Infrastructure.Helpers;
using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace AssetManagement.Test.Unit.UserServiceTest
{
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
            _userService = new UserService(_unitOfWorkMock.Object, _cryptographyHelperMock.Object, _helperMock.Object, _mapperMock.Object);
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
    }
}
