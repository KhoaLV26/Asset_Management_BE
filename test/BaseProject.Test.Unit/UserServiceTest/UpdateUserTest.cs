using System;
using System.Threading.Tasks;
using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Application.Services.Implementations;
using AssetManagement.Domain.Constants;
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

        //[Fact]
        //public async Task UpdateUserAsync_PassAllConstraint_UpdateToDatabaseAndReturnsUpdateUserResponse()
        //{
        //    // Arrange
        //    var userId = Guid.NewGuid();
        //    var roleId = Guid.NewGuid();
        //    var role = new Role
        //    {
        //        Id = roleId,
        //        Name = RoleConstant.STAFF
        //    };
        //    var user = new User
        //    {
        //        Id = userId,
        //        StaffCode = "SD0001",
        //        DateOfBirth = DateOnly.FromDateTime(DateTime.Now),
        //        FirstName = "Huy",
        //        LastName = "Phuc",
        //        Gender = EnumGender.Male,
        //        DateJoined = DateOnly.FromDateTime(DateTime.Now),
        //        RoleId = roleId,
        //        IsDeleted = false,
        //        Role = role
        //    };
        //    var editUserRequest = new EditUserRequest
        //    {
        //        DateJoined = DateOnly.FromDateTime(DateTime.Now),
        //        DateOfBirth = DateOnly.FromDateTime(DateTime.Now),
        //        Gender = EnumGender.Male,
        //        RoleId = roleId
        //    };
        //    var expected = new UpdateUserResponse
        //    {
        //        StaffCode = "SD0001"
        //    };
        //    _unitOfWorkMock.Setup(x => x.UserRepository.GetAsync(a => a.IsDeleted == false &&  a.Id == userId, x=> x.Role))
        //        .ReturnsAsync(user);
        //    _unitOfWorkMock.Setup(x => x.RoleRepository.GetAsync(a => a.IsDeleted == false &&  a.Id == editUserRequest.RoleId))
        //        .ReturnsAsync(role);
        //    _unitOfWorkMock.Setup(u => u.UserRepository.Update(user))
        //        .Verifiable();
        //    _unitOfWorkMock.Setup(u => u.CommitAsync())
        //        .ReturnsAsync(1).Verifiable();

        //    // Act
        //    var result = await _userService.UpdateUserAsync(userId, editUserRequest);
        //    // Assert
        //    Assert.Equal(expected.StaffCode, result.StaffCode);
        //    _unitOfWorkMock.Verify(u => u.UserRepository.Update(user), Times.Once);
        //    _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        //}

        //[Fact]
        //public async Task UpdateUserAsync_AdminUpdateToStaff_ReturnsUpdateUserResponse()
        //{
        //    // Arrange
        //    var userId = Guid.NewGuid();
        //    var roleId = Guid.NewGuid();
        //    var expectedMessage = "Cannot edit role from admin to staff.";
        //    var role = new Role
        //    {
        //        Id = roleId,
        //        Name = RoleConstant.STAFF
        //    };
        //    var user = new User
        //    {
        //        Id = userId,
        //        StaffCode = "SD0001",
        //        DateOfBirth = DateOnly.FromDateTime(DateTime.Now),
        //        FirstName = "Huy",
        //        LastName = "Phuc",
        //        Gender = EnumGender.Male,
        //        DateJoined = DateOnly.FromDateTime(DateTime.Now),
        //        RoleId = roleId,
        //        IsDeleted = false,
        //        Role = new Role
        //        {
        //            Id = new Guid(),
        //            Name = RoleConstant.ADMIN
        //        }
        //    };
        //    var editUserRequest = new EditUserRequest
        //    {
        //        DateJoined = DateOnly.FromDateTime(DateTime.Now),
        //        DateOfBirth = DateOnly.FromDateTime(DateTime.Now),
        //        Gender = EnumGender.Male,
        //        RoleId = roleId
        //    };

        //    _unitOfWorkMock.Setup(x => x.UserRepository.GetAsync(a => a.IsDeleted == false && a.Id == userId, x => x.Role))
        //        .ReturnsAsync(user);
        //    _unitOfWorkMock.Setup(x => x.RoleRepository.GetAsync(a => a.IsDeleted == false && a.Id == editUserRequest.RoleId))
        //        .ReturnsAsync(role);

        //    // Act
        //    var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await _userService.UpdateUserAsync(userId, editUserRequest));

        //    // Assert
        //    Assert.Equal(expectedMessage, exception.Message);
        //}

        //[Fact]
        //public async Task UpdateUserAsync_RoleNotFound_ThrowArgumentExceptionWithMessage()
        //{
        //    // Arrange
        //    var userId = Guid.NewGuid();
        //    var roleId = Guid.NewGuid();
        //    var expectedMessage = "Role not found.";

        //    var user = new User
        //    {
        //        Id = userId,
        //        StaffCode = "SD0001",
        //        DateOfBirth = DateOnly.FromDateTime(DateTime.Now),
        //        FirstName = "Huy",
        //        LastName = "Phuc",
        //        Gender = EnumGender.Male,
        //        DateJoined = DateOnly.FromDateTime(DateTime.Now),
        //        RoleId = roleId,
        //        IsDeleted = false,
        //    };
        //    var editUserRequest = new EditUserRequest
        //    {
        //        DateJoined = DateOnly.FromDateTime(DateTime.Now),
        //        DateOfBirth = DateOnly.FromDateTime(DateTime.Now),
        //        Gender = EnumGender.Male,
        //        RoleId = roleId
        //    };

        //    _unitOfWorkMock.Setup(x => x.UserRepository.GetAsync(a => a.IsDeleted == false &&  a.Id == userId, x=> x.Role))
        //        .ReturnsAsync(user);
        //    _unitOfWorkMock.Setup(x => x.RoleRepository.GetAsync(a => a.IsDeleted == false &&  a.Id == editUserRequest.RoleId))
        //        .ReturnsAsync((Role?)null);

        //    // Act
        //    var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await _userService.UpdateUserAsync(userId, editUserRequest));

        //    // Assert
        //    Assert.Equal(expectedMessage, exception.Message);
        //}

        //[Fact]
        //public async Task UpdateUserAsync_UserNotFound_ThrowArgumentExceptionWithMessage()
        //{
        //    // Arrange
        //    var userId = Guid.NewGuid();
        //    var expectedMessage = "User not found.";
        //    _unitOfWorkMock.Setup(x => x.UserRepository.GetAsync(a => a.IsDeleted == false && a.Id == userId))!
        //        .ReturnsAsync((User?)null);

        //    // Act
        //    var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await _userService.UpdateUserAsync(userId,It.IsAny<EditUserRequest>()));

        //    // Assert
        //    Assert.Equal(expectedMessage, exception.Message);
        //}
    }
}