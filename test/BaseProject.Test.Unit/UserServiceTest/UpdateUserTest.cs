using System;
using System.Threading.Tasks;
using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Application.Services.Implementations;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Domain.Interfaces;
using AssetManagement.Infrastructure.Helpers;
using AutoMapper;
using Moq;
using Xunit;

namespace AssetManagement.Test.Unit.UserServiceTest
{
    public class UpdateUserTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICryptographyHelper> _cryptographyHelperMock;
        private readonly Mock<IHelper> _helperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly UserService _userService;

        public UpdateUserTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _cryptographyHelperMock = new Mock<ICryptographyHelper>();
            _helperMock = new Mock<IHelper>();
            _mapperMock = new Mock<IMapper>();
            _userService = new UserService(_unitOfWorkMock.Object, _cryptographyHelperMock.Object, _helperMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task UpdateUserAsync_UserFound_ReturnsCreatedAssetResponse()
        {
            var userId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                StaffCode = "SD0001",
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now),
                FirstName = "Huy",
                LastName = "Phuc",
                Gender = EnumGender.Male,
                DateJoined = DateOnly.FromDateTime(DateTime.Now),
                RoleId = roleId,
                IsDeleted = false
            };
            var editUserRequest = new EditUserRequest
            {
                FirstName = "Huy",
                LastName = "Phuc",
                DateJoined = DateOnly.FromDateTime(DateTime.Now),
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now),
                Gender = EnumGender.Male,
                RoleId = roleId
            };
            var expected = new UpdateUserResponse
            {
                StaffCode = "SD0001"
            };
            _unitOfWorkMock.Setup(x => x.UserRepository.GetAsync(a => a.IsDeleted == false &&  a.Id == userId))
                .ReturnsAsync(user);
            _unitOfWorkMock.Setup(u => u.UserRepository.Update(It.IsAny<User>()))
                .Verifiable();
            _unitOfWorkMock.Setup(u => u.CommitAsync())
                .ReturnsAsync(1).Verifiable();

            // Act
            var result = await _userService.UpdateUserAsync(userId, editUserRequest);
            // Assert
            Assert.Equal(expected.StaffCode, result.StaffCode);
            _unitOfWorkMock.Verify(u => u.UserRepository.Update(user), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_UserNotFound_ThrowArgumentException()
        {
            var userId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            _unitOfWorkMock.Setup(x => x.UserRepository.GetAsync(a => a.IsDeleted == false && a.Id == userId))!
                .ReturnsAsync((User?)null);
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => await _userService.UpdateUserAsync(userId,It.IsAny<EditUserRequest>()));
        }
    }
}
