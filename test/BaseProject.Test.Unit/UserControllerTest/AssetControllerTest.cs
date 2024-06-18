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

namespace AssetManagement.Test.Unit.UserControllerTest
{
    public class AssetControllerTest
    {
        private readonly Mock<IAssetService> _assetServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly AssetController _assetController;

        public AssetControllerTest()
        {
            _assetServiceMock = new Mock<IAssetService>();
            _userServiceMock = new Mock<IUserService>();
            _assetController = new AssetController(_assetServiceMock.Object, _userServiceMock.Object);
        }

        [Fact]
        public async Task CreateAssetAsync_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var assetRequest = new AssetRequest();
            var assetResponse = new AssetResponse();

            _assetServiceMock.Setup(service => service.CreateAssetAsync(assetRequest))
                .ReturnsAsync(assetResponse);

            // Act
            var result = await _assetController.CreateAssetAsync(assetRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralCreateResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Asset created successfully.", response.Message);
            Assert.Equal(assetResponse, response.Data);
        }

        [Fact]
        public async Task CreateAssetAsync_AssetCreationFailed_ReturnsConflictResult()
        {
            // Arrange
            var assetRequest = new AssetRequest();

            _assetServiceMock.Setup(service => service.CreateAssetAsync(assetRequest))
                .ReturnsAsync((AssetResponse)null);

            // Act
            var result = await _assetController.CreateAssetAsync(assetRequest);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Asset creation failed.", response.Message);
        }

        [Fact]
        public async Task CreateAssetAsync_Exception_ReturnsConflictResult()
        {
            // Arrange
            var assetRequest = new AssetRequest();
            var exceptionMessage = "Test exception";

            _assetServiceMock.Setup(service => service.CreateAssetAsync(assetRequest))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _assetController.CreateAssetAsync(assetRequest);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Message);
        }
    }
}
