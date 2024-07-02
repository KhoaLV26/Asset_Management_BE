using AssetManagement.Application.Services.Implementations;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Domain.Interfaces;
using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;


namespace AssetManagement.Test.Unit.AssetServiceTest
{
    public class GetAssetDetailTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IAssetRepository> _mockAssetRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly AssetService _assetService;

        public GetAssetDetailTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockAssetRepository = new Mock<IAssetRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockUnitOfWork.Setup(u => u.AssetRepository).Returns(_mockAssetRepository.Object);

            _assetService = new AssetService(_mockUnitOfWork.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetAssetByIdAsync_Returns_AssetDetailResponse_When_AssetExists()
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

            var existingAsset = new Asset
            {
                Id = assetId,
                AssetName = "Test Asset",
                AssetCode = "ABC123",
                CategoryId = categoryId,
                Category = category,
                Status = EnumAssetStatus.Available,
                InstallDate = DateOnly.FromDateTime(DateTime.UtcNow),
                LocationId = locationId,
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

            _mockAssetRepository.Setup(repo => repo.GetAssetDetail(assetId))
                .ReturnsAsync(existingAsset);

            // Act
            var result = await _assetService.GetAssetByIdAsync(assetId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingAsset.AssetName, result.AssetName);
            Assert.Equal(existingAsset.AssetCode, result.AssetCode);
            Assert.Equal(existingAsset.CategoryId, result.CategoryId);
            Assert.Equal(existingAsset.Status, result.Status);
            Assert.NotEmpty(result.AssignmentResponses);
            Assert.Equal(existingAsset.Assignments.Count, result.AssignmentResponses.Count());
            Assert.Equal(existingAsset.Assignments.First().Id, result.AssignmentResponses.First().Id);
            Assert.Equal(existingAsset.Assignments.First().AssetId, result.AssignmentResponses.First().AssetId);
            Assert.Equal(existingAsset.Assignments.First().AssignedBy, result.AssignmentResponses.First().AssignedBy);
            Assert.Equal(existingAsset.Assignments.First().AssignedTo, result.AssignmentResponses.First().AssignedTo);
            Assert.Equal(existingAsset.Assignments.First().AssignedDate, result.AssignmentResponses.First().AssignedDate);
            Assert.Equal(existingAsset.Assignments.First().Status, result.AssignmentResponses.First().Status);
            Assert.Equal(existingAsset.Assignments.First().UserBy.Username, result.AssignmentResponses.First().By);
            Assert.Equal(existingAsset.Assignments.First().UserTo.Username, result.AssignmentResponses.First().To);
        }


        [Fact]
        public async Task GetAssetByIdAsync_Returns_Null_When_AssetDoesNotExist()
        {
            // Arrange
            var assetId = Guid.NewGuid();
            _mockAssetRepository.Setup(repo => repo.GetAssetDetail(assetId))
                .ReturnsAsync((Asset)null);

            // Act
            var result = await _assetService.GetAssetByIdAsync(assetId);

            // Assert
            Assert.Null(result);
        }
    }
}
