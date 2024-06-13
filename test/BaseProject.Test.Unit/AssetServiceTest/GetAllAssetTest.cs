using Xunit;
using Moq;
using AssetManagement.Application.Services.Implementations;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Interfaces;
using AssetManagement.Application.Models.Responses;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace AssetManagement.Test.Unit.AssetServiceTest
{
    public class GetAllAssetTest
    {
        [Fact]
        public async Task GetAllAssetAsync_Returns_Assets()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockAssetRepository = new Mock<IAssetRepository>();
            var category1 = Guid.NewGuid();
            var category2 = Guid.NewGuid();
            var assets = new List<Asset>
            {
                new Asset { Id = Guid.NewGuid(), AssetCode = "ASSET1", AssetName = "Asset 1", CategoryId = category1, Status = Domain.Enums.EnumAssetStatus.Available},
                new Asset { Id = Guid.NewGuid(), AssetCode = "ASSET2", AssetName = "Asset 2", CategoryId = category2, Status = Domain.Enums.EnumAssetStatus.Assigned }
            };
            mockAssetRepository.Setup(repo => repo.GetAllAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Asset, bool>>>(), It.IsAny<Func<IQueryable<Asset>, IOrderedQueryable<Asset>>>(), It.IsAny<string>()))
                               .ReturnsAsync((items: assets, totalCount: assets.Count));
            mockUnitOfWork.SetupGet(uow => uow.AssetRepository).Returns(mockAssetRepository.Object);

            var assetService = new AssetService(mockUnitOfWork.Object);

            // Act
            var (data, totalCount) = await assetService.GetAllAssetAsync();

            // Assert
            Assert.NotNull(data);
            Assert.Equal(2, totalCount);
            Assert.Collection(data,
                item =>
                {
                    Assert.Equal("ASSET1", item.AssetCode);
                    Assert.Equal("Asset 1", item.AssetName);
                    Assert.Equal(category1.ToString() , item.CategoryId.ToString());
                    Assert.Equal("Available", item.Status.ToString());
                },
                item =>
                {
                    Assert.Equal("ASSET2", item.AssetCode);
                    Assert.Equal("Asset 2", item.AssetName);
                    Assert.Equal(category2.ToString(), item.CategoryId.ToString());
                    Assert.Equal("Assigned", item.Status.ToString());
                });
        }
    }
}
