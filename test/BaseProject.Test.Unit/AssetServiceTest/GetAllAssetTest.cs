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
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace AssetManagement.Test.Unit.AssetServiceTest
{
    public class AssetServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly AssetService _assetService;

        public AssetServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _assetService = new AssetService(_unitOfWorkMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetAllAssetAsync_WithoutParameters_ReturnsAssetResponseWithTotalCount()
        {
            // Arrange
            var assets = new List<Asset>
        {
            new Asset { Id = Guid.NewGuid(), AssetCode = "A001", AssetName = "Asset 1", CategoryId = Guid.NewGuid(), Category = new Category { Name = "Category 1" }, Status = EnumAssetStatus.Available },
            new Asset { Id = Guid.NewGuid(), AssetCode = "A002", AssetName = "Asset 2", CategoryId = Guid.NewGuid(), Category = new Category { Name = "Category 2" }, Status = EnumAssetStatus.Assigned }
        };

            _unitOfWorkMock.Setup(uow => uow.AssetRepository.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<Expression<Func<Asset, bool>>>(),
                It.IsAny<Func<IQueryable<Asset>, IOrderedQueryable<Asset>>>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Asset, bool>>>()
            )).ReturnsAsync((assets, assets.Count));

            // Act
            var result = await _assetService.GetAllAssetAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<(IEnumerable<AssetResponse> data, int totalCount)>(result);
            Assert.Equal(2, result.totalCount);
            Assert.Equal(2, result.data.Count());
        }

        [Fact]
        public async Task GetAllAssetAsync_WithParameters_ReturnsFilteredAndSortedAssetResponseWithTotalCount()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var assets = new List<Asset>
        {
            new Asset { Id = Guid.NewGuid(), AssetCode = "A001", AssetName = "Asset 1", CategoryId = categoryId, Category = new Category { Name = "Category 1" }, Status = EnumAssetStatus.Available },
            new Asset { Id = Guid.NewGuid(), AssetCode = "A002", AssetName = "Asset 2", CategoryId = Guid.NewGuid(), Category = new Category { Name = "Category 2" }, Status = EnumAssetStatus.Assigned }
        };

            var user = new User { Id = adminId, LocationId = Guid.NewGuid() };

            _unitOfWorkMock.Setup(uow => uow.UserRepository.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(user);

            _unitOfWorkMock.Setup(uow => uow.AssetRepository.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<Expression<Func<Asset, bool>>>(),
                It.IsAny<Func<IQueryable<Asset>, IOrderedQueryable<Asset>>>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Asset, bool>>>()
            )).ReturnsAsync((assets, assets.Count));

            // Act
            var result = await _assetService.GetAllAssetAsync(adminId, 1, "available", categoryId, "Asset 1", "asc", "assetName");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<(IEnumerable<AssetResponse> data, int totalCount)>(result);
            Assert.Equal(assets.Count, result.totalCount);
            Assert.Equal(assets.Count, result.data.Count());
            Assert.Equal("Asset 1", result.data.First().AssetName);
        }

        [Fact]
        public async Task GetAllAssetAsync_WithNewAssetCode_ReturnsPrioritizedAssetResponseWithTotalCount()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var newAssetCode = "NEW001";
            var assets = new List<Asset>
            {
                new Asset { Id = Guid.NewGuid(), AssetCode = "A001", AssetName = "Asset 1", CategoryId = categoryId, Category = new Category { Name = "Category 1" }, Status = EnumAssetStatus.Available },
                new Asset { Id = Guid.NewGuid(), AssetCode = newAssetCode, AssetName = "New Asset", CategoryId = categoryId, Category = new Category { Name = "Category 1" }, Status = EnumAssetStatus.Available },
                new Asset { Id = Guid.NewGuid(), AssetCode = "A002", AssetName = "Asset 2", CategoryId = categoryId, Category = new Category { Name = "Category 2" }, Status = EnumAssetStatus.Assigned }
            };

            var user = new User { Id = adminId, LocationId = Guid.NewGuid() };

            _unitOfWorkMock.Setup(uow => uow.UserRepository.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(user);

            _unitOfWorkMock.Setup(uow => uow.AssetRepository.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<Expression<Func<Asset, bool>>>(),
                It.IsAny<Func<IQueryable<Asset>, IOrderedQueryable<Asset>>>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Asset, bool>>>()
            )).ReturnsAsync((assets, assets.Count));

            // Act
            var result = await _assetService.GetAllAssetAsync(adminId, 1, "available", categoryId, null, "asc", "assetCode", "", newAssetCode);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<(IEnumerable<AssetResponse> data, int totalCount)>(result);
            Assert.Equal(3, result.totalCount);
            Assert.Equal(3, result.data.Count());
            //Assert.Equal(newAssetCode, result.data.First().AssetCode);
        }

        [Fact]
        public async Task GetFilterQuery_WithInvalidStatusValue_ThrowsInvalidCastException()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var user = new User { Id = adminId, LocationId = Guid.NewGuid() };
            _unitOfWorkMock.Setup(uow => uow.UserRepository.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(user);

            // Act & Assert
            var getFilterQueryMethod = typeof(AssetService)
                .GetMethod("GetFilterQuery", BindingFlags.NonPublic | BindingFlags.Instance);
            await Assert.ThrowsAsync<InvalidCastException>(() => (Task<Expression<Func<Asset, bool>>>)getFilterQueryMethod.Invoke(
                _assetService, new object[] { adminId, categoryId, "invalid_status", "Asset" }));
        }

        [Fact]
        public async Task GetFilterQuery_WithoutStateParameter_AppliesDefaultStateConditions()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var user = new User { Id = adminId, LocationId = Guid.NewGuid() };
            _unitOfWorkMock.Setup(uow => uow.UserRepository.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(user);

            // Act
            var getFilterQueryMethod = typeof(AssetService)
                .GetMethod("GetFilterQuery", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = await (Task<Expression<Func<Asset, bool>>>)getFilterQueryMethod.Invoke(
                _assetService, new object[] { adminId, categoryId, null, "Asset" });

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<Expression<Func<Asset, bool>>>(result);
        }

        [Theory]
        [InlineData("category", "asc")]
        [InlineData("category", "desc")]
        [InlineData("state", "asc")]
        [InlineData("state", "desc")]
        [InlineData("invalid_sort_by", "asc")]
        [InlineData(null, "asc")]
        public void GetOrderQuery_WithDifferentSortOptions_ReturnsExpectedOrderByFunction(string sortBy, string sortOrder)
        {
            // Act
            var getOrderQueryMethod = typeof(AssetService)
                .GetMethod("GetOrderQuery", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = (Func<IQueryable<Asset>, IOrderedQueryable<Asset>>)getOrderQueryMethod.Invoke(
                _assetService, new object[] { sortOrder, sortBy });

            // Assert
            switch (sortBy)
            {
                case "category":
                case "state":
                    Assert.NotNull(result);
                    Assert.IsType<Func<IQueryable<Asset>, IOrderedQueryable<Asset>>>(result);
                    break;
                case "invalid_sort_by":
                case null:
                    Assert.Null(result);
                    break;
                default:
                    Assert.Null(result);
                    break;
            }
        }
        //[Fact]
        //public async Task GetFilterQuery_WithValidParameters_ReturnsFilterExpression()
        //{
        //    // Arrange
        //    var adminId = Guid.NewGuid();
        //    var categoryId = Guid.NewGuid();
        //    var user = new User { Id = adminId, LocationId = Guid.NewGuid() };
        //    _unitOfWorkMock.Setup(uow => uow.UserRepository.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
        //        .ReturnsAsync(user);

        //    // Act
        //    var getFilterQueryMethod = typeof(AssetService)
        //        .GetMethod("GetFilterQuery", BindingFlags.NonPublic | BindingFlags.Instance);
        //    var result = await (Task<Expression<Func<Asset, bool>>>)getFilterQueryMethod.Invoke(
        //        _assetService, new object[] { adminId, categoryId, "available", "Asset" });

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.IsType<Expression<Func<Asset, bool>>>(result);
        //}

        //[Fact]
        //public void GetOrderQuery_WithValidParameters_ReturnsOrderByFunction()
        //{
        //    // Arrange
        //    var sortOrder = "asc";
        //    var sortBy = "assetName";

        //    // Act
        //    var getOrderQueryMethod = typeof(AssetService)
        //        .GetMethod("GetOrderQuery", BindingFlags.NonPublic | BindingFlags.Instance);
        //    var result = (Func<IQueryable<Asset>, IOrderedQueryable<Asset>>)getOrderQueryMethod.Invoke(
        //        _assetService, new object[] { sortOrder, sortBy });

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.IsType<Func<IQueryable<Asset>, IOrderedQueryable<Asset>>>(result);
        //}
    }
}
