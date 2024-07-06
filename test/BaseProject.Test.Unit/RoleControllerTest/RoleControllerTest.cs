using AssetManagement.Application.Models.Responses;
using AssetManagement.Application.Services;
using AssetManagement.Domain.Models;
using AssetManagement.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace AssetManagement.Test.Unit.RoleControllerTest
{
    public class RoleControllerTest
    {
        private readonly Mock<IRoleService> _mockRoleService;
        private readonly RolesController _controller;

        public RoleControllerTest()
        {
            _mockRoleService = new Mock<IRoleService>();
            _controller = new RolesController(_mockRoleService.Object);
        }

        [Fact]
        public async Task GetAllRoles_ReturnsOkResult_WithRoles()
        {
            // Arrange
            var roles = new List<RoleResponse>
            {
                new RoleResponse { Id = Guid.NewGuid(), Name = "Admin" },
                new RoleResponse { Id = Guid.NewGuid(), Name = "User" }
            };
            _mockRoleService.Setup(s => s.GetAllAsync()).ReturnsAsync(roles);

            // Act
            var result = await _controller.GetAllRoles();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralCreateResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Roles retrieved successfully.", response.Message);
            Assert.Equal(roles, response.Data);
        }
    }
}
