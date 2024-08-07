﻿using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Application.Services;
using AssetManagement.Domain.Enums;
using AssetManagement.Domain.Models;
using AssetManagement.WebAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace AssetManagement.Test.Unit.UserControllerTest
{
    [ExcludeFromCodeCoverage]
    public class UserControllerTest
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly UsersController _controller;
        private readonly string adminId = Guid.NewGuid().ToString();

        public UserControllerTest()
        {
            _userServiceMock = new Mock<IUserService>();
            _controller = new UsersController(_userServiceMock.Object);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Actor, adminId)
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = principal
                }
            };
        }

        [Fact]
        public async Task RegisterUser_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var request = new UserRegisterRequest
            {
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateOnly(1990, 1, 1),
                DateJoined = new DateOnly(2022, 1, 1),
                RoleId = Guid.NewGuid(),
                CreateBy = Guid.NewGuid()
            };

            var userResponse = new UserRegisterResponse
            {
                StaffCode = "SD0001",
                Username = "johndoe"
            };

            _userServiceMock.Setup(s => s.AddUserAsync(request))
                .ReturnsAsync(userResponse);

            // Act
            var result = await _controller.RegisterUser(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralCreateResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("User registered successfully.", response.Message);
            Assert.Equal(userResponse, response.Data);
        }

        [Fact]
        public async Task RegisterUser_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var request = new UserRegisterRequest();
            _controller.ModelState.AddModelError("FirstName", "First name is required.");

            // Act
            var result = await _controller.RegisterUser(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<GeneralCreateResponse>(badRequestResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Invalid request data.", response.Message);
            Assert.IsType<ModelStateDictionary>(response.Data);
        }

        [Fact]
        public async Task RegisterUser_ArgumentException_ReturnsConflict()
        {
            // Arrange
            var request = new UserRegisterRequest();
            var exceptionMessage = "User must be at least 18 years old.";
            _userServiceMock.Setup(s => s.AddUserAsync(request))
                .ThrowsAsync(new ArgumentException(exceptionMessage));

            // Act
            var result = await _controller.RegisterUser(request);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralCreateResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Message);
        }

        [Fact]
        public async Task RegisterUser_InvalidOperationException_ReturnsConflict()
        {
            // Arrange
            var request = new UserRegisterRequest();
            var exceptionMessage = "An error occurred while registering the user.";
            _userServiceMock.Setup(s => s.AddUserAsync(request))
                .ThrowsAsync(new InvalidOperationException(exceptionMessage));

            // Act
            var result = await _controller.RegisterUser(request);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Message);
        }

        [Fact]
        public async Task RegisterUser_Exception_ReturnsInternalServerError()
        {
            // Arrange
            var request = new UserRegisterRequest();
            var exceptionMessage = "An unexpected error occurred.";
            _userServiceMock.Setup(s => s.AddUserAsync(request))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.RegisterUser(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var response = Assert.IsType<GeneralBoolResponse>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Equal("An error occurred while registering the user.", response.Message);
        }

        [Fact]
        public async Task GetFilteredUsers_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var search = "john";
            var role = "Admin";
            var sortBy = "StaffCode";
            var sortOrder = "asc";
            var pageNumber = 1;
            var newStaffCode = "";

            var userItems = new List<GetUserResponse>
            {
                new GetUserResponse { StaffCode = "SD0001", FirstName = "John", LastName = "Doe" },
                new GetUserResponse { StaffCode = "SD0002", FirstName = "Jane", LastName = "Smith" }
            };
            var totalCount = userItems.Count;

            _userServiceMock.Setup(s => s.GetFilteredUsersAsync(adminId, search, role, sortBy, sortOrder, pageNumber, newStaffCode,
                    It.IsAny<int>()))
                .ReturnsAsync((userItems, totalCount));

            // Act
            var result = await _controller.GetFilteredUsers(search, role, sortBy, sortOrder, pageNumber, newStaffCode);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralGetsResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Successfully.", response.Message);
            Assert.Equal(userItems, response.Data);
            Assert.Equal(totalCount, response.TotalCount);
        }

        [Fact]
        public async Task GetFilteredUsers_ArgumentException_ReturnsConflict()
        {
            // Arrange
            var search = "john";
            var role = "Admin";
            var sortBy = "StaffCode";
            var sortOrder = "asc";
            var pageNumber = 1;
            var newStaffCode = "";
            var pageSize = 10;

            var exceptionMessage = "Invalid argument.";

            var adminId = Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new Claim("userId", adminId)
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = principal
                }
            };

            _userServiceMock.Setup(s => s.GetFilteredUsersAsync(adminId, search, role, sortBy, sortOrder, pageNumber, newStaffCode, pageSize))
                .ThrowsAsync(new ArgumentException(exceptionMessage));

            // Act
            var result = await _controller.GetFilteredUsers(search, role, sortBy, sortOrder, pageNumber, newStaffCode);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralGetsResponse>(conflictResult.Value);
            Assert.False(response.Success);
            //Assert.Equal(exceptionMessage, response.Message);
        }

        [Fact]
        public async Task GetFilteredUsers_InvalidOperationException_ReturnsConflict()
        {
            // Arrange
            var search = "john";
            var role = "Admin";
            var sortBy = "StaffCode";
            var sortOrder = "asc";
            var pageNumber = 1;
            var newStaffCode = "";
            var pageSize = 10;

            var exceptionMessage = "Invalid operation.";

            _userServiceMock.Setup(s => s.GetFilteredUsersAsync(adminId, search, role, sortBy, sortOrder, pageNumber, newStaffCode, pageSize))
                .ThrowsAsync(new InvalidOperationException(exceptionMessage));

            // Act
            var result = await _controller.GetFilteredUsers(search, role, sortBy, sortOrder, pageNumber, newStaffCode);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralGetsResponse>(conflictResult.Value);
            Assert.False(response.Success);
            //Assert.Equal(exceptionMessage, response.Message);
        }

        [Fact]
        public async Task GetFilteredUsers_Exception_ReturnsInternalServerError()
        {
            // Arrange
            var search = "john";
            var role = "Admin";
            var sortBy = "StaffCode";
            var sortOrder = "asc";
            var pageNumber = 1;
            var newStaffCode = "";

            var exceptionMessage = "An unexpected error occurred.";


            _userServiceMock.Setup(s => s.GetFilteredUsersAsync(adminId, search, role, sortBy, sortOrder, pageNumber, newStaffCode, It.IsAny<int>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.GetFilteredUsers(search, role, sortBy, sortOrder, pageNumber, newStaffCode);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var response = Assert.IsType<GeneralGetsResponse>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Message);
        }

        [Fact]
        public async Task DisableUser_Success_ReturnsOkResult()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userServiceMock.Setup(s => s.DisableUser(userId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DisableUser(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("User disabled successfully.", response.Message);
        }

        [Fact]
        public async Task DisableUser_Failure_ReturnsConflict()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userServiceMock.Setup(s => s.DisableUser(userId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DisableUser(userId);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal("User have valid assignment", response.Message);
        }

        [Fact]
        public async Task DisableUser_Exception_ReturnsConflict()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var exceptionMessage = "An unexpected error occurred.";
            _userServiceMock.Setup(s => s.DisableUser(userId))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.DisableUser(userId);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Message);
        }

        [Fact]
        public void DisableUser_Unauthorized_ReturnsUnauthorized()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var controller = new UsersController(_userServiceMock.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            // Act
            var result = controller.DisableUser(userId).Result;

            // Assert
            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task Get_WithValidId_ReturnsOkResultWithUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expected = new UserDetailResponse
            {
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now),
                FirstName = "Huy",
                LastName = "Phuc",
                Gender = EnumGender.Male,
                DateJoined = DateOnly.FromDateTime(DateTime.Now),
                RoleId = Guid.NewGuid()
            };

            _userServiceMock.Setup(service => service.GetUserDetailAsync(userId))
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.Get(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralGetResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("User retrieve successfully.", response.Message);
        }

        [Fact]
        public async Task Get_WithException_ReturnsConflictResult()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var exceptionMessage = "An error occurred";

            _userServiceMock.Setup(service => service.GetUserDetailAsync(userId))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.Get(userId);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralGetResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Message);
        }

        [Fact]
        public async Task Put_NotExistId_ReturnsConflictResult()
        {
            // Arrange

            var userId = Guid.NewGuid();
            var exceptionMessage = "An error occurred";

            _userServiceMock.Setup(service => service.UpdateUserAsync(userId, It.IsAny<EditUserRequest>(), It.IsAny<Guid>()))
                .ThrowsAsync(new ArgumentException(exceptionMessage));
            // Act
            var result = await _controller.Put(userId, It.IsAny<EditUserRequest>());

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralGetResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Message);
        }

        [Fact]
        public async Task Put_ExistId_ReturnsOkResult()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var staffCode = "SD00001";
            var updateResponse = new UpdateUserResponse
            {
                StaffCode = staffCode
            };

            _userServiceMock.Setup(service => service.UpdateUserAsync(userId, It.IsAny<EditUserRequest>(), Guid.NewGuid()))
                .ReturnsAsync(updateResponse);
            // Act
            var result = await _controller.Put(userId, It.IsAny<EditUserRequest>());

            // Assert
            var conflictResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralGetResponse>(conflictResult.Value);
            Assert.True(response.Success);
        }
    }
}