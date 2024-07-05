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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AssetManagement.Test.Unit.AssetServiceTest
{
    public class DeleteAssetTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IAssetRepository> _mockAssetRepository;
        private readonly Mock<IMapper> _mapperMock;
        private readonly AssetService _assetService;

        public DeleteAssetTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _assetService = new AssetService(_unitOfWorkMock.Object, _mapperMock.Object);
            _mockAssetRepository = new Mock<IAssetRepository>();
            _unitOfWorkMock.Setup(u => u.AssetRepository).Returns(_mockAssetRepository.Object);

        }

        [Fact]
        public async Task DeleteAssetAsync_AssetNotFound_ThrowsException()
        {
            // Arrange
            var assetId = Guid.NewGuid();

            _unitOfWorkMock.Setup(uow => uow.AssetRepository.GetAsync(It.IsAny<Expression<Func<Asset, bool>>>()))
                .ReturnsAsync((Asset)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _assetService.DeleteAssetAsync(assetId));
            Assert.Equal("Asset not found", exception.Message);
        }

        [Fact]
        public async Task DeleteAssetAsync_AssetWithHistoricalAssignments_ThrowsException()
        {
            // Arrange
            var assignerId = Guid.NewGuid();
            var assignedId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var assetId = Guid.NewGuid();

            var category = new Category
            {
                Id = categoryId,
                Name = "Cate",
                Code = "CA"
            };

            var asset = new Asset
            {
                Id = assetId,
                AssetName = "Test Asset",
                AssetCode = "ABC123",
                CategoryId = categoryId,
                Category = category,
                LocationId = Guid.NewGuid(),
                Status = EnumAssetStatus.Available,
                Assignments = new List<Assignment>
                    {
                        new Assignment
                        {
                            Id = Guid.NewGuid(),
                            AssetId = assetId,
                            AssignedBy = assignerId,
                            AssignedTo = assignedId,
                            AssignedDate = DateTime.UtcNow,
                            Status = EnumAssignmentStatus.Accepted,
                            UserBy = new User { Username = "Assigner" },
                            UserTo = new User { Username = "Assignee" }
                        }
                    }
            };

            var assetDetailResponse = new AssetDetailResponse
            {
                Id = assetId,
                AssignmentResponses = new List<AssignmentResponse>
                {
                    new AssignmentResponse
                    {
                        Id = Guid.NewGuid()
                    }
                }
            };

            _mockAssetRepository.Setup(repo => repo.GetAssetDetail(assetId))
                .ReturnsAsync(asset);


            _unitOfWorkMock.Setup(uow => uow.AssetRepository.GetAsync(It.IsAny<Expression<Func<Asset, bool>>>()))
                .ReturnsAsync(asset);

            _unitOfWorkMock.Setup(uow => uow.CommitAsync())
                .ReturnsAsync(1);

            _unitOfWorkMock.Setup(uow => uow.AssetRepository.SoftDelete(asset));

            _mapperMock.Setup(mapper => mapper.Map<AssetDetailResponse>(asset))
                .Returns(assetDetailResponse);



            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _assetService.DeleteAssetAsync(assetId));
            Assert.Equal("This asset have historical assignment", exception.Message);
        }

        [Fact]
        public async Task DeleteAssetAsync_SuccessfulDeletion_ReturnsDeletedAssetResponse()
        {
            // Arrange
            var assignerId = Guid.NewGuid();
            var assignedId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var assetId = Guid.NewGuid();

            var category = new Category
            {
                Id = categoryId,
                Name = "Cate",
                Code = "CA"
            };

            var asset = new Asset
            {
                Id = assetId,
                AssetName = "Test Asset",
                AssetCode = "ABC123",
                CategoryId = categoryId,
                Category = category,
                Status = EnumAssetStatus.Available,
                LocationId = Guid.NewGuid(),
                Assignments = new List<Assignment>
                {
                    new Assignment
                    {
                        UserBy = new User
                        {
                            Username = "A"
                        },
                        UserTo = new User
                        {
                            Username = "B"
                        }
                    }
                }       
            };

            var assetDetailResponse = new AssetDetailResponse
            {
                Id = assetId,
                AssignmentResponses = Enumerable.Empty<AssignmentResponse>()
            };

            _mockAssetRepository.Setup(repo => repo.GetAssetDetail(assetId))
                .ReturnsAsync(asset);


            _unitOfWorkMock.Setup(uow => uow.AssetRepository.GetAsync(It.IsAny<Expression<Func<Asset, bool>>>()))
                .ReturnsAsync(asset);

            _unitOfWorkMock.Setup(uow => uow.CommitAsync())
                .ReturnsAsync(1);

            _unitOfWorkMock.Setup(uow => uow.AssetRepository.SoftDelete(asset));

            _mapperMock.Setup(mapper => mapper.Map<AssetDetailResponse>(asset))
                .Returns(assetDetailResponse);


            _mapperMock.Setup(mapper => mapper.Map<AssetResponse>(asset))
                .Returns(new AssetResponse { Id = assetId });


            // Act
            var result = await _assetService.DeleteAssetAsync(assetId);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<AssetResponse>(result);
            Assert.Equal(assetId, result.Id);

            _unitOfWorkMock.Verify(uow => uow.AssetRepository.SoftDelete(It.IsAny<Asset>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAssetAsync_FailedCommit_ThrowsException()
        {
            // Arrange
            var assignerId = Guid.NewGuid();
            var assignedId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            var locationId = Guid.NewGuid();

            var category = new Category
            {
                Id = categoryId,
                Name = "Cate",
                Code = "CA"
            };

            var asset = new Asset
            {
                Id = assetId,
                AssetName = "Test Asset",
                AssetCode = "ABC123",
                CategoryId = categoryId,
                Category = category,
                Status = EnumAssetStatus.Available,
                InstallDate = DateOnly.FromDateTime(DateTime.UtcNow),
                LocationId = locationId,
                Assignments = new List<Assignment>()
            };

            _mockAssetRepository.Setup(repo => repo.GetAssetDetail(assetId))
                .ReturnsAsync(asset);

            _unitOfWorkMock.Setup(uow => uow.AssetRepository.GetAsync(It.IsAny<Expression<Func<Asset, bool>>>()))
                .ReturnsAsync(asset);

            _unitOfWorkMock.Setup(uow => uow.CommitAsync())
                .ReturnsAsync(0);

            _unitOfWorkMock.Setup(uow => uow.AssetRepository.SoftDelete(asset));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _assetService.DeleteAssetAsync(assetId));
            Assert.Equal("Failed to delete asset", exception.Message);

            _unitOfWorkMock.Verify(uow => uow.AssetRepository.SoftDelete(It.IsAny<Asset>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
        }
    }
}
