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
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace AssetManagement.Test.Unit.UserServiceTest
{
    [ExcludeFromCodeCoverage]
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
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-30)),
                DateJoined = DateOnly.FromDateTime(DateTime.Parse("2021-07-12")),
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

        [Fact]
        public async Task AddUserAsync_UserUnder18_ThrowsArgumentException()
        {
            // Arrange
            var userRegisterRequest = new UserRegisterRequest
            {
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-17)), // User is 17 years old
                DateJoined = DateOnly.FromDateTime(DateTime.Now),
                Gender = EnumGender.Male,
                RoleId = Guid.NewGuid(),
                CreateBy = Guid.NewGuid()
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _userService.AddUserAsync(userRegisterRequest));
        }

        [Fact]
        public async Task AddUserAsync_JoinedDateBeforeDateOfBirth_ThrowsArgumentException()
        {
            // Arrange
            var userRegisterRequest = new UserRegisterRequest
            {
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-20)),
                DateJoined = DateOnly.FromDateTime(DateTime.Now.AddYears(-21)), // Joined date is before date of birth
                Gender = EnumGender.Male,
                RoleId = Guid.NewGuid(),
                CreateBy = Guid.NewGuid()
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _userService.AddUserAsync(userRegisterRequest));
        }

        [Theory]
        [InlineData(DayOfWeek.Saturday)]
        [InlineData(DayOfWeek.Sunday)]
        public async Task AddUserAsync_JoinedDateOnWeekend_ThrowsArgumentException(DayOfWeek dayOfWeek)
        {
            // Arrange
            var userRegisterRequest = new UserRegisterRequest
            {
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-20)),
                DateJoined = DateOnly.FromDateTime(GetNextWeekday(DateTime.Now, dayOfWeek)), // Joined date is on the specified weekend day
                Gender = EnumGender.Male,
                RoleId = Guid.NewGuid(),
                CreateBy = Guid.NewGuid()
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _userService.AddUserAsync(userRegisterRequest));
        }

        private DateTime GetNextWeekday(DateTime startDate, DayOfWeek dayOfWeek)
        {
            int daysToAdd = ((int)dayOfWeek - (int)startDate.DayOfWeek + 7) % 7;
            return startDate.AddDays(daysToAdd);
        }
        [Fact]
        public async Task AddUserAsync_CommitFails_ThrowsInvalidOperationException()
        {
            // Arrange
            var userRegisterRequest = new UserRegisterRequest
            {
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-30)),
                DateJoined = DateOnly.FromDateTime(DateTime.Parse("2021-07-12")),
                Gender = EnumGender.Male,
                RoleId = Guid.NewGuid(),
                CreateBy = Guid.NewGuid()
            };

            _unitOfWorkMock.Setup(u => u.UserRepository.GetAllAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(new User[0]);
            _unitOfWorkMock.Setup(u => u.UserRepository.GetAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<Expression<Func<User, object>>>()))
                .ReturnsAsync(new User { Id = Guid.NewGuid(), LocationId = Guid.NewGuid() });
            _unitOfWorkMock.Setup(u => u.RoleRepository.GetAsync(It.IsAny<Expression<Func<Role, bool>>>()))
                .ReturnsAsync(new Role { Id = Guid.NewGuid() });
            _unitOfWorkMock.Setup(u => u.UserRepository.AddAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CommitAsync())
                .ReturnsAsync(0);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.AddUserAsync(userRegisterRequest));
            _unitOfWorkMock.Verify(u => u.UserRepository.AddAsync(It.IsAny<User>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task AddUserAsync_UsernameExists_AppendsUniqueNumber()
        {
            // Arrange
            var userRegisterRequest = new UserRegisterRequest
            {
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-30)),
                DateJoined = DateOnly.FromDateTime(DateTime.Parse("2021-07-12")),
                Gender = EnumGender.Male,
                RoleId = Guid.NewGuid(),
                CreateBy = Guid.NewGuid()
            };

            var existingUsers = new[]
            {
                new User { Username = "johnd" },
                new User { Username = "johnd1" }
            };

            var adminUser = new User { Id = userRegisterRequest.CreateBy, LocationId = Guid.NewGuid() };
            var role = new Role { Id = userRegisterRequest.RoleId };

            _unitOfWorkMock.Setup(u => u.UserRepository.GetAllAsync())
                .ReturnsAsync(existingUsers);
            _unitOfWorkMock.Setup(u => u.UserRepository.GetAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<Expression<Func<User, object>>>()))
                .ReturnsAsync(adminUser);
            _unitOfWorkMock.Setup(u => u.RoleRepository.GetAsync(It.IsAny<Expression<Func<Role, bool>>>()))
                .ReturnsAsync(role);
            _unitOfWorkMock.Setup(u => u.UserRepository.AddAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CommitAsync())
                .ReturnsAsync(1);

            _helperMock.Setup(h => h.GetUsername(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("johnd");

            _mapperMock.Setup(m => m.Map<UserRegisterResponse>(It.IsAny<User>()))
                .Returns((User user) => new UserRegisterResponse { Username = user.Username });

            // Act
            var result = await _userService.AddUserAsync(userRegisterRequest);

            // Assert
            Assert.Equal("johnd2", result.Username);
            _unitOfWorkMock.Verify(u => u.UserRepository.AddAsync(It.Is<User>(u => u.Username == "johnd2")), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }
    }
}
