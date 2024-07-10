using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Services.Implementations;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Domain.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AssetManagement.Test.Unit.AssignmentServiceTest
{
    public class UpdateAssignmentTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly AssignmentService _assignmentService;

        public UpdateAssignmentTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _assignmentService = new AssignmentService(_mockUnitOfWork.Object, null, null);
        }

        [Fact]
        public async Task UpdateAssignment_AssignmentNotFound_ThrowsArgumentException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new AssignmentRequest();
            _mockUnitOfWork.Setup(uow => uow.AssignmentRepository.GetAsync(It.IsAny<Expression<Func<Assignment, bool>>>(), It.IsAny<Expression<Func<Assignment, object>>[]>()))
                .ReturnsAsync((Assignment)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _assignmentService.UpdateAssignment(id, request));
            Assert.Equal("Assignment not exist", ex.Message);
        }

        [Fact]
        public async Task UpdateAssignment_AssignedToUserNotFound_ThrowsArgumentException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new AssignmentRequest { AssignedTo = Guid.NewGuid() };
            _mockUnitOfWork.Setup(uow => uow.AssignmentRepository.GetAsync(It.IsAny<Expression<Func<Assignment, bool>>>(), It.IsAny<Expression<Func<Assignment, object>>[]>()))
                .ReturnsAsync(new Assignment());
            _mockUnitOfWork.Setup(uow => uow.UserRepository.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _assignmentService.UpdateAssignment(id, request));
            Assert.Equal("User does not exist!", ex.Message);
        }

        [Fact]
        public async Task UpdateAssignment_AssetNotFound_ThrowsArgumentException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new AssignmentRequest { AssetId = Guid.NewGuid() };
            _mockUnitOfWork.Setup(uow => uow.AssignmentRepository.GetAsync(It.IsAny<Expression<Func<Assignment, bool>>>(), It.IsAny<Expression<Func<Assignment, object>>[]>()))
                .ReturnsAsync(new Assignment());
            _mockUnitOfWork.Setup(uow => uow.AssetRepository.GetAsync(It.IsAny<Expression<Func<Asset, bool>>>()))
                .ReturnsAsync((Asset)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _assignmentService.UpdateAssignment(id, request));
            Assert.Equal("Asset does not exist!", ex.Message);
        }

        [Fact]
        public async Task UpdateAssignment_FailedToUpdateAssignment_ThrowsArgumentException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new AssignmentRequest { AssetId = Guid.NewGuid(), AssignedTo = Guid.NewGuid() };
            _mockUnitOfWork.Setup(uow => uow.AssignmentRepository.GetAsync(It.IsAny<Expression<Func<Assignment, bool>>>(), It.IsAny<Expression<Func<Assignment, object>>[]>()))
                .ReturnsAsync(new Assignment { AssetId = Guid.NewGuid() });
            _mockUnitOfWork.Setup(uow => uow.AssetRepository.GetAsync(It.IsAny<Expression<Func<Asset, bool>>>()))
                .ReturnsAsync(new Asset { Status = EnumAssetStatus.Available });
            _mockUnitOfWork.Setup(uow => uow.UserRepository.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(new User());
            _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(0);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _assignmentService.UpdateAssignment(id, request));
            Assert.Equal("An error occurred while updating assignment.", ex.Message);
        }

        [Fact]
        public async Task UpdateAssignment_FailedToUpdateOldAssetStatus_ThrowsArgumentException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new AssignmentRequest { AssetId = Guid.NewGuid(), AssignedTo = Guid.NewGuid() };
            _mockUnitOfWork.Setup(uow => uow.AssignmentRepository.GetAsync(It.IsAny<Expression<Func<Assignment, bool>>>(), It.IsAny<Expression<Func<Assignment, object>>[]>()))
                .ReturnsAsync(new Assignment { AssetId = Guid.NewGuid() });
            _mockUnitOfWork.Setup(uow => uow.AssetRepository.GetAsync(It.IsAny<Expression<Func<Asset, bool>>>()))
                .ReturnsAsync(new Asset { Status = EnumAssetStatus.Available });
            _mockUnitOfWork.Setup(uow => uow.UserRepository.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(new User());
            _mockUnitOfWork.SetupSequence(uow => uow.CommitAsync())
                .ReturnsAsync(1)  // Successful assignment update
                .ReturnsAsync(0); // Failed old asset status update

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _assignmentService.UpdateAssignment(id, request));
            Assert.Equal("The assignment was updated but failed to update old asset status.", ex.Message);
        }

        [Fact]
        public async Task UpdateAssignment_FailedToUpdateNewAssetStatus_ThrowsArgumentException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new AssignmentRequest { AssetId = Guid.NewGuid(), AssignedTo = Guid.NewGuid() };
            _mockUnitOfWork.Setup(uow => uow.AssignmentRepository.GetAsync(It.IsAny<Expression<Func<Assignment, bool>>>(), It.IsAny<Expression<Func<Assignment, object>>[]>()))
                .ReturnsAsync(new Assignment { AssetId = Guid.NewGuid() });
            _mockUnitOfWork.Setup(uow => uow.AssetRepository.GetAsync(It.IsAny<Expression<Func<Asset, bool>>>()))
                .ReturnsAsync(new Asset { Status = EnumAssetStatus.Available });
            _mockUnitOfWork.Setup(uow => uow.UserRepository.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(new User());
            _mockUnitOfWork.SetupSequence(uow => uow.CommitAsync())
                .ReturnsAsync(1)  // Successful assignment update
                .ReturnsAsync(1)  // Successful old asset status update
                .ReturnsAsync(0); // Failed new asset status update

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _assignmentService.UpdateAssignment(id, request));
            Assert.Equal("The assignment was updated but failed to update new asset status.", ex.Message);
        }

        [Fact]
        public async Task ResponseAssignment_AssignmentNotFound_ThrowsArgumentException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var accepted = "true";
            _mockUnitOfWork.Setup(uow => uow.AssignmentRepository.GetAsync(It.IsAny<Expression<Func<Assignment, bool>>>(), It.IsAny<Expression<Func<Assignment, object>>[]>()))
                .ReturnsAsync((Assignment)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _assignmentService.ResponseAssignment(id, userId, accepted));
            Assert.Equal("Assignment not exist", ex.Message);
        }

        [Fact]
        public async Task ResponseAssignment_UserNotFound_ThrowsArgumentException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var accepted = "true";
            _mockUnitOfWork.Setup(uow => uow.AssignmentRepository.GetAsync(It.IsAny<Expression<Func<Assignment, bool>>>(), It.IsAny<Expression<Func<Assignment, object>>[]>()))
                .ReturnsAsync(new Assignment { AssignedTo = userId });
            _mockUnitOfWork.Setup(uow => uow.UserRepository.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _assignmentService.ResponseAssignment(id, userId, accepted));
            Assert.Equal("User does not exist!", ex.Message);
        }

        [Fact]
        public async Task ResponseAssignment_NotAssignedToUser_ThrowsArgumentException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var accepted = "true";
            _mockUnitOfWork.Setup(uow => uow.AssignmentRepository.GetAsync(It.IsAny<Expression<Func<Assignment, bool>>>(), It.IsAny<Expression<Func<Assignment, object>>[]>()))
                .ReturnsAsync(new Assignment { AssignedTo = Guid.NewGuid() });
            _mockUnitOfWork.Setup(uow => uow.UserRepository.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(new User());

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _assignmentService.ResponseAssignment(id, userId, accepted));
            Assert.Equal("This is not your assignment!", ex.Message);
        }

        [Fact]
        public async Task ResponseAssignment_AssetNotFound_ThrowsArgumentException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var accepted = "true";
            _mockUnitOfWork.Setup(uow => uow.AssignmentRepository.GetAsync(It.IsAny<Expression<Func<Assignment, bool>>>(), It.IsAny<Expression<Func<Assignment, object>>[]>()))
                .ReturnsAsync(new Assignment { AssignedTo = userId });
            _mockUnitOfWork.Setup(uow => uow.UserRepository.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(new User());
            _mockUnitOfWork.Setup(uow => uow.AssetRepository.GetAsync(It.IsAny<Expression<Func<Asset, bool>>>()))
                .ReturnsAsync((Asset)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _assignmentService.ResponseAssignment(id, userId, accepted));
            Assert.Equal("Asset does not exist!", ex.Message);
        }

        [Fact]
        public async Task ResponseAssignment_AssetNotAvailable_ThrowsArgumentException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var accepted = "true";
            _mockUnitOfWork.Setup(uow => uow.AssignmentRepository.GetAsync(It.IsAny<Expression<Func<Assignment, bool>>>(), It.IsAny<Expression<Func<Assignment, object>>[]>()))
                .ReturnsAsync(new Assignment { AssignedTo = userId });
            _mockUnitOfWork.Setup(uow => uow.UserRepository.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(new User());
            _mockUnitOfWork.Setup(uow => uow.AssetRepository.GetAsync(It.IsAny<Expression<Func<Asset, bool>>>()))
                .ReturnsAsync(new Asset { Status = EnumAssetStatus.Assigned });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _assignmentService.ResponseAssignment(id, userId, accepted));
            Assert.Equal("Asset is not available!", ex.Message);
        }

        [Fact]
        public async Task ResponseAssignment_FailedToUpdateAssignment_ThrowsArgumentException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var accepted = "true";
            _mockUnitOfWork.Setup(uow => uow.AssignmentRepository.GetAsync(It.IsAny<Expression<Func<Assignment, bool>>>(), It.IsAny<Expression<Func<Assignment, object>>[]>()))
                .ReturnsAsync(new Assignment { AssignedTo = userId });
            _mockUnitOfWork.Setup(uow => uow.UserRepository.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(new User());
            _mockUnitOfWork.Setup(uow => uow.AssetRepository.GetAsync(It.IsAny<Expression<Func<Asset, bool>>>()))
                .ReturnsAsync(new Asset { Status = EnumAssetStatus.Available });
            _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(0);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _assignmentService.ResponseAssignment(id, userId, accepted));
            Assert.Equal("An error occurred while response to assignment!", ex.Message);
        }

        [Fact]
        public async Task ResponseAssignment_ValidRequest_ReturnsAssignmentResponse()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var accepted = "true";
            _mockUnitOfWork.Setup(uow => uow.AssignmentRepository.GetAsync(It.IsAny<Expression<Func<Assignment, bool>>>(), It.IsAny<Expression<Func<Assignment, object>>[]>()))
                .ReturnsAsync(new Assignment { Id = id, AssignedTo = userId });
            _mockUnitOfWork.Setup(uow => uow.UserRepository.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(new User());
            _mockUnitOfWork.Setup(uow => uow.AssetRepository.GetAsync(It.IsAny<Expression<Func<Asset, bool>>>()))
                .ReturnsAsync(new Asset { Status = EnumAssetStatus.Available });
            _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(1);

            // Act
            var result = await _assignmentService.ResponseAssignment(id, userId, accepted);

            // Assert
            Assert.Equal(id, result.Id);
        }

        [Fact]
        public async Task ResponseAssignment_AssetAvailable_ReturnsAssignmentResponse()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var accepted = "true";
            var assignment = new Assignment { Id = id, AssignedTo = userId };
            var user = new User();
            var asset = new Asset { Status = EnumAssetStatus.Available };

            _mockUnitOfWork.Setup(uow => uow.AssignmentRepository.GetAsync(It.IsAny<Expression<Func<Assignment, bool>>>(), It.IsAny<Expression<Func<Assignment, object>>[]>()))
                .ReturnsAsync(assignment);
            _mockUnitOfWork.Setup(uow => uow.UserRepository.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(user);
            _mockUnitOfWork.Setup(uow => uow.AssetRepository.GetAsync(It.IsAny<Expression<Func<Asset, bool>>>()))
                .ReturnsAsync(asset);
            _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(1);

            // Act
            var result = await _assignmentService.ResponseAssignment(id, userId, accepted);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.Equal(EnumAssignmentStatus.Accepted, assignment.Status);
            _mockUnitOfWork.Verify(uow => uow.AssignmentRepository.Update(It.IsAny<Assignment>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task ResponseAssignment_DeclineAssignment_ReturnsAssignmentResponse()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var accepted = "false";
            var assignment = new Assignment { Id = id, AssignedTo = userId };
            var user = new User();
            var asset = new Asset { Status = EnumAssetStatus.Available };

            _mockUnitOfWork.Setup(uow => uow.AssignmentRepository.GetAsync(It.IsAny<Expression<Func<Assignment, bool>>>(), It.IsAny<Expression<Func<Assignment, object>>[]>()))
                .ReturnsAsync(assignment);
            _mockUnitOfWork.Setup(uow => uow.UserRepository.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(user);
            _mockUnitOfWork.Setup(uow => uow.AssetRepository.GetAsync(It.IsAny<Expression<Func<Asset, bool>>>()))
                .ReturnsAsync(asset);
            _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(1);

            // Act
            var result = await _assignmentService.ResponseAssignment(id, userId, accepted);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.Equal(EnumAssignmentStatus.Declined, assignment.Status);
            _mockUnitOfWork.Verify(uow => uow.AssignmentRepository.Update(It.IsAny<Assignment>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(), Times.Once);
        }
    }
}

