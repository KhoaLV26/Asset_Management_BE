using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Application.Services;
using AssetManagement.Application.Services.Implementations;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Domain.Interfaces;
using AutoMapper;
using Moq;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace AssetManagement.Test.Unit.AssignmentServiceTest
{
    public class CreateAssignmentTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IAssetRepository> _assetRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IAssignmentRepository> _assignmentRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly AssignmentService _assignmentService;
        private readonly Mock<IAssetService> _assetServiceMock;

        public CreateAssignmentTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _assetRepositoryMock = new Mock<IAssetRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _assignmentRepositoryMock = new Mock<IAssignmentRepository>();
            _assetServiceMock = new Mock<IAssetService>();
            _mapperMock = new Mock<IMapper>();

            _unitOfWorkMock.Setup(u => u.AssetRepository).Returns(_assetRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.UserRepository).Returns(_userRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.AssignmentRepository).Returns(_assignmentRepositoryMock.Object);

            _assignmentService = new AssignmentService(_unitOfWorkMock.Object, _mapperMock.Object, _assetServiceMock.Object);
        }

        [Fact]
        public async Task AddAssignmentAsync_AssetNotAvailable_ThrowsArgumentException()
        {
            // Arrange
            var request = new AssignmentRequest { AssetId = Guid.NewGuid(), AssignedTo = Guid.NewGuid(), AssignedBy = Guid.NewGuid() };
            var asset = new Asset { Id = request.AssetId, Status = EnumAssetStatus.Assigned };

            _assetRepositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Asset, bool>>>()))
                .ReturnsAsync(asset);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _assignmentService.AddAssignmentAsync(request));
            Assert.Equal("The asset is not available for assignment.", exception.Message);
        }

        [Fact]
        public async Task AddAssignmentAsync_AssetDeleted_ThrowsArgumentException()
        {
            // Arrange
            var request = new AssignmentRequest { AssetId = Guid.NewGuid(), AssignedTo = Guid.NewGuid(), AssignedBy = Guid.NewGuid() };
            var asset = new Asset { Id = request.AssetId, Status = EnumAssetStatus.Available, IsDeleted = true };

            _assetRepositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Asset, bool>>>()))
                .ReturnsAsync(asset);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _assignmentService.AddAssignmentAsync(request));
            Assert.Equal("The asset is deleted.", exception.Message);
        }

        [Fact]
        public async Task AddAssignmentAsync_UserDeleted_ThrowsArgumentException()
        {
            // Arrange
            var request = new AssignmentRequest { AssetId = Guid.NewGuid(), AssignedTo = Guid.NewGuid(), AssignedBy = Guid.NewGuid() };
            var asset = new Asset { Id = request.AssetId, Status = EnumAssetStatus.Available, IsDeleted = false };
            var assignTo = new User { Id = request.AssignedTo, IsDeleted = true };
            var assignBy = new User { Id = request.AssignedBy, IsDeleted = false };

            _assetRepositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Asset, bool>>>()))
                .ReturnsAsync(asset);
            _userRepositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User)null);
            _userRepositoryMock.Setup(x => x.GetAsync(x => x.Id == request.AssignedTo)).ReturnsAsync(assignTo);
            _userRepositoryMock.Setup(x => x.GetAsync(x => x.Id == request.AssignedBy)).ReturnsAsync(assignBy);
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _assignmentService.AddAssignmentAsync(request));
            Assert.Equal("The user is disabled.", exception.Message);
        }

        [Fact]
        public async Task AddAssignmentAsync_ValidRequest_AddsAssignmentAndUpdatesAsset()
        {
            // Arrange
            var request = new AssignmentRequest
            {
                AssetId = Guid.NewGuid(),
                AssignedTo = Guid.NewGuid(),
                AssignedBy = Guid.NewGuid(),
                AssignedDate = DateTime.UtcNow,
                Note = "Test note"
            };
            var asset = new Asset { Id = request.AssetId, Status = EnumAssetStatus.Available, IsDeleted = false };
            var assignTo = new User { Id = request.AssignedTo, IsDeleted = false };
            var assignBy = new User { Id = request.AssignedBy, IsDeleted = false };
            var assignment = new Assignment
            {
                Id = Guid.NewGuid(),
                AssignedTo = request.AssignedTo,
                AssignedBy = request.AssignedBy,
                AssignedDate = request.AssignedDate,
                AssetId = asset.Id,
                Status = EnumAssignmentStatus.WaitingForAcceptance,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.AssignedBy,
                Note = request.Note
            };
            var assignmentResponse = new AssignmentResponse();

            _assetRepositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Asset, bool>>>()))
                .ReturnsAsync(asset);
            _userRepositoryMock.SetupSequence(r => r.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(assignTo)
                .ReturnsAsync(assignBy);
            _assignmentRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Assignment>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CommitAsync())
                .ReturnsAsync(1);
            _mapperMock.Setup(m => m.Map<AssignmentResponse>(It.IsAny<Assignment>()))
                .Returns(assignmentResponse);
            _userRepositoryMock.Setup(x => x.GetAsync(x => x.Id == request.AssignedTo)).ReturnsAsync(assignTo);
            _userRepositoryMock.Setup(x => x.GetAsync(x => x.Id == request.AssignedBy)).ReturnsAsync(assignBy);
            // Act
            var result = await _assignmentService.AddAssignmentAsync(request);

            // Assert
            _assignmentRepositoryMock.Verify(r => r.AddAsync(It.Is<Assignment>(a => a.AssetId == request.AssetId)), Times.Once);
            _assetRepositoryMock.Verify(r => r.Update(It.Is<Asset>(a => a.Status == EnumAssetStatus.Assigned)), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Exactly(2));
            Assert.Equal(assignmentResponse, result);
        }
    }
}
