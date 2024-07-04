// ﻿using AssetManagement.Application.Services;
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
//     public class DeleteAssignmentTest
//     {
//         private readonly Mock<IUnitOfWork> _mockUnitOfWork;
//         private readonly Mock<IMapper> _mockMapper;
//         private readonly Mock<IAssetRepository> _mockAssetRepository;
//         private readonly Mock<IAssignmentRepository> _mockAssignmentRepository;
//         private readonly Mock<IAssetService> _mockAssetService;


//         public DeleteAssignmentTest()
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
//         public async Task DeleteAssignment_WhenAssignmentNotFound_ReturnsFalse()
//         {
//             // Arrange
//             var assignmentService = new AssignmentService(_mockUnitOfWork.Object, _mockMapper.Object, _mockAssetService.Object);
//             var id = Guid.NewGuid();
//             _mockUnitOfWork.Setup(x => x.AssignmentRepository.GetAsync(It.IsAny<Expression<Func<Assignment, bool>>>(), It.IsAny<Expression<Func<Assignment, object>>[]>())).ReturnsAsync((Assignment)null);

//             // Act
//             var result = await assignmentService.DeleteAssignment(id);

//             // Assert
//             Assert.False(result);
//         }

//         [Fact]
//         public async Task DeleteAssignment_WhenSuccess_ReturnsTrue()
//         {
//             // Arrange
//             var assignmentService = new AssignmentService(_mockUnitOfWork.Object, _mockMapper.Object, _mockAssetService.Object);
//             var id = Guid.NewGuid();
//             var assignment = new Assignment
//             {
//                 Id = id,
//                 IsDeleted = false,
//                 Status = EnumAssignmentStatus.WaitingForAcceptance,
//                 Asset = new Asset
//                 {
//                     Id = Guid.NewGuid(),
//                     Status = EnumAssetStatus.Assigned
//                 }
//             };
//             _mockUnitOfWork.Setup(x => x.AssignmentRepository.GetAsync(It.IsAny<Expression<Func<Assignment, bool>>>(), It.IsAny<Expression<Func<Assignment, object>>[]>())).ReturnsAsync(assignment);
//             _mockUnitOfWork.Setup(x => x.AssignmentRepository.SoftDelete(assignment)).Verifiable();
//             _mockUnitOfWork.Setup(x => x.CommitAsync()).ReturnsAsync(1);

//             // Act
//             var result = await assignmentService.DeleteAssignment(id);

//             // Assert
//             Assert.True(result);
//         }
//     }
// }
