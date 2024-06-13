using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Application.Services.Implementations;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Domain.Interfaces;
using AssetManagement.Infrastructure.Helpers;
using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AssetManagement.Test.Unit.UserServiceTest
{
    public class AddUserServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICryptographyHelper> _cryptographyHelperMock;
        private readonly Mock<IHelper> _helperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly UserService _userService;

        public AddUserServiceTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _cryptographyHelperMock = new Mock<ICryptographyHelper>();
            _helperMock = new Mock<IHelper>();
            _mapperMock = new Mock<IMapper>();
            _userService = new UserService(_unitOfWorkMock.Object, _cryptographyHelperMock.Object, _helperMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task AddUserAsync_ValidRequest_ShouldAddUser()
        {
            // Arrange
            var userRegisterRequest = new UserRegisterRequest
            {
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-20)),
                DateJoined = DateOnly.FromDateTime(DateTime.Now),
                Gender = EnumGender.Male,
                RoleId = Guid.NewGuid(),
                CreateBy = Guid.NewGuid()
            };

            var expectedResponse = new UserRegisterResponse
            {
                StaffCode = "SD0001",
                Username = "johnd"
            };

            // Use Expression<Func<User, bool>> instead of Func<User, bool>
            _unitOfWorkMock.Setup(u => u.UserRepository.GetAllAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(new User[0]);

            // Use Expression<Func<User, object>> instead of Func<User, object>
            _unitOfWorkMock.Setup(u => u.UserRepository.GetAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<Expression<Func<User, object>>>()))
                .ReturnsAsync(new User { Id = Guid.NewGuid(), LocationId = Guid.NewGuid() });

            // Use Expression<Func<Role, bool>> instead of Func<Role, bool>
            _unitOfWorkMock.Setup(u => u.RoleRepository.GetAsync(It.IsAny<Expression<Func<Role, bool>>>()))
                .ReturnsAsync(new Role { Id = Guid.NewGuid() });
            _unitOfWorkMock.Setup(u => u.UserRepository.AddAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CommitAsync())
                .ReturnsAsync(1);
            _mapperMock.Setup(m => m.Map<UserRegisterResponse>(It.IsAny<User>()))
                .Returns(expectedResponse);

            // Act
            var result = await _userService.AddUserAsync(userRegisterRequest);

            // Assert
            Assert.Equal(expectedResponse.StaffCode, result.StaffCode);
            Assert.Equal(expectedResponse.Username, result.Username);
            _unitOfWorkMock.Verify(u => u.UserRepository.AddAsync(It.IsAny<User>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        // Add more test methods for other scenarios, such as invalid requests, edge cases, etc.
    }
}
