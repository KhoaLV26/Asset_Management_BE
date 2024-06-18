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
using System.Threading.Tasks;
using Xunit;

namespace AssetManagement.Tests.Services
{
    public class AssetServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IAssetRepository> _mockAssetRepository;
        private readonly AssetService _assetService;
        private readonly Mock<IMapper> _mockMapper;


        public AssetServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockAssetRepository = new Mock<IAssetRepository>();
            _mockUnitOfWork.Setup(u => u.AssetRepository).Returns(_mockAssetRepository.Object);
            _mockMapper = new Mock<IMapper>();
            _assetService = new AssetService(_mockUnitOfWork.Object, _mockMapper.Object);
        }

        //[Fact]
        //public async Task GetAllAssetAsync_ReturnsPaginatedList()
        //{
        //    // Arrange
        //    var categoryId1 = Guid.NewGuid();
        //    var categoryId2 = Guid.NewGuid();
        //    var assets = new List<Asset>
        //    {
        //        new Asset { Id = Guid.NewGuid(), AssetCode = "A001", AssetName = "Asset 1", CategoryId = categoryId1, Category = new Category { Name = "Category 1" }, Status = EnumAssetStatus.Available },
        //        new Asset { Id = Guid.NewGuid(), AssetCode = "A002", AssetName = "Asset 2", CategoryId = categoryId2, Category = new Category { Name = "Category 2" }, Status = EnumAssetStatus.Assigned}
        //    };
        //    var paginatedList = (items: assets, totalCount: 2);

        //    _mockAssetRepository.Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Asset, bool>>>(), It.IsAny<Func<IQueryable<Asset>, IOrderedQueryable<Asset>>>(), It.IsAny<string>()))
        //        .ReturnsAsync(paginatedList);

        //    // Act
        //    var result = await _assetService.GetAllAssetAsync();

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Equal(2, result.totalCount);
        //    Assert.Equal(2, result.data.Count());
        //}
    }
}
