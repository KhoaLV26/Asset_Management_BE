using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Application.Services;
using AssetManagement.Domain.Models;
using AssetManagement.Infrastructure.Services;
using AssetManagement.WebAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AssetManagement.Test.Unit.AuthControllerTest
{
    public class AuthControllerTest
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly AuthController _controller;

        public AuthControllerTest()
        {
            _authServiceMock = new Mock<IAuthService>();
            _emailServiceMock = new Mock<IEmailService>();
            _controller = new AuthController(_authServiceMock.Object, _emailServiceMock.Object);
        }


        [Fact]
        public async Task LoginAsync_InvalidCredentials_ReturnsConflict()
        {
            // Arrange
            var request = new UserLoginRequest
            {
                Username = "testuser",
                Password = "testpassword"
            };
            var exceptionMessage = "Invalid credentials";

            _authServiceMock.Setup(a => a.LoginAsync(request.Username, request.Password))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.LoginAsync(request);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Message);
        }
        
        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsOk()
        {
            // Arrange
            var request = new UserLoginRequest
            {
                Username = "testuser",
                Password = "testpassword"
            };
            var jwtToken = "jwtToken";
            var tokenHash = "tokenHash";
            var refreshToken = "refreshToken";
            var getUserResponse = new GetUserResponse
            {

            };
            var responses = (tokenHash, refreshToken, getUserResponse);

            _authServiceMock.Setup(a => a.LoginAsync(request.Username, request.Password))
                .ReturnsAsync(responses);

            // Act
            var result = await _controller.LoginAsync(request);

            // Assert
            var conflictResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralGetResponse>(conflictResult.Value);
            Assert.True(response.Success);
            Assert.Equal(response.Message, "User logged in successfully");
            
        }

        [Fact]
        public async Task ResetPasswordAsync_ValidRequest_ReturnsOkResult()
        {
            var userName = "userName";
            var password = "password";
            var refreshToken = "refreshToken";
            // Arrange
            var request = new UserResetPasswordRequest
            {
                Username = userName,
                Password = password,
                RefreshToken = refreshToken
            };
            
            _authServiceMock.Setup(a => a.ResetPasswordAsync(request.Username, request.Password, request.RefreshToken))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.ResetPasswordAsync(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(okResult.Value);
            Assert.Equal("Password reset successfully", response.Message);
        }

        [Fact]
        public async Task ResetPasswordAsync_InvalidRequest_ReturnsConflict()
        {
            var userName = "userName";
            var password = "password";
            var refreshToken = "refreshToken";
            // Arrange
            var request = new UserResetPasswordRequest
            {
                Username = userName,
                Password = password,
                RefreshToken = refreshToken
            };
            var exceptionMessage = "Invalid request";

            _authServiceMock.Setup(a => a.ResetPasswordAsync(request.Username, request.Password, request.RefreshToken))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.ResetPasswordAsync(request);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Message);
        }

        [Fact]
        public async Task ChangePasswordAsync_ValidRequest_ReturnsOkResult()
        {
            var userName = "userName";
            var newPassword = "newPassword";
            var oldPassword = "oldPassword";
            var refreshToken = "refreshToken";
            var currentToken = "currentToken";
            // Arrange
            var request = new ChangePasswordRequest
            {
                Username = userName,
                NewPassword = newPassword,
                OldPassword = oldPassword,
                RefreshToken = refreshToken
            };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        Headers =
                        {
                            Authorization = currentToken
                        }
                    }
                }
            };
            _authServiceMock.Setup(a => a.ChangePasswordAsync(request.Username, request.OldPassword, request.NewPassword, request.RefreshToken, currentToken))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.ChangePasswordAsync(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(okResult.Value);
            Assert.Equal("Password changed successfully", response.Message);
        }

        [Fact]
        public async Task ChangePasswordAsync_InvalidRequest_ReturnsConflict()
        {
            // Arrange
            var userName = "userName";
            var newPassword = "newPassword";
            var oldPassword = "oldPassword";
            var refreshToken = "refreshToken";
            var currentToken = "currentToken";
            var exceptionMessage = "";

            // Arrange
            var request = new ChangePasswordRequest
            {
                Username = userName,
                NewPassword = newPassword,
                OldPassword = oldPassword,
                RefreshToken = refreshToken
            };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        Headers =
                        {
                            Authorization = currentToken
                        }
                    }
                }
            };

            _authServiceMock.Setup(a => a.ChangePasswordAsync(request.Username, request.OldPassword, request.NewPassword, request.RefreshToken, currentToken))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.ChangePasswordAsync(request);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Message);
        }

        [Fact]
        public async Task RefreshTokenAsync_InvalidRequest_ReturnsConflict()
        {
            // Arrange
            var request = new RefreshTokenRequest
            {
                RefreshToken = "invalidRefreshToken"
            };
            var exceptionMessage = "Invalid refresh token";

            _authServiceMock.Setup(a => a.RefreshTokenAsync(request.RefreshToken))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.RefreshTokenAsync(request);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Message);
        }
        
        [Fact]
        public async Task RefreshTokenAsync_ValidRequest_ReturnsOkResponse()
        {
            var jwtToken = "jwtToken";
            var tokenHash = "tokenHash";
            var getUserResponse = new GetUserResponse
            {

            };
            // Arrange
            var request = new RefreshTokenRequest
            {
                RefreshToken = "invalidRefreshToken"
            };
            var exceptionMessage = "Valid refresh token";
            var responses = (It.IsAny<string>(), request.RefreshToken, getUserResponse);
            _authServiceMock.Setup(a => a.RefreshTokenAsync(request.RefreshToken))
                .ReturnsAsync(responses);

            // Act
            var result = await _controller.RefreshTokenAsync(request);

            // Assert
            var conflictResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralGetResponse>(conflictResult.Value);
            Assert.True(response.Success);
        }
    }
}
