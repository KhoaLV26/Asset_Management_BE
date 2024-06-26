using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Application.Services;
using AssetManagement.Domain.Enums;
using AssetManagement.Domain.Models;
using AssetManagement.WebAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AssetManagement.Test.Unit.AssetControlletTest
{
    public class AssetControllerTest
    {
        private readonly Mock<IAssetService> _assetServiceMock;
        private readonly AssetController _controller;

        public AssetControllerTest()
        {
            _assetServiceMock = new Mock<IAssetService>();
            _controller = new AssetController(_assetServiceMock.Object);
        }

        [Fact]
        public async Task CreateAssetAsync_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var assetRequest = new AssetRequest
            {
                AssetName = "Test Asset",
                CategoryId = Guid.NewGuid(),
                Specification = "Test Specification",
                InstallDate = DateOnly.FromDateTime(DateTime.Now),
                Status = EnumAssetStatus.Available,
                CreatedBy = Guid.NewGuid()
            };

            var expectedAssetResponse = new AssetResponse
            {
                Id = Guid.NewGuid(),
                AssetCode = "TEST001",
                AssetName = assetRequest.AssetName,
                CategoryId = assetRequest.CategoryId,
                CategoryName = "Test Category",
                Status = assetRequest.Status
            };

            _assetServiceMock.Setup(service => service.CreateAssetAsync(assetRequest))
                .ReturnsAsync(expectedAssetResponse);

            // Act
            var result = await _controller.CreateAssetAsync(assetRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralCreateResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Asset created successfully.", response.Message);
            Assert.Equal(expectedAssetResponse, response.Data);
        }

        [Fact]
        public async Task CreateAssetAsync_AssetCreationFailed_ReturnsConflictResult()
        {
            // Arrange
            var assetRequest = new AssetRequest
            {
                AssetName = "Test Asset",
                CategoryId = Guid.NewGuid(),
                Specification = "Test Specification",
                InstallDate = DateOnly.FromDateTime(DateTime.Now),
                Status = EnumAssetStatus.Available,
                CreatedBy = Guid.NewGuid()
            };

            _assetServiceMock.Setup(service => service.CreateAssetAsync(assetRequest))
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
        public async Task CreateAssetAsync_Exception_ReturnsConflictResult()
        {
            // Arrange
            var assetRequest = new AssetRequest
            {
                AssetName = "Test Asset",
                CategoryId = Guid.NewGuid(),
                Specification = "Test Specification",
                InstallDate = DateOnly.FromDateTime(DateTime.Now),
                Status = EnumAssetStatus.Available,
                CreatedBy = Guid.NewGuid()
            };

            var exceptionMessage = "Test exception";

            _assetServiceMock.Setup(service => service.CreateAssetAsync(assetRequest))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.CreateAssetAsync(assetRequest);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Message);
        }

        //    [Fact]
        //    public async Task GetAllAssetAsync_ReturnsOkResult_WhenAssetsExist()
        //    {
        //        // Arrange
        //        var adminId = Guid.NewGuid();
        //        var assets = new List<AssetResponse>
        //{
        //    new AssetResponse { Id = Guid.NewGuid(), AssetCode = "A001", AssetName = "Asset 1" },
        //    new AssetResponse { Id = Guid.NewGuid(), AssetCode = "A002", AssetName = "Asset 2" }
        //};
        //        var assetResult = (assets, assets.Count);

        //        // Set up the controller context with a mocked user
        //        var claims = new List<Claim>
        //{
        //    new Claim("userId", adminId.ToString())
        //};
        //        var identity = new ClaimsIdentity(claims);
        //        var principal = new ClaimsPrincipal(identity);
        //        var httpContext = new DefaultHttpContext { User = principal };
        //        var controllerContext = new ControllerContext { HttpContext = httpContext };
        //        _controller.ControllerContext = controllerContext;

        //        _assetServiceMock.Setup(s => s.GetAllAssetAsync(adminId, It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), "Category", It.IsAny<string>()))
        //            .ReturnsAsync(assetResult);

        //        // Act
        //        var result = await _controller.GetAllAssetAsync(1, null, null, null, null, null);

        //        // Assert
        //        var okResult = Assert.IsType<OkObjectResult>(result);
        //        var response = Assert.IsType<GeneralGetsResponse>(okResult.Value);
        //        Assert.True(response.Success);
        //        Assert.Equal("Assets retrieved successfully.", response.Message);
        //        Assert.Equal(assets.Count, response.TotalCount);
        //        Assert.Equal(assets, response.Data);
        //    }

        [Fact]
        public async Task GetAllAssetAsync_ReturnsConflictResult_WhenNoAssetsExist()
        {
            // Arrange
            var adminId = Guid.NewGuid();
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
        }

        [Fact]
        public async Task GetAssetId_ReturnsOkResult_WhenAssetExists()
        {
            // Arrange
            var assetId = Guid.NewGuid();
            var asset = new AssetDetailResponse
            {
                AssetCode = "A001",
                AssetName = "Asset 1",
                CategoryId = Guid.NewGuid(),
                Status = EnumAssetStatus.Available,
                AssignmentResponses = new List<AssignmentResponse>()
            };

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
        public async Task GetAssetId_ReturnsOkResult_WhenAssetDoesNotExist()
        {
            // Arrange
            var assetId = Guid.NewGuid();

            _assetServiceMock.Setup(s => s.GetAssetByIdAsync(assetId))
                .ReturnsAsync((AssetDetailResponse)null);

            // Act
            var result = await _controller.GetAssetId(assetId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralGetResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Asset retrived successfully.", response.Message);
            Assert.Null(response.Data);
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
            var adminId = Guid.NewGuid();
            var exceptionMessage = "An error occurred.";

            // Set up the controller context with a mocked user
            var claims = new List<Claim>
            {
                new Claim("userId", adminId.ToString())
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };
            var controllerContext = new ControllerContext { HttpContext = httpContext };
            _controller.ControllerContext = controllerContext;

            _assetServiceMock.Setup(s => s.GetAllAssetAsync(adminId, It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), "Category", It.IsAny<string>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.GetAllAssetAsync(1, null, null, null, null);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralGetsResponse>(conflictResult.Value);
            Assert.False(response.Success);
        }

        [Fact]
        public async Task DeleteAsset_ValidId_ReturnsOkResult()
        {
            // Arrange
            var assetId = Guid.NewGuid();
            var expectedAssetResponse = new AssetResponse
            {
                Id = assetId,
                AssetCode = "TEST001",
                AssetName = "Test Asset",
                CategoryId = Guid.NewGuid(),
                CategoryName = "Test Category",
                Status = EnumAssetStatus.Available
            };

            _assetServiceMock.Setup(service => service.DeleteAssetAsync(assetId))
                .ReturnsAsync(expectedAssetResponse);

            // Act
            var result = await _controller.DeleteAsset(assetId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralGetResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Asset delete successfully.", response.Message);
            Assert.Equal(expectedAssetResponse, response.Data);
        }

        [Fact]
        public async Task DeleteAsset_AssetDeleteFailed_ReturnsConflictResult()
        {
            // Arrange
            var assetId = Guid.NewGuid();

            _assetServiceMock.Setup(service => service.DeleteAssetAsync(assetId))
                .ReturnsAsync((AssetResponse)null);

            // Act
            var result = await _controller.DeleteAsset(assetId);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Asset delete failed.", response.Message);
        }

        [Fact]
        public async Task DeleteAsset_Exception_ReturnsConflictResult()
        {
            // Arrange
            var assetId = Guid.NewGuid();
            var exceptionMessage = "An error occurred.";

            _assetServiceMock.Setup(service => service.DeleteAssetAsync(assetId))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.DeleteAsset(assetId);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Message);
        }

        [Fact]
        public async Task UpdateAsset_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var assetId = Guid.NewGuid();
            var assetRequest = new AssetUpdateRequest
            {
                AssetName = "Updated Asset",
                Specification = "Updated Specification",
                InstallDate = DateOnly.FromDateTime(DateTime.Now),
                Status = EnumAssetStatus.Available
            };

            var expectedAssetResponse = new AssetResponse
            {
                AssetCode = "TEST001",
                AssetName = assetRequest.AssetName,
                Specification = assetRequest.Specification,
                InstallDate = assetRequest.InstallDate,
                Status = assetRequest.Status
            };

            _assetServiceMock.Setup(service => service.UpdateAsset(assetId, assetRequest))
                .ReturnsAsync(expectedAssetResponse);

            // Act
            var result = await _controller.UpdateAsset(assetId, assetRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralGetResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Update successfully", response.Message);
            Assert.Equal(expectedAssetResponse, response.Data);
        }

        [Fact]
        public async Task UpdateAsset_Exception_ReturnsConflictResult()
        {
            // Arrange
            var assetId = Guid.NewGuid();
            var assetRequest = new AssetUpdateRequest();
            var exceptionMessage = "An error occurred.";

            _assetServiceMock.Setup(service => service.UpdateAsset(assetId, assetRequest))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.UpdateAsset(assetId, assetRequest);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralGetResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Message);
        }

        [Fact]
        public async Task GetReports_ReturnsConflictResult_WhenNoReportsExist()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var exceptionMessage = "Value cannot be null. (Parameter 'input')";

            // Set up the controller context with a mocked user
            var claims = new List<Claim>
            {
                new Claim("locationId", locationId.ToString())
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };
            var controllerContext = new ControllerContext { HttpContext = httpContext };
            _controller.ControllerContext = controllerContext;

            _assetServiceMock.Setup(s => s.GetReports(It.IsAny<string>(), It.IsAny<string>(), locationId, It.IsAny<int>()))
                .ThrowsAsync(new ArgumentNullException("input"));

            // Act
            var result = await _controller.GetReports(1, null, null);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralGetsResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Message);
        }

        [Fact]
        public async Task GetReports_ReturnsConflictResult_WhenExceptionIsThrown()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var exceptionMessage = "Value cannot be null. (Parameter 'input')";

            // Set up the controller context with a mocked user
            var claims = new List<Claim>
            {
                new Claim("locationId", locationId.ToString())
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };
            var controllerContext = new ControllerContext { HttpContext = httpContext };
            _controller.ControllerContext = controllerContext;

            _assetServiceMock.Setup(s => s.GetReports(It.IsAny<string>(), It.IsAny<string>(), locationId, It.IsAny<int>()))
                .ThrowsAsync(new ArgumentNullException("input"));

            // Act
            var result = await _controller.GetReports(1, null, null);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralGetsResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Message);
        }

        [Fact]
        public async Task ExportToExcel_ReturnsConflictResult_WhenExceptionOccurs()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var exceptionMessage = "Value cannot be null. (Parameter 'input')";

            // Set up the controller context with a mocked user
            var claims = new List<Claim>
            {
                new Claim("locationId", locationId.ToString())
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };
            var controllerContext = new ControllerContext { HttpContext = httpContext };
            _controller.ControllerContext = controllerContext;

            _assetServiceMock.Setup(s => s.ExportToExcelAsync(locationId))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.ExportToExcel();

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Message);
        }
    }
}
