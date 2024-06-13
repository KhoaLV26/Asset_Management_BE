using AssetManagement.Application.Models.Responses;
using AssetManagement.Application.Services.Implementations;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Domain.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;


namespace AssetManagement.Test.Unit.AssetServiceTest
{
    public class GetAssetDetailTest
    {
        public class AssetServiceTests
        {
            private readonly Mock<IUnitOfWork> _mockUnitOfWork;
            private readonly Mock<IAssetRepository> _mockAssetRepository;
            private readonly AssetService _assetService;

            public AssetServiceTests()
            {
                _mockUnitOfWork = new Mock<IUnitOfWork>();
                _mockAssetRepository = new Mock<IAssetRepository>();
                _mockUnitOfWork.Setup(u => u.AssetRepository).Returns(_mockAssetRepository.Object);

                _assetService = new AssetService(_mockUnitOfWork.Object);
            }

            [Fact]
            public async Task GetAssetByIdAsync_Returns_AssetDetailResponse_When_AssetExists()
            {
                // Arrange
                var assignerId = Guid.NewGuid();
                var assignedId = Guid.NewGuid();
                var categoryId = Guid.NewGuid();
                var assetId = Guid.NewGuid();

                var existingAsset = new Asset
                {
                    Id = assetId,
                    AssetName = "Test Asset",
                    AssetCode = "ABC123",
                    CategoryId = categoryId,
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
                Status = EnumAssignmentStatus.Accepted
            }
        }
                };

                var unitOfWorkMock = new Mock<IUnitOfWork>();
                var assetRepositoryMock = new Mock<IAssetRepository>();

                unitOfWorkMock.Setup(uow => uow.AssetRepository).Returns(assetRepositoryMock.Object);
                assetRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<Asset, bool>>>(), It.Is<Expression<Func<Asset, object>>>(expr => expr.ToString().Contains("Assignments"))))
                    .ReturnsAsync(existingAsset);

                var service = new AssetService(unitOfWorkMock.Object);

                // Act
                var result = await service.GetAssetByIdAsync(existingAsset.Id);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(existingAsset.AssetName, result.AssetName);
    Assert.Equal(existingAsset.AssetCode, result.AssetCode);
    Assert.Equal(existingAsset.CategoryId, result.CategoryId);
    Assert.Equal(existingAsset.Status, result.Status);
    Assert.NotEmpty(result.AssignmentResponses);
    //Assert.Equal(existingAsset.Assignments.Count, result.AssignmentResponses.Count);
    Assert.Equal(existingAsset.Assignments.First().Id, result.AssignmentResponses.First().Id);
    Assert.Equal(existingAsset.Assignments.First().AssetId, result.AssignmentResponses.First().AssetId);
    Assert.Equal(existingAsset.Assignments.First().AssignedBy, result.AssignmentResponses.First().AssignedBy);
    Assert.Equal(existingAsset.Assignments.First().AssignedTo, result.AssignmentResponses.First().AssignedTo);
    Assert.Equal(existingAsset.Assignments.First().AssignedDate, result.AssignmentResponses.First().AssignedDate);
    Assert.Equal(existingAsset.Assignments.First().Status, result.AssignmentResponses.First().Status);
}

            [Fact]
            public async Task GetAssetByIdAsync_Returns_Null_When_AssetDoesNotExist()
            {
                // Arrange
                var unitOfWorkMock = new Mock<IUnitOfWork>();
                var assetRepositoryMock = new Mock<IAssetRepository>();

                unitOfWorkMock.Setup(uow => uow.AssetRepository).Returns(assetRepositoryMock.Object);
                assetRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<Asset, bool>>>(), It.IsAny<Expression<Func<Asset, object>>[]>())).ReturnsAsync((Asset)null);


                var service = new AssetService(unitOfWorkMock.Object);

                // Act
                var result = await service.GetAssetByIdAsync(Guid.NewGuid());

                // Assert
                Assert.Null(result);
            }
        }
    }
}