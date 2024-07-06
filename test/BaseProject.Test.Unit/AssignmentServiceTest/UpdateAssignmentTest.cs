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
    }
}

