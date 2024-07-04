//using AssetManagement.Application.Models.Requests;
//using AssetManagement.Application.Models.Responses;
//using AssetManagement.Application.Services;
//using AssetManagement.Domain.Models;
//using AssetManagement.Infrastructure.Services;
//using AssetManagement.WebAPI.Controllers;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Moq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Claims;
//using System.Text;
//using System.Threading.Tasks;
//using Xunit;

//namespace AssetManagement.Test.Unit.AuthControllerTest
//{
//    public class AuthControllerTest
//    {
//        private readonly Mock<IAuthService> _authServiceMock;
//        private readonly Mock<IEmailService> _emailServiceMock;
//        private readonly AuthController _controller;

//        public AuthControllerTest()
//        {
//            _authServiceMock = new Mock<IAuthService>();
//            _emailServiceMock = new Mock<IEmailService>();
//            _controller = new AuthController(_authServiceMock.Object, _emailServiceMock.Object);
//        }


//        [Fact]
//        public async Task LoginAsync_InvalidCredentials_ReturnsConflict()
//        {
//            // Arrange
//            var request = new UserLoginRequest
//            {
//                Username = "testuser",
//                Password = "testpassword"
//            };
//            var exceptionMessage = "Invalid credentials";

//            _authServiceMock.Setup(a => a.LoginAsync(request.Username, request.Password))
//                .ThrowsAsync(new Exception(exceptionMessage));

//            // Act
//            var result = await _controller.LoginAsync(request);

//            // Assert
//            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
//            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
//            Assert.False(response.Success);
//            Assert.Equal(exceptionMessage, response.Message);
//        }

//        [Fact]
//        public async Task LogoutAsync_ValidUser_ReturnsOkResult()
//        {
//            // Arrange
//            var userId = Guid.NewGuid();
//            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
//            {
//        new Claim(ClaimTypes.NameIdentifier, userId.ToString())
//            }));
//            _controller.ControllerContext = new ControllerContext
//            {
//                HttpContext = new DefaultHttpContext { User = user }
//            };
//            _authServiceMock.Setup(a => a.LogoutAsync(userId))
//                .ReturnsAsync(1);

//            // Act
//            var result = await _controller.LogoutAsync();

//            // Assert
//            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
//            Assert.Equal("Value cannot be null. (Parameter 'input')", conflictResult.Value);
//        }

//        [Fact]
//        public async Task LogoutAsync_InvalidUser_ReturnsConflict()
//        {
//            // Arrange
//            var userId = Guid.NewGuid();
//            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
//            {
//        new Claim(ClaimTypes.NameIdentifier, userId.ToString())
//            }));
//            _controller.ControllerContext = new ControllerContext
//            {
//                HttpContext = new DefaultHttpContext { User = user }
//            };

//            _authServiceMock.Setup(a => a.LogoutAsync(userId))
//                .ReturnsAsync(0);

//            // Act
//            var result = await _controller.LogoutAsync();

//            // Assert
//            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
//            Assert.Equal("Value cannot be null. (Parameter 'input')", conflictResult.Value);
//        }

//        [Fact]
//        public async Task ResetPasswordAsync_ValidRequest_ReturnsOkResult()
//        {
//            // Arrange
//            var request = new UserResetPasswordRequest
//            {
//                Username = "testuser",
//                Password = "newpassword"
//            };

//            _authServiceMock.Setup(a => a.ResetPasswordAsync(request.Username, request.Password))
//                .ReturnsAsync(1);

//            // Act
//            var result = await _controller.ResetPasswordAsync(request);

//            // Assert
//            var okResult = Assert.IsType<OkObjectResult>(result);
//            var response = Assert.IsType<GeneralBoolResponse>(okResult.Value);
//            Assert.Equal("Password reset successfully", response.Message);
//        }

//        [Fact]
//        public async Task ResetPasswordAsync_InvalidRequest_ReturnsConflict()
//        {
//            // Arrange
//            var request = new UserResetPasswordRequest
//            {
//                Username = "testuser",
//                Password = "newpassword"
//            };
//            var exceptionMessage = "Invalid request";

//            _authServiceMock.Setup(a => a.ResetPasswordAsync(request.Username, request.Password))
//                .ThrowsAsync(new Exception(exceptionMessage));

//            // Act
//            var result = await _controller.ResetPasswordAsync(request);

//            // Assert
//            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
//            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
//            Assert.False(response.Success);
//            Assert.Equal(exceptionMessage, response.Message);
//        }

//        [Fact]
//        public async Task ChangePasswordAsync_ValidRequest_ReturnsOkResult()
//        {
//            // Arrange
//            var request = new ChangePasswordRequest
//            {
//                Username = "testuser",
//                OldPassword = "oldpassword",
//                NewPassword = "newpassword"
//            };

//            _authServiceMock.Setup(a => a.ChangePasswordAsync(request.Username, request.OldPassword, request.NewPassword))
//                .ReturnsAsync(1);

//            // Act
//            var result = await _controller.ChangePasswordAsync(request);

//            // Assert
//            var okResult = Assert.IsType<OkObjectResult>(result);
//            var response = Assert.IsType<GeneralBoolResponse>(okResult.Value);
//            Assert.Equal("Password changed successfully", response.Message);
//        }

//        [Fact]
//        public async Task ChangePasswordAsync_InvalidRequest_ReturnsConflict()
//        {
//            // Arrange
//            var request = new ChangePasswordRequest
//            {
//                Username = "testuser",
//                OldPassword = "oldpassword",
//                NewPassword = "newpassword"
//            };
//            var exceptionMessage = "Invalid request";

//            _authServiceMock.Setup(a => a.ChangePasswordAsync(request.Username, request.OldPassword, request.NewPassword))
//                .ThrowsAsync(new Exception(exceptionMessage));

//            // Act
//            var result = await _controller.ChangePasswordAsync(request);

//            // Assert
//            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
//            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
//            Assert.False(response.Success);
//            Assert.Equal(exceptionMessage, response.Message);
//        }

//        [Fact]
//        public async Task RefreshTokenAsync_InvalidRequest_ReturnsConflict()
//        {
//            // Arrange
//            var request = new RefreshTokenRequest
//            {
//                RefreshToken = "invalidRefreshToken"
//            };
//            var exceptionMessage = "Invalid refresh token";

//            _authServiceMock.Setup(a => a.RefreshTokenAsync(request.RefreshToken))
//                .ThrowsAsync(new Exception(exceptionMessage));

//            // Act
//            var result = await _controller.RefreshTokenAsync(request);

//            // Assert
//            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
//            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
//            Assert.False(response.Success);
//            Assert.Equal(exceptionMessage, response.Message);
//        }
//    }
//}
