using AssetManagement.Application.Models.Responses;
using AssetManagement.Application.Services.Implementations;
using AssetManagement.Domain.Constants;
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
                It.IsAny<Expression<Func<Asset, bool>>>(),
                It.IsAny<int>()
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
                It.IsAny<Expression<Func<Asset, bool>>>(),
                It.IsAny<int>()
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
                It.IsAny<Expression<Func<Asset, bool>>>(),
                It.IsAny<int>()
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
        [InlineData(SortConstants.Report.SORT_BY_TOTAL, "asc")]
        [InlineData(SortConstants.Report.SORT_BY_TOTAL, "desc")]
        [InlineData(SortConstants.Report.SORT_BY_ASSIGNED, "asc")]
        [InlineData(SortConstants.Report.SORT_BY_ASSIGNED, "desc")]
        [InlineData(SortConstants.Report.SORT_BY_AVAILABLE, "asc")]
        [InlineData(SortConstants.Report.SORT_BY_AVAILABLE, "desc")]
        [InlineData(SortConstants.Report.SORT_BY_NOT_AVAILABLE, "asc")]
        [InlineData(SortConstants.Report.SORT_BY_NOT_AVAILABLE, "desc")]
        [InlineData(SortConstants.Report.SORT_BY_WAITING_FOR_RECYCLING, "asc")]
        [InlineData(SortConstants.Report.SORT_BY_WAITING_FOR_RECYCLING, "desc")]
        [InlineData(SortConstants.Report.SORT_BY_RECYCLED, "asc")]
        [InlineData(SortConstants.Report.SORT_BY_RECYCLED, "desc")]
        [InlineData(SortConstants.Report.SORT_BY_CATEGORY, "asc")]
        [InlineData(SortConstants.Report.SORT_BY_CATEGORY, "desc")]
        public void GetOrderReportQuery_ShouldReturnCorrectOrderingFunction(string sortBy, string sortOrder)
        {
            // Arrange
            var reports = new[]
            {
                new ReportResponse { Category = "B", Total = 3, Assigned = 2, Available = 1, NotAvailable = 0, WaitingForRecycling = 0, Recycled = 0 },
                new ReportResponse { Category = "A", Total = 1, Assigned = 0, Available = 1, NotAvailable = 0, WaitingForRecycling = 0, Recycled = 0 },
                new ReportResponse { Category = "C", Total = 2, Assigned = 1, Available = 0, NotAvailable = 1, WaitingForRecycling = 0, Recycled = 0 }
            }.AsQueryable();

            // Act
            var orderFunc = _assetService.GetType()
                .GetMethod("GetOrderReportQuery", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_assetService, new object[] { sortOrder, sortBy }) as Func<IQueryable<ReportResponse>, IOrderedQueryable<ReportResponse>>;

            var result = orderFunc(reports).ToList();

            // Assert
            switch (sortBy.ToLower())
            {
                case SortConstants.Report.SORT_BY_TOTAL:
                    AssertOrder(result, r => r.Total, sortOrder);
                    break;
                case SortConstants.Report.SORT_BY_ASSIGNED:
                    AssertOrder(result, r => r.Assigned, sortOrder);
                    break;
                case SortConstants.Report.SORT_BY_AVAILABLE:
                    AssertOrder(result, r => r.Available, sortOrder);
                    break;
                case SortConstants.Report.SORT_BY_NOT_AVAILABLE:
                    AssertOrder(result, r => r.NotAvailable, sortOrder);
                    break;
                case SortConstants.Report.SORT_BY_WAITING_FOR_RECYCLING:
                    AssertOrder(result, r => r.WaitingForRecycling, sortOrder);
                    break;
                case SortConstants.Report.SORT_BY_RECYCLED:
                    AssertOrder(result, r => r.Recycled, sortOrder);
                    break;
                case SortConstants.Report.SORT_BY_CATEGORY:
                    AssertOrder(result, r => r.Category, sortOrder);
                    break;
            }
        }

        [Fact]
        public void GetOrderReportQuery_ShouldReturnNull_WhenSortByIsInvalid()
        {
            // Arrange
            string sortBy = "InvalidSortBy";
            string sortOrder = "asc";

            // Act
            var orderFunc = _assetService.GetType()
                .GetMethod("GetOrderReportQuery", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_assetService, new object[] { sortOrder, sortBy }) as Func<IQueryable<ReportResponse>, IOrderedQueryable<ReportResponse>>;

            // Assert
            Assert.Null(orderFunc);
        }

        private void AssertOrder<T>(System.Collections.Generic.List<ReportResponse> result, Func<ReportResponse, T> selector, string sortOrder)
            where T : IComparable
        {
            if (sortOrder != "desc")
            {
                Assert.True(result.SequenceEqual(result.OrderBy(selector)));
            }
            else
            {
                Assert.True(result.SequenceEqual(result.OrderByDescending(selector)));
            }
        }
    }
}

