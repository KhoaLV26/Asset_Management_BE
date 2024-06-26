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
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AssetManagement.Test.Unit.AssetServiceTest
{
    public class GetReportTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly AssetService _assetService;

        public GetReportTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _assetService = new AssetService(_unitOfWorkMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetReports_WithValidSortBy_ShouldReturnOrderedReports()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var categories = new List<Category>
    {
        new Category { Id = Guid.NewGuid(), Name = "Category 1" },
        new Category { Id = Guid.NewGuid(), Name = "Category 2" }
    };
            var assets = new List<Asset>
    {
        new Asset { CategoryId = categories[0].Id, Status = EnumAssetStatus.Assigned },
        new Asset { CategoryId = categories[0].Id, Status = EnumAssetStatus.Available },
        new Asset { CategoryId = categories[1].Id, Status = EnumAssetStatus.NotAvailable }
    };

            _unitOfWorkMock.Setup(u => u.CategoryRepository.GetAllAsync(c => !c.IsDeleted)).ReturnsAsync(categories);
            _unitOfWorkMock.Setup(u => u.AssetRepository.GetAllAsync(a => a.LocationId == locationId && !a.IsDeleted)).ReturnsAsync(assets);

            // Act
            var result = await _assetService.GetReports("asc", "total", locationId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.count);
            Assert.Equal(2, result.Item1.Count());

            var report1 = result.Item1.ElementAt(0);
            var report2 = result.Item1.ElementAt(1);
            Assert.True(report1.Total <= report2.Total);
        }

        [Fact]
        public async Task GetReports_WithInvalidSortBy_ShouldReturnUnorderedReports()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var categories = new List<Category>
    {
        new Category { Id = Guid.NewGuid(), Name = "Category 1" },
        new Category { Id = Guid.NewGuid(), Name = "Category 2" }
    };
            var assets = new List<Asset>
    {
        new Asset { CategoryId = categories[0].Id, Status = EnumAssetStatus.Assigned },
        new Asset { CategoryId = categories[0].Id, Status = EnumAssetStatus.Available },
        new Asset { CategoryId = categories[1].Id, Status = EnumAssetStatus.NotAvailable }
    };

            _unitOfWorkMock.Setup(u => u.CategoryRepository.GetAllAsync(c => !c.IsDeleted)).ReturnsAsync(categories);
            _unitOfWorkMock.Setup(u => u.AssetRepository.GetAllAsync(a => a.LocationId == locationId && !a.IsDeleted)).ReturnsAsync(assets);

            // Act
            var result = await _assetService.GetReports(null, "invalid", locationId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.count);
            Assert.Equal(2, result.Item1.Count());

            // Assuming the default order is by category name
            var report1 = result.Item1.ElementAt(0);
            var report2 = result.Item1.ElementAt(1);
            Assert.Equal("Category 1", report1.Category);
            Assert.Equal("Category 2", report2.Category);
        }
    }
}
