using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Application.Services;
using AssetManagement.Domain.Models;
using AssetManagement.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AssetManagement.Test.Unit.LocationControllerTest
{
    public class LocationControllerTest
    {
        private readonly Mock<ILocationService> _mockLocationService;
        private readonly LocationController _controller;

        public LocationControllerTest()
        {
            _mockLocationService = new Mock<ILocationService>();
            _controller = new LocationController(_mockLocationService.Object);
        }

        [Fact]
        public async Task GetAllLocationAsync_ReturnsOkResult_WhenServiceCallSucceeds()
        {
            // Arrange
            var locations = new List<LocationResponse> { new LocationResponse(), new LocationResponse() };
            _mockLocationService.Setup(service => service.GetAllLocationAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync((locations, 2));

            // Act
            var result = await _controller.GetAllLocationAsync(1, null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralGetsResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Successfully.", response.Message);
            Assert.Equal(locations, response.Data);
            Assert.Equal(2, response.TotalCount);
        }

        [Fact]
        public async Task GetAllLocationAsync_ReturnsConflictResult_WhenExceptionOccurs()
        {
            // Arrange
            _mockLocationService.Setup(service => service.GetAllLocationAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetAllLocationAsync(1, null, null);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralGetsResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Test exception", response.Message);
        }

        [Fact]
        public async Task CreateLocationAsync_ReturnsOkResult_WhenServiceCallSucceeds()
        {
            // Arrange
            var request = new LocationCreateRequest { Name = "Test Location", Code = "TL" };
            var locationResponse = new LocationCreateResponse { Id = Guid.NewGuid(), Name = "Test Location", Code = "TL" };
            _mockLocationService.Setup(service => service.CreateLocationAsync(It.IsAny<LocationCreateRequest>()))
                .ReturnsAsync(locationResponse);

            // Act
            var result = await _controller.CreateLocationAsync(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralCreateResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Location created successfully.", response.Message);
            Assert.Equal(locationResponse, response.Data);
        }

        [Fact]
        public async Task CreateLocationAsync_ReturnsConflictResult_WhenExceptionOccurs()
        {
            // Arrange
            var request = new LocationCreateRequest { Name = "Test Location", Code = "TL" };
            _mockLocationService.Setup(service => service.CreateLocationAsync(It.IsAny<LocationCreateRequest>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.CreateLocationAsync(request);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralCreateResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Test exception", response.Message);
        }

        [Fact]
        public async Task UpdateLocationAsync_ReturnsOkResult_WhenServiceCallSucceeds()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var request = new LocationUpdateRequest { Name = "Updated Location", Code = "UL" };
            var locationResponse = new LocationResponse { Id = locationId, Name = "Updated Location", Code = "UL" };
            _mockLocationService.Setup(service => service.UpdateLocationAsync(It.IsAny<Guid>(), It.IsAny<LocationUpdateRequest>()))
                .ReturnsAsync(locationResponse);

            // Act
            var result = await _controller.UpdateLocationAsync(locationId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralCreateResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Location updated successfully.", response.Message);
            Assert.Equal(locationResponse, response.Data);
        }

        [Fact]
        public async Task UpdateLocationAsync_ReturnsConflictResult_WhenExceptionOccurs()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var request = new LocationUpdateRequest { Name = "Updated Location", Code = "UL" };
            _mockLocationService.Setup(service => service.UpdateLocationAsync(It.IsAny<Guid>(), It.IsAny<LocationUpdateRequest>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.UpdateLocationAsync(locationId, request);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralCreateResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Test exception", response.Message);
        }

        [Fact]
        public async Task GetLocationByIdAsync_ReturnsOkResult_WhenServiceCallSucceeds()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var locationResponse = new LocationResponse { Id = locationId, Name = "Test Location", Code = "TL" };
            _mockLocationService.Setup(service => service.GetLocationByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(locationResponse);

            // Act
            var result = await _controller.GetLocationByIdAsync(locationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralGetsResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Successfully.", response.Message);
            Assert.Equal(locationResponse, response.Data);
        }

        [Fact]
        public async Task GetLocationByIdAsync_ReturnsConflictResult_WhenExceptionOccurs()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            _mockLocationService.Setup(service => service.GetLocationByIdAsync(It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetLocationByIdAsync(locationId);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralGetsResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Test exception", response.Message);
        }
    }
}
