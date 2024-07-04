//using AssetManagement.Application.Models.Responses;
//using AssetManagement.Application.Services.Implementations;
//using AssetManagement.Application.Services;
//using AssetManagement.Domain.Entities;
//using AssetManagement.Domain.Enums;
//using AssetManagement.Domain.Interfaces;
//using AssetManagement.Infrastructure.Helpers;
//using AssetManagement.Infrastructure.Services;
//using AutoMapper;
//using Microsoft.IdentityModel.Tokens;
//using Moq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Xunit;

//namespace AssetManagement.Test.Unit.AuthServiceTest
//{
//    public class AuthServiceTest
//    {
//        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
//        private readonly Mock<ITokenService> _tokenServiceMock;
//        private readonly Mock<ICryptographyHelper> _cryptographyHelperMock;
//        private readonly Mock<IEmailService> _emailServiceMock;
//        private readonly Mock<IMapper> _mapperMock;
//        private readonly AuthService _authService;

//        public AuthServiceTest()
//        {
//            _unitOfWorkMock = new Mock<IUnitOfWork>();
//            _tokenServiceMock = new Mock<ITokenService>();
//            _cryptographyHelperMock = new Mock<ICryptographyHelper>();
//            _emailServiceMock = new Mock<IEmailService>();
//            _mapperMock = new Mock<IMapper>();
//            _authService = new AuthService(_unitOfWorkMock.Object, _tokenServiceMock.Object, _cryptographyHelperMock.Object, _emailServiceMock.Object, _mapperMock.Object);
//        }

//        [Fact]
//        public async Task LoginAsync_ValidCredentials_ReturnsTokenAndUserResponse()
//        {
//            // Arrange
//            var username = "testuser";
//            var password = "testpassword";
//            var user = new User
//            {
//                Id = Guid.NewGuid(),
//                Username = username,
//                HashPassword = "hashedPassword",
//                SaltPassword = "salt",
//                Status = EnumUserStatus.Active,
//                Role = new Role(),
//                Location = new Location()
//            };
//            var token = "generatedToken";
//            var refreshToken = new RefreshToken { TokenHash = "refreshTokenHash" };
//            var userResponse = new GetUserResponse();

//            var userRepositoryMock = new Mock<IUserRepository>();
//            userRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
//                    It.IsAny<System.Linq.Expressions.Expression<Func<User, object>>[]>()))
//                .ReturnsAsync(user);

//            var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
//            refreshTokenRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<RefreshToken>()))
//                .Returns(Task.CompletedTask);

//            var unitOfWorkMock = new Mock<IUnitOfWork>();
//            unitOfWorkMock.Setup(uow => uow.UserRepository).Returns(userRepositoryMock.Object);
//            unitOfWorkMock.Setup(uow => uow.RefreshTokenRepository).Returns(refreshTokenRepositoryMock.Object);
//            unitOfWorkMock.Setup(uow => uow.CommitAsync()).ReturnsAsync(1);

//            var cryptographyHelperMock = new Mock<ICryptographyHelper>();
//            cryptographyHelperMock.Setup(ch => ch.VerifyPassword(password, user.HashPassword, user.SaltPassword))
//                .Returns(true);

//            var tokenServiceMock = new Mock<ITokenService>();
//            tokenServiceMock.Setup(ts => ts.GenerateToken(user)).Returns(token);
//            tokenServiceMock.Setup(ts => ts.GenerateRefreshToken()).Returns(refreshToken);

//            var mapperMock = new Mock<IMapper>();
//            mapperMock.Setup(m => m.Map<GetUserResponse>(user)).Returns(userResponse);

//            var authService = new AuthService(unitOfWorkMock.Object, tokenServiceMock.Object,
//                cryptographyHelperMock.Object, null, mapperMock.Object);

//            // Act
//            var result = await authService.LoginAsync(username, password);

//            // Assert
//            Assert.Equal(token, result.token);
//            Assert.Equal(refreshToken.TokenHash, result.refreshToken);
//            Assert.Equal(userResponse, result.userResponse);
//            unitOfWorkMock.Verify(uow => uow.RefreshTokenRepository.AddAsync(refreshToken), Times.Once);
//            unitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
//        }

//        [Fact]
//        public async Task LoginAsync_InvalidCredentials_ThrowsUnauthorizedAccessException()
//        {
//            // Arrange
//            var username = "testuser";
//            var password = "testpassword";

//            _unitOfWorkMock.Setup(uow => uow.UserRepository.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<System.Linq.Expressions.Expression<Func<User, object>>[]>()))
//                .ReturnsAsync((User)null);

//            // Act & Assert
//            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(username, password));
//        }

//        [Fact]
//        public async Task LoginAsync_InactiveAccount_ThrowsUnauthorizedAccessException()
//        {
//            // Arrange
//            var username = "testuser";
//            var password = "testpassword";
//            var user = new User
//            {
//                Id = Guid.NewGuid(),
//                Username = username,
//                HashPassword = "hashedPassword",
//                SaltPassword = "salt",
//                Status = EnumUserStatus.Inactive,
//                Role = new Role(),
//                Location = new Location()
//            };

//            _unitOfWorkMock.Setup(uow => uow.UserRepository.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<System.Linq.Expressions.Expression<Func<User, object>>[]>()))
//                .ReturnsAsync(user);
//            _cryptographyHelperMock.Setup(ch => ch.VerifyPassword(password, user.HashPassword, user.SaltPassword))
//                .Returns(true);

//            // Act & Assert
//            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(username, password));
//        }

//        [Fact]
//        public async Task RefreshTokenAsync_ValidRefreshToken_ReturnsNewTokenAndUserResponse()
//        {
//            // Arrange
//            var refreshTokenValue = "refreshTokenValue";
//            var userId = Guid.NewGuid();
//            var refreshToken = new RefreshToken
//            {
//                TokenHash = refreshTokenValue,
//                ExpiredAt = DateTime.UtcNow.AddDays(1),
//                UserId = userId
//            };
//            var user = new User
//            {
//                Id = userId,
//                Role = new Role(),
//                Location = new Location()
//            };
//            var newToken = "newGeneratedToken";
//            var newRefreshToken = new RefreshToken { TokenHash = "newRefreshTokenHash" };
//            var userResponse = new GetUserResponse();

//            _unitOfWorkMock.Setup(uow => uow.RefreshTokenRepository.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
//                .ReturnsAsync(refreshToken);
//            _unitOfWorkMock.Setup(uow => uow.UserRepository.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<System.Linq.Expressions.Expression<Func<User, object>>[]>()))
//                .ReturnsAsync(user);
//            _tokenServiceMock.Setup(ts => ts.GenerateToken(user))
//                .Returns(newToken);
//            _tokenServiceMock.Setup(ts => ts.GenerateRefreshToken())
//                .Returns(newRefreshToken);
//            _mapperMock.Setup(m => m.Map<GetUserResponse>(user))
//                .Returns(userResponse);

//            // Act
//            var result = await _authService.RefreshTokenAsync(refreshTokenValue);

//            // Assert
//            Assert.Equal(newToken, result.token);
//            Assert.Equal(newRefreshToken.TokenHash, result.refreshToken);
//            Assert.Equal(userResponse, result.userResponse);
//            _unitOfWorkMock.Verify(uow => uow.RefreshTokenRepository.Delete(refreshToken), Times.Once);
//            _unitOfWorkMock.Verify(uow => uow.RefreshTokenRepository.AddAsync(newRefreshToken), Times.Once);
//            _unitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
//        }

//        [Fact]
//        public async Task RefreshTokenAsync_InvalidRefreshToken_ThrowsSecurityTokenException()
//        {
//            // Arrange
//            var refreshTokenValue = "invalidRefreshTokenValue";

//            _unitOfWorkMock.Setup(uow => uow.RefreshTokenRepository.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
//                .ReturnsAsync((RefreshToken)null);

//            // Act & Assert
//            await Assert.ThrowsAsync<SecurityTokenException>(() => _authService.RefreshTokenAsync(refreshTokenValue));
//        }

//        [Fact]
//        public async Task RefreshTokenAsync_ExpiredRefreshToken_ThrowsSecurityTokenException()
//        {
//            // Arrange
//            var refreshTokenValue = "expiredRefreshTokenValue";
//            var refreshToken = new RefreshToken
//            {
//                TokenHash = refreshTokenValue,
//                ExpiredAt = DateTime.UtcNow.AddDays(-1)
//            };

//            _unitOfWorkMock.Setup(uow => uow.RefreshTokenRepository.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
//                .ReturnsAsync(refreshToken);

//            // Act & Assert
//            await Assert.ThrowsAsync<SecurityTokenException>(() => _authService.RefreshTokenAsync(refreshTokenValue));
//        }

//        [Fact]
//        public async Task RefreshTokenAsync_UserNotFound_ThrowsKeyNotFoundException()
//        {
//            // Arrange
//            var refreshTokenValue = "refreshTokenValue";
//            var userId = Guid.NewGuid();
//            var refreshToken = new RefreshToken
//            {
//                TokenHash = refreshTokenValue,
//                ExpiredAt = DateTime.UtcNow.AddDays(1),
//                UserId = userId
//            };

//            _unitOfWorkMock.Setup(uow => uow.RefreshTokenRepository.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
//                .ReturnsAsync(refreshToken);
//            _unitOfWorkMock.Setup(uow => uow.UserRepository.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<System.Linq.Expressions.Expression<Func<User, object>>[]>()))
//                .ReturnsAsync((User)null);

//            // Act & Assert
//            await Assert.ThrowsAsync<KeyNotFoundException>(() => _authService.RefreshTokenAsync(refreshTokenValue));
//        }

//        [Fact]
//        public async Task ResetPasswordAsync_ValidUserName_ReturnsNumberOfChanges()
//        {
//            // Arrange
//            var userName = "testuser";
//            var newPassword = "newPassword";
//            var user = new User { Username = userName };
//            var passwordSalt = "generatedSalt";
//            var passwordHash = "generatedHash";
//            var numberOfChanges = 1;

//            _unitOfWorkMock.Setup(uow => uow.UserRepository.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
//                .ReturnsAsync(user);
//            _cryptographyHelperMock.Setup(ch => ch.GenerateSalt())
//                .Returns(passwordSalt);
//            _cryptographyHelperMock.Setup(ch => ch.HashPassword(newPassword, passwordSalt))
//                .Returns(passwordHash);
//            _unitOfWorkMock.Setup(uow => uow.CommitAsync())
//                .ReturnsAsync(numberOfChanges);

//            // Act
//            var result = await _authService.ResetPasswordAsync(userName, newPassword);

//            // Assert
//            Assert.Equal(numberOfChanges, result);
//            _unitOfWorkMock.Verify(uow => uow.UserRepository.Update(It.Is<User>(u => u.HashPassword == passwordHash && u.SaltPassword == passwordSalt && !u.IsFirstLogin)), Times.Once);
//            _unitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
//        }

//        [Fact]
//        public async Task ResetPasswordAsync_InvalidUserName_ThrowsKeyNotFoundException()
//        {
//            // Arrange
//            var userName = "invaliduser";
//            var newPassword = "newPassword";

//            _unitOfWorkMock.Setup(uow => uow.UserRepository.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
//                .ReturnsAsync((User)null);

//            // Act & Assert
//            await Assert.ThrowsAsync<KeyNotFoundException>(() => _authService.ResetPasswordAsync(userName, newPassword));
//        }

//        [Fact]
//        public async Task ChangePasswordAsync_ValidCredentials_ReturnsNumberOfChanges()
//        {
//            // Arrange
//            var userName = "testuser";
//            var oldPassword = "oldPassword";
//            var newPassword = "newPassword";
//            var user = new User
//            {
//                Username = userName,
//                HashPassword = "hashedPassword",
//                SaltPassword = "salt"
//            };
//            var passwordSalt = "generatedSalt";
//            var passwordHash = "generatedHash";
//            var numberOfChanges = 1;

//            _unitOfWorkMock.Setup(uow => uow.UserRepository.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
//                .ReturnsAsync(user);
//            _cryptographyHelperMock.Setup(ch => ch.VerifyPassword(oldPassword, user.HashPassword, user.SaltPassword))
//                .Returns(true);
//            _cryptographyHelperMock.Setup(ch => ch.GenerateSalt())
//                .Returns(passwordSalt);
//            _cryptographyHelperMock.Setup(ch => ch.HashPassword(newPassword, passwordSalt))
//                .Returns(passwordHash);
//            _unitOfWorkMock.Setup(uow => uow.CommitAsync())
//                .ReturnsAsync(numberOfChanges);

//            // Act
//            var result = await _authService.ChangePasswordAsync(userName, oldPassword, newPassword);

//            // Assert
//            Assert.Equal(numberOfChanges, result);
//            _unitOfWorkMock.Verify(uow => uow.UserRepository.Update(It.Is<User>(u => u.HashPassword == passwordHash && u.SaltPassword == passwordSalt && !u.IsFirstLogin)), Times.Once);
//            _unitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
//        }

//        [Fact]
//        public async Task ChangePasswordAsync_InvalidUserName_ThrowsKeyNotFoundException()
//        {
//            // Arrange
//            var userName = "invaliduser";
//            var oldPassword = "oldPassword";
//            var newPassword = "newPassword";

//            _unitOfWorkMock.Setup(uow => uow.UserRepository.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
//                .ReturnsAsync((User)null);

//            // Act & Assert
//            await Assert.ThrowsAsync<KeyNotFoundException>(() => _authService.ChangePasswordAsync(userName, oldPassword, newPassword));
//        }

//        [Fact]
//        public async Task ChangePasswordAsync_IncorrectOldPassword_ThrowsUnauthorizedAccessException()
//        {
//            // Arrange
//            var userName = "testuser";
//            var oldPassword = "incorrectPassword";
//            var newPassword = "newPassword";
//            var user = new User
//            {
//                Username = userName,
//                HashPassword = "hashedPassword",
//                SaltPassword = "salt"
//            };

//            _unitOfWorkMock.Setup(uow => uow.UserRepository.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
//                .ReturnsAsync(user);
//            _cryptographyHelperMock.Setup(ch => ch.VerifyPassword(oldPassword, user.HashPassword, user.SaltPassword))
//                .Returns(false);

//            // Act & Assert
//            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.ChangePasswordAsync(userName, oldPassword, newPassword));
//        }

//        [Fact]
//        public async Task LogoutAsync_ValidUserId_ReturnsNumberOfChanges()
//        {
//            // Arrange
//            var userId = Guid.NewGuid();
//            var refreshTokens = new List<RefreshToken>
//    {
//        new RefreshToken { UserId = userId },
//        new RefreshToken { UserId = userId }
//    };
//            var numberOfChanges = 2;

//            _unitOfWorkMock.Setup(uow => uow.RefreshTokenRepository.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
//                .ReturnsAsync(refreshTokens);
//            _unitOfWorkMock.Setup(uow => uow.CommitAsync())
//                .ReturnsAsync(numberOfChanges);

//            // Act
//            var result = await _authService.LogoutAsync(userId);

//            // Assert
//            Assert.Equal(numberOfChanges, result);
//            _unitOfWorkMock.Verify(uow => uow.RefreshTokenRepository.RemoveRange(refreshTokens), Times.Once);
//            _unitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
//        }
//    }
//}
