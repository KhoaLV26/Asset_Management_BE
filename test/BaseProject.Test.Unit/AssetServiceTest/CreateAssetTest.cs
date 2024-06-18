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
using System.Linq;
using System.Linq.Expressions;
using System.Text;
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
        public async Task CreateAssetAsync_ValidRequest_ReturnsAssetResponse()
        {
            // Arrange
            var assetRequest = new AssetRequest
            {
                AssetName = "Test Asset",
                CategoryId = Guid.NewGuid(),
                Status = EnumAssetStatus.Available,
                CreatedBy = Guid.NewGuid(),
                Specification = "Test Specification",
                InstallDate = DateOnly.FromDateTime(DateTime.Now)
            };

            var adminUser = new User { Id = assetRequest.CreatedBy, LocationId = Guid.NewGuid() };
            var category = new Category { Id = assetRequest.CategoryId, Code = "TA" };
            var newAsset = new Asset
            {
                AssetCode = "TA000001",
                AssetName = assetRequest.AssetName,
                CategoryId = assetRequest.CategoryId,
                Category = category,
                Status = assetRequest.Status,
                CreatedBy = assetRequest.CreatedBy,
                LocationId = adminUser.LocationId,
                Location = adminUser.Location,
                Specification = assetRequest.Specification,
                InstallDate = assetRequest.InstallDate
            };
            var assetResponse = new AssetResponse();

            _unitOfWorkMock.Setup(uow => uow.UserRepository.GetAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Expression<Func<User, object>>[]>()
            )).ReturnsAsync(adminUser);

            _unitOfWorkMock.Setup(uow => uow.CategoryRepository.GetAsync(
                It.IsAny<Expression<Func<Category, bool>>>()
            )).ReturnsAsync(category);

            _unitOfWorkMock.Setup(uow => uow.AssetRepository.GetAllAsync(
                It.IsAny<Expression<Func<Asset, bool>>>(),
                It.IsAny<Expression<Func<Asset, object>>[]>()
            )).ReturnsAsync(new Asset[0]);

            _unitOfWorkMock.Setup(uow => uow.AssetRepository.AddAsync(It.IsAny<Asset>()))
                .Callback<Asset>(asset => newAsset = asset);

            _unitOfWorkMock.Setup(uow => uow.CommitAsync()).ReturnsAsync(1);

            _mapperMock.Setup(mapper => mapper.Map<AssetResponse>(It.IsAny<Asset>()))
                .Returns(assetResponse);

            // Act
            var result = await _assetService.CreateAssetAsync(assetRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(assetResponse, result);
            _unitOfWorkMock.Verify(uow => uow.UserRepository.GetAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Expression<Func<User, object>>[]>()
            ), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CategoryRepository.GetAsync(
                It.IsAny<Expression<Func<Category, bool>>>()
            ), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.AssetRepository.GetAllAsync(
                It.IsAny<Expression<Func<Asset, bool>>>(),
                It.IsAny<Expression<Func<Asset, object>>[]>()
            ), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.AssetRepository.AddAsync(It.IsAny<Asset>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
            _mapperMock.Verify(mapper => mapper.Map<AssetResponse>(It.IsAny<Asset>()), Times.Once);
        }

        [Fact]
        public async Task CreateAssetAsync_AdminNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var assetRequest = new AssetRequest { CreatedBy = Guid.NewGuid() };

            _unitOfWorkMock.Setup(uow => uow.UserRepository.GetAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Expression<Func<User, object>>[]>()
            )).ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _assetService.CreateAssetAsync(assetRequest));
        }

        [Fact]
        public async Task CreateAssetAsync_CategoryNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var assetRequest = new AssetRequest { CategoryId = Guid.NewGuid() };
            var adminUser = new User { Id = assetRequest.CreatedBy, LocationId = Guid.NewGuid() };

            _unitOfWorkMock.Setup(uow => uow.UserRepository.GetAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Expression<Func<User, object>>[]>()
            )).ReturnsAsync(adminUser);

            _unitOfWorkMock.Setup(uow => uow.CategoryRepository.GetAsync(
                It.IsAny<Expression<Func<Category, bool>>>()
            )).ReturnsAsync((Category)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _assetService.CreateAssetAsync(assetRequest));
        }

        [Fact]
        public async Task CreateAssetAsync_FailedToCreateAsset_ThrowsException()
        {
            // Arrange
            var assetRequest = new AssetRequest
            {
                AssetName = "Test Asset",
                CategoryId = Guid.NewGuid(),
                Status = EnumAssetStatus.Available,
                CreatedBy = Guid.NewGuid(),
                Specification = "Test Specification",
                InstallDate = DateOnly.FromDateTime(DateTime.Now)
            };

            var adminUser = new User { Id = assetRequest.CreatedBy, LocationId = Guid.NewGuid() };
            var category = new Category { Id = assetRequest.CategoryId, Code = "TA" };

            _unitOfWorkMock.Setup(uow => uow.UserRepository.GetAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Expression<Func<User, object>>[]>()
            )).ReturnsAsync(adminUser);

            _unitOfWorkMock.Setup(uow => uow.CategoryRepository.GetAsync(
                It.IsAny<Expression<Func<Category, bool>>>()
            )).ReturnsAsync(category);

            _unitOfWorkMock.Setup(uow => uow.AssetRepository.GetAllAsync(
                It.IsAny<Expression<Func<Asset, bool>>>(),
                It.IsAny<Expression<Func<Asset, object>>[]>()
            )).ReturnsAsync(new Asset[0]);

            _unitOfWorkMock.Setup(uow => uow.CommitAsync()).ReturnsAsync(0);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _assetService.CreateAssetAsync(assetRequest));
        }
    }
}
