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
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AssetManagement.Test.Unit.AssignmentServiceTest
{
    public class CreateAssignmentTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly AssignmentService _assignmentService;

        public CreateAssignmentTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _assignmentService = new AssignmentService(_unitOfWorkMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task AddAssignmentAsync_ValidRequest_ShouldAddAssignment()
        {
            // Arrange
            var request = new AssignmentRequest
            {
                AssignedTo = Guid.NewGuid(),
                AssignedBy = Guid.NewGuid(),
                AssignedDate = DateTime.UtcNow,
                AssetId = Guid.NewGuid(),
                Note = "Test assignment"
            };

            var expectedAssignment = new Assignment
            {
                Id = Guid.NewGuid(),
                AssignedTo = request.AssignedTo,
                AssignedBy = request.AssignedBy,
                AssignedDate = request.AssignedDate,
                AssetId = request.AssetId,
                Status = EnumAssignmentStatus.WaitingForAcceptance,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.AssignedBy,
                Note = request.Note
            };

            var expectedResponse = new AssignmentResponse
            {
                Id = expectedAssignment.Id,
                AssignedTo = expectedAssignment.AssignedTo,
                AssignedBy = expectedAssignment.AssignedBy,
                AssignedDate = expectedAssignment.AssignedDate,
                AssetId = expectedAssignment.AssetId,
                Status = expectedAssignment.Status
            };

            _unitOfWorkMock.Setup(uow => uow.AssignmentRepository.AddAsync(It.IsAny<Assignment>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.CommitAsync())
                .ReturnsAsync(1);
            _mapperMock.Setup(mapper => mapper.Map<AssignmentResponse>(It.IsAny<Assignment>()))
                .Returns(expectedResponse);

            // Act
            var result = await _assignmentService.AddAssignmentAsync(request);

            // Assert
            Assert.Equal(expectedResponse.Id, result.Id);
            Assert.Equal(expectedResponse.AssignedTo, result.AssignedTo);
            Assert.Equal(expectedResponse.AssignedBy, result.AssignedBy);
            Assert.Equal(expectedResponse.AssignedDate, result.AssignedDate);
            Assert.Equal(expectedResponse.AssetId, result.AssetId);
            Assert.Equal(expectedResponse.Status, result.Status);
            _unitOfWorkMock.Verify(uow => uow.AssignmentRepository.AddAsync(It.IsAny<Assignment>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task AddAssignmentAsync_CommitFails_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var request = new AssignmentRequest
            {
                AssignedTo = Guid.NewGuid(),
                AssignedBy = Guid.NewGuid(),
                AssignedDate = DateTime.UtcNow,
                AssetId = Guid.NewGuid(),
                Note = "Test assignment"
            };

            _unitOfWorkMock.Setup(uow => uow.AssignmentRepository.AddAsync(It.IsAny<Assignment>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.CommitAsync())
                .ReturnsAsync(0);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _assignmentService.AddAssignmentAsync(request));
            _unitOfWorkMock.Verify(uow => uow.AssignmentRepository.AddAsync(It.IsAny<Assignment>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
        }
    }
}
