using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Services.Implementations;
using AssetManagement.Domain.Entities;
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

        public EditAssetTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
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
                Status = Domain.Enums.EnumAssetStatus.Available
            };
            var updateRequest = new AssetUpdateRequest
            {
                AssetName = "Updated Asset",
                Specification = "Updated Specification",
                InstallDate = DateOnly.FromDateTime(DateTime.Now),
                Status = Domain.Enums.EnumAssetStatus.WaitingForRecycling
            };

            _unitOfWorkMock.Setup(r => r.AssetRepository.GetAsync(x => x.Id == assetId && !x.IsDeleted))
                .ReturnsAsync(existingAsset);

            // Act
            var result = await _assetService.UpdateAsset(assetId, updateRequest);

            // Assert
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);

            Assert.NotNull(result);
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
                Status = Domain.Enums.EnumAssetStatus.WaitingForRecycling
            };

            _unitOfWorkMock.Setup(r => r.AssetRepository.GetAsync(x => x.Id == assetId && !x.IsDeleted))
                .ReturnsAsync((Asset)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _assetService.UpdateAsset(assetId, updateRequest));

            _unitOfWorkMock.Verify(r => r.AssetRepository.GetAsync(x => x.Id == assetId && !x.IsDeleted), Times.Once);
            _unitOfWorkMock.Verify(r => r.AssetRepository.Update(It.IsAny<Asset>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        }
    }
}
