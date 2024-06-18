using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Application.Services;
using AssetManagement.Domain.Enums;
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

namespace AssetManagement.Test.Unit.AssetControlletTest
{
    public class AssetControllerTest
    {
        private readonly Mock<IAssetService> _assetServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly AssetController _controller;
        public AssetControllerTest()
        {
            _assetServiceMock = new Mock<IAssetService>();
            _userServiceMock = new Mock<IUserService>();
            _controller = new AssetController(_assetServiceMock.Object, _userServiceMock.Object);
        }
        [Fact]
        public async Task GetAllAssetAsync_ReturnsOkResult_WhenAssetsExist()
        {
            // Arrange
            var adminId = Guid.Parse("CFF14216-AC4D-4D5D-9222-C951287E51C6");
            var assets = new List<AssetResponse>
    {
        new AssetResponse { Id = Guid.NewGuid(), AssetCode = "A001", AssetName = "Asset 1" },
        new AssetResponse { Id = Guid.NewGuid(), AssetCode = "A002", AssetName = "Asset 2" }
    };
            var assetResult = (assets, assets.Count);

            _assetServiceMock.Setup(s => s.GetAllAssetAsync(adminId, It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), "Category", It.IsAny<string>()))
                .ReturnsAsync(assetResult);

            // Act
            var result = await _controller.GetAllAssetAsync(1, null, null, null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralGetsResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Assets retrieved successfully.", response.Message);
            Assert.Equal(assets.Count, response.TotalCount);
            Assert.Equal(assets, response.Data);
        }

        [Fact]
        public async Task GetAllAssetAsync_ReturnsConflictResult_WhenNoAssetsExist()
        {
            // Arrange
            var adminId = Guid.Parse("CFF14216-AC4D-4D5D-9222-C951287E51C6");
            var assets = new List<AssetResponse>();
            var assetResult = (assets, assets.Count);

            _assetServiceMock.Setup(s => s.GetAllAssetAsync(adminId, It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), "Category", It.IsAny<string>()))
                .ReturnsAsync(assetResult);

            // Act
            var result = await _controller.GetAllAssetAsync(1, null, null, null, null);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralGetsResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal("No data.", response.Message);
        }

        [Fact]
        public async Task GetAssetId_ReturnsOkResult_WhenAssetExists()
        {
            // Arrange
            var assetId = Guid.NewGuid();
            var asset = new AssetDetailResponse { Id = assetId, AssetCode = "A001", AssetName = "Asset 1" };

            _assetServiceMock.Setup(s => s.GetAssetByIdAsync(assetId))
                .ReturnsAsync(asset);

            // Act
            var result = await _controller.GetAssetId(assetId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralGetResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Asset retrived successfully.", response.Message);
            Assert.Equal(asset, response.Data);
        }

        [Fact]
        public async Task GetAssetId_ReturnsConflictResult_WhenAssetDoesNotExist()
        {
            // Arrange
            var assetId = Guid.NewGuid();

            _assetServiceMock.Setup(s => s.GetAssetByIdAsync(assetId))
                .ReturnsAsync((AssetDetailResponse)null);

            // Act
            var result = await _controller.GetAssetId(assetId);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Asset not found.", response.Message);
        }

        [Fact]
        public async Task GetAssetId_ReturnsConflictResult_WhenExceptionIsThrown()
        {
            // Arrange
            var assetId = Guid.NewGuid();
            var exceptionMessage = "An error occurred.";

            _assetServiceMock.Setup(s => s.GetAssetByIdAsync(assetId))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.GetAssetId(assetId);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralGetResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Message);
        }

        [Fact]
        public async Task GetAllAssetAsync_ReturnsConflictResult_WhenExceptionIsThrown()
        {
            // Arrange
            var adminId = Guid.Parse("CFF14216-AC4D-4D5D-9222-C951287E51C6");
            var exceptionMessage = "An error occurred.";

            _assetServiceMock.Setup(s => s.GetAllAssetAsync(adminId, It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), "Category", It.IsAny<string>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.GetAllAssetAsync(1, null, null, null, null);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralGetsResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Message);
        }

        [Fact]
        public async Task CreateAssetAsync_ReturnsOkResult_WhenAssetCreationSucceeds()
        {
            // Arrange
            var assetRequest = new AssetRequest
            {
                AssetName = "Asset 1",
                CategoryId = Guid.NewGuid(),
                Specification = "Test specification",
                InstallDate = DateOnly.FromDateTime(DateTime.Now),
                Status = EnumAssetStatus.Available
            };

            var createdAsset = new AssetResponse
            {
                Id = Guid.NewGuid(),
                AssetName = assetRequest.AssetName,
                CategoryId = assetRequest.CategoryId,
                Specification = assetRequest.Specification,
                InstallDate = assetRequest.InstallDate,
                Status = assetRequest.Status
            };

            _assetServiceMock.Setup(s => s.CreateAssetAsync(assetRequest))
                .ReturnsAsync(createdAsset);

            // Act
            var result = await _controller.CreateAssetAsync(assetRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralCreateResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Asset created successfully.", response.Message);
            Assert.Equal(createdAsset, response.Data);
        }

        [Fact]
        public async Task CreateAssetAsync_ReturnsConflictResult_WhenAssetCreationFails()
        {
            // Arrange
            var assetRequest = new AssetRequest
            {
                AssetName = "Asset 1",
                CategoryId = Guid.NewGuid(),
                Specification = "Test specification",
                InstallDate = DateOnly.FromDateTime(DateTime.Now),
                Status = EnumAssetStatus.Available
            };

            _assetServiceMock.Setup(s => s.CreateAssetAsync(assetRequest))
                .ReturnsAsync((AssetResponse)null);

            // Act
            var result = await _controller.CreateAssetAsync(assetRequest);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Asset creation failed.", response.Message);
        }

        [Fact]
        public async Task CreateAssetAsync_ReturnsConflictResult_WhenExceptionIsThrown()
        {
            // Arrange
            var assetRequest = new AssetRequest
            {
                AssetName = "Asset 1",
                CategoryId = Guid.NewGuid(),
                Specification = "Test specification",
                InstallDate = DateOnly.FromDateTime(DateTime.Now),
                Status = EnumAssetStatus.Available
            };

            var exceptionMessage = "An error occurred.";
            _assetServiceMock.Setup(s => s.CreateAssetAsync(assetRequest))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.CreateAssetAsync(assetRequest);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Message);
        }
    }
}
