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
    public class AdminEditAssetTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly AssetService _assetService;

        public AdminEditAssetTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _assetService = new AssetService(_unitOfWorkMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task UpdateAsset_WithValidData_ShouldUpdateAsset()
        {
            // Arrange
            var assetId = Guid.NewGuid();
            var assetCode = "A";
            var assetRequest = new AssetUpdateRequest
            {
                AssetName = "Updated Asset",
                Specification = "Updated Specification",
                InstallDate = DateOnly.FromDateTime(DateTime.Now),
                Status = EnumAssetStatus.Available
            };

            var currentAsset = new Asset { Id = assetId, AssetCode = assetCode };
            _unitOfWorkMock.Setup(u => u.AssetRepository.GetAsync(x => x.Id == assetId && !x.IsDeleted)).ReturnsAsync(currentAsset);
            _unitOfWorkMock.Setup(u => u.CommitAsync()).ReturnsAsync(1);

            // Act
            var result = await _assetService.UpdateAsset(assetId, assetRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(assetCode, result.AssetCode);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsset_WithInvalidAssetId_ShouldThrowArgumentException()
        {
            // Arrange
            var assetId = Guid.NewGuid();
            var assetRequest = new AssetUpdateRequest();

            _unitOfWorkMock.Setup(u => u.AssetRepository.GetAsync(x => x.Id == assetId)).ReturnsAsync((Asset)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _assetService.UpdateAsset(assetId, assetRequest));
        }
    }
}
