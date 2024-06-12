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
        public async Task AddUserAsync_ValidRequest_ReturnsUserRegisterResponse()
        {
            // Arrange
            var userRegisterRequest = new UserRegisterRequest
            {
                FirstName = "John",
                LastName = "Doe",
                Gender = EnumGender.Male,
                DateOfBirth = new DateOnly(1990, 1, 1),
                DateJoined = new DateOnly(2023, 1, 1),
                RoleId = Guid.NewGuid(),
                CreateBy = Guid.NewGuid()
            };

            var adminUser = new User { LocationId = Guid.NewGuid() };
            var role = new Role();

            _unitOfWorkMock.Setup(u => u.UserRepository.GetAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<Expression<Func<User, object>>[]>()))
    .ReturnsAsync((User)null);
            _unitOfWorkMock.Setup(u => u.UserRepository.GetAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<Expression<Func<User, object>>[]>()))
                .ReturnsAsync(adminUser);
            _unitOfWorkMock.Setup(u => u.RoleRepository.GetAsync(It.IsAny<Expression<Func<Role, bool>>>()))
                .ReturnsAsync(role);
            _unitOfWorkMock.Setup(u => u.CommitAsync())
                .ReturnsAsync(1);

            _helperMock.Setup(h => h.GetUsername(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("johndoe");

            _cryptographyHelperMock.Setup(c => c.GenerateSalt())
                .Returns("salt");
            _cryptographyHelperMock.Setup(c => c.HashPassword(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("hashedPassword");

            // Act
            var result = await _userService.AddUserAsync(userRegisterRequest);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<UserRegisterResponse>(result);
            _unitOfWorkMock.Verify(u => u.UserRepository.AddAsync(It.IsAny<User>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }
    }
}
