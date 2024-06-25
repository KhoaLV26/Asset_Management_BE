using AssetManagement.Application.Models.Responses;
using AssetManagement.Application.Services.Implementations;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Domain.Interfaces;
using AutoMapper;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AssetManagement.Test.Unit.AssignmentServiceTest
{
    public class GetFilterAssignmentTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly AssignmentService _assignmentService;

        public GetFilterAssignmentTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _assignmentService = new AssignmentService(_mockUnitOfWork.Object, _mockMapper.Object);
        }

    //    [Fact]
    //    public async Task GetAssignmentDetailAsync_ValidId_ReturnsAssignmentResponse()
    //    {
    //        // Arrange
    //        var assignmentId = Guid.NewGuid();
    //        var assignment = new Assignment
    //        {
    //            Id = assignmentId,
    //            AssignedTo = Guid.NewGuid(),
    //            AssignedBy = Guid.NewGuid(),
    //            AssignedDate = DateTime.UtcNow,
    //            AssetId = Guid.NewGuid(),
    //            Status = EnumAssignmentStatus.WaitingForAcceptance,
    //            CreatedAt = DateTime.UtcNow,
    //            CreatedBy = Guid.NewGuid(),
    //            Note = "Test note",
    //            Asset = new Asset { AssetCode = "A001", AssetName = "Asset 1" },
    //            UserTo = new User { Username = "user1" },
    //            UserBy = new User { Username = "admin" }
    //        };

    //        var expectedResponse = new AssignmentResponse
    //        {
    //            Id = assignment.Id,
    //            AssignedTo = assignment.AssignedTo,
    //            To = assignment.UserTo.Username,
    //            AssignedBy = assignment.AssignedBy,
    //            By = assignment.UserBy.Username,
    //            AssignedDate = assignment.AssignedDate,
    //            AssetId = assignment.AssetId,
    //            AssetCode = assignment.Asset.AssetCode,
    //            AssetName = assignment.Asset.AssetName,
    //            Note = assignment.Note,
    //            Status = assignment.Status
    //        };

    //        _mockUnitOfWork.Setup(u => u.AssignmentRepository.GetAssignmentDetailAsync(assignmentId))
    //            .ReturnsAsync(assignment);

    //        // Act
    //        var result = await _assignmentService.GetAssignmentDetailAsync(assignmentId);

    //        // Assert
    //        Assert.NotNull(result);
    //        Assert.Equal(expectedResponse.Id, result.Id);
    //        Assert.Equal(expectedResponse.AssignedTo, result.AssignedTo);
    //        Assert.Equal(expectedResponse.To, result.To);
    //        Assert.Equal(expectedResponse.AssignedBy, result.AssignedBy);
    //        Assert.Equal(expectedResponse.By, result.By);
    //        Assert.Equal(expectedResponse.AssignedDate, result.AssignedDate);
    //        Assert.Equal(expectedResponse.AssetId, result.AssetId);
    //        Assert.Equal(expectedResponse.AssetCode, result.AssetCode);
    //        Assert.Equal(expectedResponse.AssetName, result.AssetName);
    //        Assert.Equal(expectedResponse.Note, result.Note);
    //        Assert.Equal(expectedResponse.Status, result.Status);
    //    }

    //    [Fact]
    //    public async Task GetAssignmentDetailAsync_InvalidId_ReturnsNull()
    //    {
    //        // Arrange
    //        var assignmentId = Guid.NewGuid();
    //        _mockUnitOfWork.Setup(u => u.AssignmentRepository.GetAssignmentDetailAsync(assignmentId))
    //            .ReturnsAsync((Assignment)null);

    //        // Act
    //        var result = await _assignmentService.GetAssignmentDetailAsync(assignmentId);

    //        // Assert
    //        Assert.Null(result);
    //    }

    //    [Fact]
    //    public void GetOrderQuery_NullSortOrder_ReturnsAscendingOrder()
    //    {
    //        // Arrange
    //        string sortOrder = null;
    //        string sortBy = "assetCode";

    //        // Act
    //        var orderBy = _assignmentService.GetOrderQuery(sortOrder, sortBy);

    //        // Assert
    //        Assert.NotNull(orderBy);
    //        var assignments = new List<Assignment>
    //{
    //    new Assignment { Asset = new Asset { AssetCode = "B001" } },
    //    new Assignment { Asset = new Asset { AssetCode = "A001" } }
    //};
    //        var orderedAssignments = orderBy(assignments.AsQueryable()).ToList();
    //        Assert.Equal("A001", orderedAssignments.First().Asset.AssetCode);
    //        Assert.Equal("B001", orderedAssignments.Last().Asset.AssetCode);
    //    }

        [Fact]
        public void GetOrderQuery_DescSortOrder_ReturnsDescendingOrder()
        {
            // Arrange
            string sortOrder = "desc";
            string sortBy = "assignedDate";

            // Act
            var orderBy = _assignmentService.GetOrderQuery(sortOrder, sortBy);

            // Assert
            Assert.NotNull(orderBy);
            var assignments = new List<Assignment>
            {
                new Assignment { AssignedDate = DateTime.UtcNow.AddDays(-1) },
                new Assignment { AssignedDate = DateTime.UtcNow }
            };
            var orderedAssignments = orderBy(assignments.AsQueryable()).ToList();
            Assert.True(orderedAssignments.First().AssignedDate > orderedAssignments.Last().AssignedDate);
        }

        [Fact]
        public void GetOrderQuery_InvalidSortBy_ReturnsNullOrderBy()
        {
            // Arrange
            string sortOrder = "asc";
            string sortBy = "invalid";

            // Act
            var orderBy = _assignmentService.GetOrderQuery(sortOrder, sortBy);

            // Assert
            Assert.Null(orderBy);
        }

        [Fact]
        public void GetFilterQuery_WithAssignedDate_ReturnsCorrectExpression()
        {
            // Arrange
            var assignedDate = new DateTime(2023, 6, 1);
            string state = null;
            string search = null;

            // Act
            var filter = _assignmentService.GetFilterQuery(assignedDate, state, search).Result;

            // Assert
            Assert.NotNull(filter);
            var assignment1 = new Assignment { AssignedDate = assignedDate };
            var assignment2 = new Assignment { AssignedDate = assignedDate.AddDays(1) };
            Assert.True(filter.Compile()(assignment1));
            Assert.False(filter.Compile()(assignment2));
        }

        [Fact]
        public void GetFilterQuery_WithValidState_ReturnsCorrectExpression()
        {
            // Arrange
            DateTime? assignedDate = null;
            string state = "Accepted"; // Valid state value
            string search = null;

            // Act
            var filter = _assignmentService.GetFilterQuery(assignedDate, state, search).Result;

            // Assert
            Assert.NotNull(filter);
            var assignment1 = new Assignment { Status = EnumAssignmentStatus.Accepted };
            var assignment2 = new Assignment { Status = EnumAssignmentStatus.WaitingForAcceptance };
            Assert.True(filter.Compile()(assignment1));
            Assert.False(filter.Compile()(assignment2));
        }

        [Fact]
        public async Task GetFilterQuery_WithInvalidState_ThrowsInvalidCastException()
        {
            // Arrange
            DateTime? assignedDate = null;
            string state = "InvalidState"; // Invalid state value
            string search = null;

            // Act & Assert
            var exception = await Record.ExceptionAsync(() => _assignmentService.GetFilterQuery(assignedDate, state, search));
            Assert.IsType<InvalidCastException>(exception);
            Assert.Equal("Invalid status value", exception.Message);
        }
    }
}
