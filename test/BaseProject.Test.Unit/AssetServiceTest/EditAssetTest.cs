using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Services.Implementations;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Domain.Interfaces;
using AutoMapper;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AssetManagement.Test.Unit.AssetServiceTest
{
    public class EditAssetTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly AssetService _assetService;
        private readonly Mock<IAssetRepository> _assetRepositoryMock;

        public EditAssetTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _assetRepositoryMock = new Mock<IAssetRepository>();
            _unitOfWorkMock.Setup(u => u.AssetRepository).Returns(_assetRepositoryMock.Object);
            _assetService = new AssetService(_unitOfWorkMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task UpdateAsset_ExistingAsset_UpdatesAssetAndReturnsResponse()
        {
            // Arrange
            var assetId = Guid.NewGuid();
            var existingAsset = new Asset
            {
                Id = assetId,
                AssetCode = "A001",
                AssetName = "Old Asset",
                Specification = "Old Specification",
                InstallDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-10)),
                Status = EnumAssetStatus.Available
            };
            var updateRequest = new AssetUpdateRequest
            {
                AssetName = "Updated Asset",
                Specification = "Updated Specification",
                InstallDate = DateOnly.FromDateTime(DateTime.Now),
                Status = EnumAssetStatus.WaitingForRecycling
            };

            _assetRepositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Asset, bool>>>()))
                .ReturnsAsync((Expression<Func<Asset, bool>> expression) =>
                {
                    if (expression.Compile().Invoke(existingAsset))
                    {
                        return existingAsset;
                    }
                    return null;
                });

            // Act
            var result = await _assetService.UpdateAsset(assetId, updateRequest);

            // Assert
            _assetRepositoryMock.Verify(r => r.GetAsync(It.IsAny<Expression<Func<Asset, bool>>>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);

            Assert.NotNull(result);
            Assert.Equal(existingAsset.AssetCode, result.AssetCode);
        }

        [Fact]
        public async Task UpdateAsset_NonExistingAsset_ThrowsArgumentException()
        {
            // Arrange
            var assetId = Guid.NewGuid();
            var updateRequest = new AssetUpdateRequest
            {
                AssetName = "Updated Asset",
                Specification = "Updated Specification",
                InstallDate = DateOnly.FromDateTime(DateTime.Now),
                Status = EnumAssetStatus.WaitingForRecycling
            };

            _unitOfWorkMock.Setup(r => r.AssetRepository.GetAsync(x => x.Id == assetId && !x.IsDeleted))
                .ReturnsAsync((Asset)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _assetService.UpdateAsset(assetId, updateRequest));

            _assetRepositoryMock.Verify(r => r.Update(It.IsAny<Asset>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        }
    }
}
