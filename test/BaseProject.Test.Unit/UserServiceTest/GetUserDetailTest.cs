using AssetManagement.Application.Models.Responses;
using AssetManagement.Application.Services.Implementations;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Domain.Interfaces;
using AssetManagement.Infrastructure.Helpers;
using AutoMapper;
using Moq;
using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace AssetManagement.Test.Unit.UserServiceTest
{
    [ExcludeFromCodeCoverage]
    public class GetUserDetailTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICryptographyHelper> _cryptographyHelperMock;
        private readonly Mock<IHelper> _helperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly UserService _userService;

        public GetUserDetailTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _cryptographyHelperMock = new Mock<ICryptographyHelper>();
            _helperMock = new Mock<IHelper>();
            _mapperMock = new Mock<IMapper>();
            _userService = new UserService(_unitOfWorkMock.Object, _cryptographyHelperMock.Object, _helperMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task GetUserDetailAsync_UserFound_ReturnCorrespondingUserResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var role = new Role
            {
                Id = roleId,
                Name = "Role"
            };
            var location = new Location
            {
                Id = Guid.NewGuid(),
                Name = "Location"
            };
            var user = new User
            {
                Id = userId,
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now),
                FirstName = "Huy",
                LastName = "Phuc",
                Gender = EnumGender.Male,
                DateJoined = DateOnly.FromDateTime(DateTime.Now),
                RoleId = roleId,
                IsDeleted = false,
                Role = role,
                Location = location,
                Username = "",
                StaffCode = ""
            };
            var expected = new UserDetailResponse
            {
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now),
                FirstName = "Huy",
                LastName = "Phuc",
                Gender = user.Gender,
                DateJoined = DateOnly.FromDateTime(DateTime.Now),
                RoleId = user.RoleId,
                RoleName = "Role",
                LocationName = "Location",
                Username = "",
                StaffCode = ""
            };
            _unitOfWorkMock.Setup(u => u.UserRepository.GetAsync(x => x.Id == userId && x.IsDeleted == false, x => x.Location, x => x.Role))
                .ReturnsAsync(user);
            // Act
            var result = await _userService.GetUserDetailAsync(userId);
            // Assert
            Assert.Equal(JsonConvert.SerializeObject(result), JsonConvert.SerializeObject(expected));
        }

        [Fact]
        public async Task GetUserDetailAsync_UserNotFound_ThrowArgumentException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _unitOfWorkMock.Setup(u => u.UserRepository.GetAsync(x => x.Id == userId && x.IsDeleted == false))!
                .ReturnsAsync((User?)null);
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => await _userService.GetUserDetailAsync(userId));
        }
    }
}
