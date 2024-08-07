using AssetManagement.Application.Models.Responses;
using AssetManagement.Application.Services;
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
using System.Threading.Tasks;
using Xunit;

namespace AssetManagement.Test.Unit.AssignmentServiceTest
{
    public class GetUserAssignmentTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IAssetRepository> _mockAssetRepository;
        private readonly Mock<IAssignmentRepository> _mockAssignmentRepository;
        private readonly Mock<IAssetService> _mockAssetService;
        private readonly AssignmentService _assignmentService;

        public GetUserAssignmentTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockAssetRepository = new Mock<IAssetRepository>();
            _mockAssignmentRepository = new Mock<IAssignmentRepository>();
            _mockAssetService = new Mock<IAssetService>();
            _mockUnitOfWork.Setup(uow => uow.AssetRepository).Returns(_mockAssetRepository.Object);
            _mockUnitOfWork.Setup(uow => uow.AssignmentRepository).Returns(_mockAssignmentRepository.Object);
            _assignmentService = new AssignmentService(_mockUnitOfWork.Object, _mockMapper.Object, _mockAssetService.Object);
        }

        [Fact]
        public async Task GetUserFilterQuery_ValidUserId_ReturnsCorrectExpression()
        {
            // Arrange
            var assignmentService = new AssignmentService(_mockUnitOfWork.Object, _mockMapper.Object, _mockAssetService.Object);

            var userId = Guid.NewGuid();
            var today = DateTime.Today;

            // Act
            var filter = await assignmentService.GetUserFilterQuery(userId);

            // Assert
            Assert.NotNull(filter);
            var assignment1 = new Assignment
            {
                IsDeleted = false,
                AssignedTo = userId,
                AssignedDate = today,
                Status = EnumAssignmentStatus.Accepted
            };
            var assignment2 = new Assignment
            {
                IsDeleted = false,
                AssignedTo = userId,
                AssignedDate = today,
                Status = EnumAssignmentStatus.WaitingForAcceptance
            };
            var assignment3 = new Assignment
            {
                IsDeleted = false,
                AssignedTo = userId,
                AssignedDate = today.AddDays(1),
                Status = EnumAssignmentStatus.Accepted
            };
            var assignment4 = new Assignment
            {
                IsDeleted = false,
                AssignedTo = userId,
                AssignedDate = today,
                Status = EnumAssignmentStatus.Declined
            };
            Assert.True(filter.Compile()(assignment1));
            Assert.True(filter.Compile()(assignment2));
            Assert.False(filter.Compile()(assignment3));
            Assert.False(filter.Compile()(assignment4));
        }

        [Fact]
        public async Task GetUserFilterQuery_WithDeletedAssignments_ReturnsNoAssignments()
        {
            // Arrange
            var assignmentService = new AssignmentService(_mockUnitOfWork.Object, _mockMapper.Object, _mockAssetService.Object);
            var userId = Guid.NewGuid();
            var today = DateTime.Today;

            // Act
            var filter = await assignmentService.GetUserFilterQuery(userId);

            // Assert
            Assert.NotNull(filter);
            var assignment1 = new Assignment
            {
                IsDeleted = true,
                AssignedTo = userId,
                AssignedDate = today,
                Status = EnumAssignmentStatus.Accepted
            };
            Assert.False(filter.Compile()(assignment1));
        }

        [Fact]
        public async Task GetUserAssignmentAsync_NoAssignments_ReturnsEmpty()
        {
            // Arrange
            var assignmentService = new AssignmentService(_mockUnitOfWork.Object, _mockMapper.Object, _mockAssetService.Object);
            var userId = Guid.NewGuid();
            _mockUnitOfWork.Setup(u => u.AssignmentRepository.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<Expression<Func<Assignment, bool>>>(),
                It.IsAny<Func<IQueryable<Assignment>, IOrderedQueryable<Assignment>>>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Assignment, bool>>>(),It.IsAny<int>()))
                .ReturnsAsync((new List<Assignment>(), 0));


            // Act
            var result = await assignmentService.GetUserAssignmentAsync(1, userId, It.IsAny<Guid>());


            // Assert
            Assert.NotNull(result.data);
            Assert.Empty(result.data);
        }

        [Fact]
        public async Task GetUserAssignmentAsync_InvalidPageNumber_ReturnsEmpty()
        {
            // Arrange
            var assignmentService = new AssignmentService(_mockUnitOfWork.Object, _mockMapper.Object, _mockAssetService.Object);
            var userId = Guid.NewGuid();
            var today = DateTime.Today;
            var assignments = new List<Assignment> { };

            _mockUnitOfWork.Setup(u => u.AssignmentRepository.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<Expression<Func<Assignment, bool>>>(),
                It.IsAny<Func<IQueryable<Assignment>, IOrderedQueryable<Assignment>>>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Assignment, bool>>>(), It.IsAny<int>()))
                .ReturnsAsync((assignments,0));

            _mockUnitOfWork.Setup(u => u.ReturnRequestRepository.GetAllAsync(
                    It.IsAny<int>(),
                    It.IsAny<Expression<Func<ReturnRequest, bool>>>(),
                    It.IsAny<Func<IQueryable<ReturnRequest>, IOrderedQueryable<ReturnRequest>>>(),
                    It.IsAny<string>(),
                    It.IsAny<Expression<Func<ReturnRequest, bool>>>(), It.IsAny<int>()))
                .ReturnsAsync((new List<ReturnRequest>(), 0));
            // Act
            var result = await assignmentService.GetUserAssignmentAsync(2, null, userId); // Page 2, but only one assignment


            // Assert
            Assert.NotNull(result.data);
            Assert.Empty(result.data);
        }

        [Fact]
        public async Task GetUserFilterQuery_WithInvalidUserId_ReturnsNoAssignments()
        {
            // Arrange
            var assignmentService = new AssignmentService(_mockUnitOfWork.Object, _mockMapper.Object, _mockAssetService.Object);
            var userId = Guid.NewGuid();
            var invalidUserId = Guid.NewGuid();
            var today = DateTime.Today;

            // Act
            var filter = await assignmentService.GetUserFilterQuery(invalidUserId);

            // Assert
            Assert.NotNull(filter);
            var assignment1 = new Assignment
            {
                IsDeleted = false,
                AssignedTo = userId,
                AssignedDate = today,
                Status = EnumAssignmentStatus.Accepted
            };
            Assert.False(filter.Compile()(assignment1));
        }

        [Fact]
        public async Task GetUserAssignmentAsync_ValidInput_ReturnsCorrectAssignments()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var today = DateTime.Today;
            var assignments = new List<Assignment>
            {
                new Assignment
                {
                    Id = Guid.NewGuid(),
                    AssignedTo = userId,
                    AssignedBy = Guid.NewGuid(),
                    AssignedDate = today,
                    AssetId = Guid.NewGuid(),
                    Status = EnumAssignmentStatus.Accepted,
                    Note = "Test note 1",
                    Asset = new Asset { AssetCode = "A001", AssetName = "Asset 1" },
                    UserTo = new User { Username = "user1" },
                    UserBy = new User { Username = "admin1" }
                },
                new Assignment
                {
                    Id = Guid.NewGuid(),
                    AssignedTo = userId,
                    AssignedBy = Guid.NewGuid(),
                    AssignedDate = today,
                    AssetId = Guid.NewGuid(),
                    Status = EnumAssignmentStatus.WaitingForAcceptance,
                    Note = "Test note 2",
                    Asset = new Asset { AssetCode = "A002", AssetName = "Asset 2" },
                    UserTo = new User { Username = "user2" },
                    UserBy = new User { Username = "admin2" }
                }
            };

            var returnRequests = new List<ReturnRequest>
            {
                new ReturnRequest
                {
                    Id = Guid.NewGuid(),
                    AssignmentId = assignments[0].Id,
                    IsDeleted = false
                },
                new ReturnRequest
                {
                    Id = Guid.NewGuid(),
                    AssignmentId = assignments[1].Id,
                    IsDeleted = false
                }
            };

            _mockUnitOfWork.Setup(u => u.AssignmentRepository.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<Expression<Func<Assignment, bool>>>(),
                It.IsAny<Func<IQueryable<Assignment>, IOrderedQueryable<Assignment>>>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Assignment, bool>>?>(),
                It.IsAny<int>()
            )).ReturnsAsync((int page, Expression<Func<Assignment, bool>> filter, Func<IQueryable<Assignment>, IOrderedQueryable<Assignment>>? orderBy, string includeProperties, Expression<Func<Assignment, bool>>? prioritizeCondition, int pageSize) =>
            {
                var filteredAssignments = assignments.AsQueryable().Where(filter);
                if (orderBy != null)
                {
                    filteredAssignments = orderBy(filteredAssignments);
                }
                return (filteredAssignments.ToList(), filteredAssignments.Count());
            });

            _mockUnitOfWork.Setup(u => u.ReturnRequestRepository.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<Expression<Func<ReturnRequest, bool>>>(),
                It.IsAny<Func<IQueryable<ReturnRequest>, IOrderedQueryable<ReturnRequest>>>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<ReturnRequest, bool>>?>(),
                It.IsAny<int>()
            )).ReturnsAsync((int page, Expression<Func<ReturnRequest, bool>> filter, Func<IQueryable<ReturnRequest>, IOrderedQueryable<ReturnRequest>>? orderBy, string includeProperties, Expression<Func<ReturnRequest, bool>>? prioritizeCondition, int pageSize) =>
            {
                var filteredReturnRequests = returnRequests.AsQueryable().Where(filter);
                return (filteredReturnRequests.ToList(), filteredReturnRequests.Count());
            });

            _mockMapper.Setup(m => m.Map<ReturnRequestResponse>(It.IsAny<ReturnRequest>()))
                .Returns((ReturnRequest request) => new ReturnRequestResponse
                {
                    Id = request.Id
                });

            // Act
            var result = await _assignmentService.GetUserAssignmentAsync(1, null, userId);

            // Assert
            Assert.NotNull(result.data);
            Assert.Equal(2, result.data.Count());
            Assert.Equal(assignments[0].Id, result.data.ElementAt(0).Id);
            Assert.Equal(assignments[1].Id, result.data.ElementAt(1).Id);

            Assert.NotNull(result.data.ElementAt(0).ReturnRequests);
            Assert.Equal(returnRequests[0].Id, result.data.ElementAt(0).ReturnRequests.Id);

            Assert.NotNull(result.data.ElementAt(1).ReturnRequests);
            Assert.Equal(returnRequests[1].Id, result.data.ElementAt(1).ReturnRequests.Id);
        }
    }
}
