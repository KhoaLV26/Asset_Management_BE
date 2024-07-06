using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Application.Services.Implementations;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Domain.Interfaces;
using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace AssetManagement.Test.Unit.AssetServiceTest
{
    public class CreateAssetTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly AssetService _assetService;

        public CreateAssetTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _assetService = new AssetService(_unitOfWorkMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task CreateAssetAsync_WithValidRequest_ReturnsCreatedAssetResponse()
        {
            // Arrange
            var assetRequest = new AssetRequest
            {
                CreatedBy = Guid.NewGuid(),
                AssetName = "New Asset",
                CategoryId = Guid.NewGuid(),
                Specification = "Test Specification",
                InstallDate = DateOnly.FromDateTime(DateTime.Now),
                Status = EnumAssetStatus.Available
            };

            var adminCreated = new User
            {
                Id = assetRequest.CreatedBy,
                LocationId = Guid.NewGuid(),
            };

            var category = new Category
            {
                Id = assetRequest.CategoryId,
                Code = "TEST",
                Name = "Test Category"
            };

            _unitOfWorkMock.Setup(uow => uow.UserRepository.GetAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Expression<Func<User, object>>>()
            )).ReturnsAsync(adminCreated);

            _unitOfWorkMock.Setup(uow => uow.CategoryRepository.GetAsync(
                It.IsAny<Expression<Func<Category, bool>>>()
            )).ReturnsAsync(category);

            _unitOfWorkMock.Setup(uow => uow.AssetRepository.GetAllAsync(
                It.IsAny<Expression<Func<Asset, bool>>>(),
                It.IsAny<Expression<Func<Asset, object>>>()
            )).ReturnsAsync(new List<Asset>());

            _unitOfWorkMock.Setup(uow => uow.AssetRepository.AddAsync(It.IsAny<Asset>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _unitOfWorkMock.Setup(uow => uow.CommitAsync())
                .ReturnsAsync(1);

            _mapperMock.Setup(mapper => mapper.Map<AssetResponse>(It.IsAny<Asset>()))
                .Returns((Asset asset) => new AssetResponse
                {
                    Id = asset.Id,
                    AssetCode = asset.AssetCode,
                    AssetName = asset.AssetName,
                    CategoryId = asset.CategoryId,
                    CategoryName = asset.Category.Name,
                    Status = asset.Status
                });

            // Act
            var result = await _assetService.CreateAssetAsync(assetRequest);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<AssetResponse>(result);
            Assert.Equal(assetRequest.AssetName, result.AssetName);
            Assert.Equal(assetRequest.CategoryId, result.CategoryId);
            Assert.Equal(category.Name, result.CategoryName);
            Assert.Equal(assetRequest.Status, result.Status);
            Assert.StartsWith(category.Code.ToUpper(), result.AssetCode);

            _unitOfWorkMock.Verify(uow => uow.AssetRepository.AddAsync(It.IsAny<Asset>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAssetAsync_WithNonExistingAdmin_ThrowsKeyNotFoundException()
        {
            // Arrange
            var assetRequest = new AssetRequest
            {
                CreatedBy = Guid.NewGuid(),
                AssetName = "New Asset",
                CategoryId = Guid.NewGuid(),
                Specification = "Test Specification",
                InstallDate = DateOnly.FromDateTime(DateTime.Now),
                Status = EnumAssetStatus.Available
            };

            _unitOfWorkMock.Setup(uow => uow.UserRepository.GetAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Expression<Func<User, object>>>()
            )).ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _assetService.CreateAssetAsync(assetRequest));
        }

        [Fact]
        public async Task CreateAssetAsync_WithNonExistingCategory_ThrowsKeyNotFoundException()
        {
            // Arrange
            var assetRequest = new AssetRequest
            {
                CreatedBy = Guid.NewGuid(),
                AssetName = "New Asset",
                CategoryId = Guid.NewGuid(),
                Specification = "Test Specification",
                InstallDate = DateOnly.FromDateTime(DateTime.Now),
                Status = EnumAssetStatus.Available
            };

            var adminCreated = new User
            {
                Id = assetRequest.CreatedBy,
                LocationId = Guid.NewGuid(),
            };

            _unitOfWorkMock.Setup(uow => uow.UserRepository.GetAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Expression<Func<User, object>>>()
            )).ReturnsAsync(adminCreated);

            _unitOfWorkMock.Setup(uow => uow.CategoryRepository.GetAsync(
                It.IsAny<Expression<Func<Category, bool>>>()
            )).ReturnsAsync((Category)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _assetService.CreateAssetAsync(assetRequest));
        }

        [Fact]
        public async Task CreateAssetAsync_WithFailedCommit_ThrowsException()
        {
            // Arrange
            var assetRequest = new AssetRequest
            {
                CreatedBy = Guid.NewGuid(),
                AssetName = "New Asset",
                CategoryId = Guid.NewGuid(),
                Specification = "Test Specification",
                InstallDate = DateOnly.FromDateTime(DateTime.Now),
                Status = EnumAssetStatus.Available
            };

            var adminCreated = new User
            {
                Id = assetRequest.CreatedBy,
                LocationId = Guid.NewGuid(),
            };

            var category = new Category
            {
                Id = assetRequest.CategoryId,
                Code = "TEST",
                Name = "Test Category"
            };

            _unitOfWorkMock.Setup(uow => uow.UserRepository.GetAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Expression<Func<User, object>>>()
            )).ReturnsAsync(adminCreated);

            _unitOfWorkMock.Setup(uow => uow.CategoryRepository.GetAsync(
                It.IsAny<Expression<Func<Category, bool>>>()
            )).ReturnsAsync(category);

            _unitOfWorkMock.Setup(uow => uow.AssetRepository.GetAllAsync(
                It.IsAny<Expression<Func<Asset, bool>>>(),
                It.IsAny<Expression<Func<Asset, object>>>()
            )).ReturnsAsync(new List<Asset>());

            _unitOfWorkMock.Setup(uow => uow.AssetRepository.AddAsync(It.IsAny<Asset>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _unitOfWorkMock.Setup(uow => uow.CommitAsync())
                .ReturnsAsync(0); // Simulate a failed commit

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _assetService.CreateAssetAsync(assetRequest));

            _unitOfWorkMock.Verify(uow => uow.AssetRepository.AddAsync(It.IsAny<Asset>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task GenerateAssetCodeAsync_WithExistingAssets_ReturnsNextAssetNumber()
        {
            // Arrange
            var prefix = "TEST";
            var existingAssets = new List<Asset>
    {
        new Asset { Id = Guid.NewGuid(), AssetCode = "TEST000001", Category = new Category { Code = "TEST" } },
        new Asset { Id = Guid.NewGuid(), AssetCode = "TEST000002", Category = new Category { Code = "TEST" } },
        new Asset { Id = Guid.NewGuid(), AssetCode = "TEST000003", Category = new Category { Code = "TEST" } }
    };

            _unitOfWorkMock.Setup(uow => uow.AssetRepository.GetAllAsync(
                It.IsAny<Expression<Func<Asset, bool>>>(),
                It.IsAny<Expression<Func<Asset, object>>>()
            )).ReturnsAsync(existingAssets);

            // Act
            var generateAssetCodeAsyncMethod = typeof(AssetService)
                .GetMethod("GenerateAssetCodeAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = await (Task<int>)generateAssetCodeAsyncMethod.Invoke(_assetService, new object[] { prefix });

            // Assert
            Assert.Equal(4, result);
        }
    }
}
