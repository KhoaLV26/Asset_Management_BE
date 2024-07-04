// ﻿using AssetManagement.Application.Models.Requests;
// using AssetManagement.Application.Models.Responses;
// using AssetManagement.Application.Services;
// using AssetManagement.Application.Services.Implementations;
// using AssetManagement.Domain.Entities;
// using AssetManagement.Domain.Enums;
// using AssetManagement.Domain.Interfaces;
// using AutoMapper;
// using Moq;
// using System;
// using System.Linq.Expressions;
// using System.Threading.Tasks;
// using Xunit;

// namespace AssetManagement.Test.Unit.AssignmentServiceTest
// {
//     public class CreateAssignmentTest
//     {
//         private readonly Mock<IUnitOfWork> _mockUnitOfWork;
//         private readonly Mock<IMapper> _mockMapper;
//         private readonly Mock<IAssetRepository> _mockAssetRepository;
//         private readonly Mock<IAssignmentRepository> _mockAssignmentRepository;
//         private readonly Mock<IAssetService> _mockAssetService;

//         public CreateAssignmentTest()
//         {
//             _mockUnitOfWork = new Mock<IUnitOfWork>();
//             _mockMapper = new Mock<IMapper>();
//             _mockAssetRepository = new Mock<IAssetRepository>();
//             _mockAssignmentRepository = new Mock<IAssignmentRepository>();
//             _mockAssetService = new Mock<IAssetService>();
//             _mockUnitOfWork.Setup(uow => uow.AssetRepository).Returns(_mockAssetRepository.Object);
//             _mockUnitOfWork.Setup(uow => uow.AssignmentRepository).Returns(_mockAssignmentRepository.Object);
//         }

//         [Fact]
//         public async Task AddAssignmentAsync_WithValidRequest_ReturnsAssignmentResponse()
//         {
//             // Arrange
//             var assignmentService = new AssignmentService(_mockUnitOfWork.Object, _mockMapper.Object, _mockAssetService.Object);
//             var assetId = Guid.NewGuid();
//             var request = new AssignmentRequest
//             {
//                 AssignedTo = Guid.NewGuid(),
//                 AssignedBy = Guid.NewGuid(),
//                 AssignedDate = DateTime.UtcNow,
//                 AssetId = assetId,
//                 Note = "Test assignment"
//             };

//             var asset = new Asset { Id = assetId, Status = EnumAssetStatus.Available };
//             var assignment = new Assignment();
//             var assignmentResponse = new AssignmentResponse();

//             _mockAssetRepository.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<Asset, bool>>>()))
//                 .ReturnsAsync(asset);
//             _mockAssignmentRepository.Setup(repo => repo.AddAsync(It.IsAny<Assignment>()))
//                 .Returns(Task.CompletedTask);
//             _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(1);
//             _mockMapper.Setup(m => m.Map<AssignmentResponse>(It.IsAny<Assignment>()))
//                 .Returns(assignmentResponse);

//             // Act
//             var result = await assignmentService.AddAssignmentAsync(request);

//             // Assert
//             Assert.NotNull(result);
//             Assert.IsType<AssignmentResponse>(result);
//             _mockAssignmentRepository.Verify(repo => repo.AddAsync(It.IsAny<Assignment>()), Times.Once);
//             _mockUnitOfWork.Verify(uow => uow.CommitAsync(), Times.Exactly(2));
//             _mockAssetRepository.Verify(repo => repo.Update(It.Is<Asset>(a => a.Status == EnumAssetStatus.Assigned)), Times.Once);
//             Assert.Equal(EnumAssetStatus.Assigned, asset.Status);
//         }

//         [Fact]
//         public async Task AddAssignmentAsync_WithUnavailableAsset_ThrowsInvalidOperationException()
//         {
//             // Arrange
//             var assignmentService = new AssignmentService(_mockUnitOfWork.Object, _mockMapper.Object, _mockAssetService.Object);
//             var assetId = Guid.NewGuid();
//             var request = new AssignmentRequest
//             {
//                 AssignedTo = Guid.NewGuid(),
//                 AssignedBy = Guid.NewGuid(),
//                 AssignedDate = DateTime.UtcNow,
//                 AssetId = assetId,
//                 Note = "Test assignment"
//             };

//             var asset = new Asset { Id = assetId, Status = EnumAssetStatus.NotAvailable };

//             _mockAssetRepository.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<Asset, bool>>>()))
//                 .ReturnsAsync(asset);

//             // Act & Assert
//             await Assert.ThrowsAsync<ArgumentException>(() => assignmentService.AddAssignmentAsync(request));
//         }

//         [Fact]
//         public async Task AddAssignmentAsync_FailedToCommit_ThrowsInvalidOperationException()
//         {
//             // Arrange
//             var assignmentService = new AssignmentService(_mockUnitOfWork.Object, _mockMapper.Object, _mockAssetService.Object);
//             var assetId = Guid.NewGuid();
//             var request = new AssignmentRequest
//             {
//                 AssignedTo = Guid.NewGuid(),
//                 AssignedBy = Guid.NewGuid(),
//                 AssignedDate = DateTime.UtcNow,
//                 AssetId = assetId,
//                 Note = "Test assignment"
//             };

//             var asset = new Asset { Id = assetId, Status = EnumAssetStatus.Available };

//             _mockAssetRepository.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<Asset, bool>>>()))
//                 .ReturnsAsync(asset);
//             _mockAssignmentRepository.Setup(repo => repo.AddAsync(It.IsAny<Assignment>()))
//                 .Returns(Task.CompletedTask);
//             _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(0);

//             // Act & Assert
//             await Assert.ThrowsAsync<ArgumentException>(() => assignmentService.AddAssignmentAsync(request));
//         }
//     }
// }
