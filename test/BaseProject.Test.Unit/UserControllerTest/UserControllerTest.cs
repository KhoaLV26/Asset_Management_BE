﻿using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Application.Services;
using AssetManagement.Domain.Models;
using AssetManagement.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AssetManagement.Test.Unit.UserControllerTest
{
    public class UserControllerTest
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly UsersController _controller;

        public UserControllerTest()
        {
            _userServiceMock = new Mock<IUserService>();
            _controller = new UsersController(_userServiceMock.Object);
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
            var adminId = "CFF14216-AC4D-4D5D-9222-C951287E51C6";
            var search = "john";
            var role = "Admin";
            var sortBy = "StaffCode";
            var sortDirection = "asc";
            var pageNumber = 1;
            var newStaffCode = "";

            var userItems = new List<GetUserResponse>
    {
        new GetUserResponse { StaffCode = "SD0001", FirstName = "John", LastName = "Doe" },
        new GetUserResponse { StaffCode = "SD0002", FirstName = "Jane", LastName = "Smith" }
    };
            var totalCount = userItems.Count;

            _userServiceMock.Setup(s => s.GetFilteredUsersAsync(adminId, search, role, sortBy, sortDirection, pageNumber, newStaffCode))
                .ReturnsAsync((userItems, totalCount));

            // Act
            var result = await _controller.GetFilteredUsers(adminId, search, role, sortBy, sortDirection, pageNumber, newStaffCode);

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
            var adminId = "CFF14216-AC4D-4D5D-9222-C951287E51C6";
            var search = "john";
            var role = "Admin";
            var sortBy = "StaffCode";
            var sortDirection = "asc";
            var pageNumber = 1;
            var newStaffCode = "";

            var exceptionMessage = "Invalid argument.";
            _userServiceMock.Setup(s => s.GetFilteredUsersAsync(adminId, search, role, sortBy, sortDirection, pageNumber, newStaffCode))
                .ThrowsAsync(new ArgumentException(exceptionMessage));

            // Act
            var result = await _controller.GetFilteredUsers(adminId, search, role, sortBy, sortDirection, pageNumber, newStaffCode);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralGetsResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Message);
        }

        [Fact]
        public async Task GetFilteredUsers_InvalidOperationException_ReturnsConflict()
        {
            // Arrange
            var adminId = "CFF14216-AC4D-4D5D-9222-C951287E51C6";
            var search = "john";
            var role = "Admin";
            var sortBy = "StaffCode";
            var sortDirection = "asc";
            var pageNumber = 1;
            var newStaffCode = "";

            var exceptionMessage = "Invalid operation.";
            _userServiceMock.Setup(s => s.GetFilteredUsersAsync(adminId, search, role, sortBy, sortDirection, pageNumber, newStaffCode))
                .ThrowsAsync(new InvalidOperationException(exceptionMessage));

            // Act
            var result = await _controller.GetFilteredUsers(adminId, search, role, sortBy, sortDirection, pageNumber, newStaffCode);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralGetsResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Message);
        }

        [Fact]
        public async Task GetFilteredUsers_Exception_ReturnsInternalServerError()
        {
            // Arrange
            var adminId = "CFF14216-AC4D-4D5D-9222-C951287E51C6";
            var search = "john";
            var role = "Admin";
            var sortBy = "StaffCode";
            var sortDirection = "asc";
            var pageNumber = 1;
            var newStaffCode = "";

            var exceptionMessage = "An unexpected error occurred.";
            _userServiceMock.Setup(s => s.GetFilteredUsersAsync(adminId, search, role, sortBy, sortDirection, pageNumber, newStaffCode))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.GetFilteredUsers(adminId, search, role, sortBy, sortDirection, pageNumber, newStaffCode);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var response = Assert.IsType<GeneralGetsResponse>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Message);
        }
    }
}
