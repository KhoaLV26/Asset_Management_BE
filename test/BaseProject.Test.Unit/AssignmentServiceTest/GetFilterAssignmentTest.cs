﻿//using AssetManagement.Application.Models.Responses;
//using AssetManagement.Application.Services.Implementations;
//using AssetManagement.Domain.Entities;
//using AssetManagement.Domain.Enums;
//using AssetManagement.Domain.Interfaces;
//using AutoMapper;
//using Moq;
//using Moq.Protected;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Text;
//using System.Threading.Tasks;
//using Xunit;

//namespace AssetManagement.Test.Unit.AssignmentServiceTest
//{
//    public class GetFilterAssignmentTest
//    {
//        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
//        private readonly Mock<IMapper> _mockMapper;
//        private readonly AssignmentService _assignmentService;

//        public GetFilterAssignmentTest()
//        {
//            _mockUnitOfWork = new Mock<IUnitOfWork>();
//            _mockMapper = new Mock<IMapper>();
//            _assignmentService = new AssignmentService(_mockUnitOfWork.Object, _mockMapper.Object);
//        }

//        [Fact]
//        public void GetOrderQuery_NullSortOrder_ReturnsAscendingOrder()
//        {
//            // Arrange
//            string sortOrder = null;
//            string sortBy = "assetCode";

//            // Act
//            var orderBy = _assignmentService.GetOrderQuery(sortOrder, sortBy);

//            // Assert
//            Assert.NotNull(orderBy);
//            var assignments = new List<Assignment>
//            {
//                new Assignment { Asset = new Asset { AssetCode = "B001" } },
//                new Assignment { Asset = new Asset { AssetCode = "A001" } }
//            };
//            var orderedAssignments = orderBy(assignments.AsQueryable()).ToList();
//            Assert.Equal("A001", orderedAssignments.First().Asset.AssetCode);
//            Assert.Equal("B001", orderedAssignments.Last().Asset.AssetCode);
//        }

//        [Fact]
//        public void GetOrderQuery_DescSortOrder_ReturnsDescendingOrder()
//        {
//            // Arrange
//            string sortOrder = "desc";
//            string sortBy = "assignedDate";

//            // Act
//            var orderBy = _assignmentService.GetOrderQuery(sortOrder, sortBy);

//            // Assert
//            Assert.NotNull(orderBy);
//            var assignments = new List<Assignment>
//            {
//                new Assignment { AssignedDate = DateTime.UtcNow.AddDays(-1) },
//                new Assignment { AssignedDate = DateTime.UtcNow }
//            };
//            var orderedAssignments = orderBy(assignments.AsQueryable()).ToList();
//            Assert.True(orderedAssignments.First().AssignedDate > orderedAssignments.Last().AssignedDate);
//        }

//        [Fact]
//        public void GetOrderQuery_InvalidSortBy_ReturnsNullOrderBy()
//        {
//            // Arrange
//            string sortOrder = "asc";
//            string sortBy = "invalid";

//            // Act
//            var orderBy = _assignmentService.GetOrderQuery(sortOrder, sortBy);

//            // Assert
//            Assert.Null(orderBy);
//        }

//        [Fact]
//        public void GetFilterQuery_WithAssignedDate_ReturnsCorrectExpression()
//        {
//            // Arrange
//            var assignedDate = new DateTime(2023, 6, 1);
//            string state = null;
//            string search = null;

//            // Act
//            var filter = _assignmentService.GetFilterQuery(assignedDate, state, search).Result;

//            // Assert
//            Assert.NotNull(filter);
//            var assignment1 = new Assignment
//            {
//                AssignedDate = assignedDate,
//                Status = EnumAssignmentStatus.Accepted
//            };
//            var assignment2 = new Assignment
//            {
//                AssignedDate = assignedDate,
//                Status = EnumAssignmentStatus.WaitingForAcceptance
//            };
//            var assignment3 = new Assignment
//            {
//                AssignedDate = assignedDate.AddDays(1),
//                Status = EnumAssignmentStatus.Accepted
//            };
//            Assert.True(filter.Compile()(assignment1));
//            Assert.True(filter.Compile()(assignment2));
//            Assert.False(filter.Compile()(assignment3));
//        }

//        [Fact]
//        public void GetFilterQuery_WithValidState_ReturnsCorrectExpression()
//        {
//            // Arrange
//            DateTime? assignedDate = null;
//            string state = "Accepted"; // Valid state value
//            string search = null;

//            // Act
//            var filter = _assignmentService.GetFilterQuery(assignedDate, state, search).Result;

//            // Assert
//            Assert.NotNull(filter);
//            var assignment1 = new Assignment { Status = EnumAssignmentStatus.Accepted };
//            var assignment2 = new Assignment { Status = EnumAssignmentStatus.WaitingForAcceptance };
//            Assert.True(filter.Compile()(assignment1));
//            Assert.False(filter.Compile()(assignment2));
//        }

//        [Fact]
//        public async Task GetFilterQuery_WithInvalidState_ThrowsInvalidCastException()
//        {
//            // Arrange
//            DateTime? assignedDate = null;
//            string state = "InvalidState"; // Invalid state value
//            string search = null;

//            // Act & Assert
//            var exception = await Record.ExceptionAsync(() => _assignmentService.GetFilterQuery(assignedDate, state, search));
//            Assert.IsType<InvalidCastException>(exception);
//            Assert.Equal("Invalid status value", exception.Message);
//        }

//        [Theory]
//        [InlineData("assetcode", "asc", "ABC123", "XYZ789")]
//        [InlineData("assetname", "desc", "Monitor", "Laptop")]
//        [InlineData("assigneddate", "asc", "2023-06-01", "2023-06-05")]
//        [InlineData("state", "desc", EnumAssignmentStatus.WaitingForAcceptance, EnumAssignmentStatus.Accepted)]
//        public void GetOrderQuery_ReturnsSortedResults(string sortBy, string sortOrder, object value1, object value2)
//        {
//            // Arrange
//            var assignments = new List<Assignment>
//    {
//        new Assignment
//        {
//            Asset = new Asset { AssetCode = "XYZ789", AssetName = "Laptop" },
//            UserTo = new User { Username = "janesmith" },
//            UserBy = new User { Username = "assetmanager" },
//            AssignedDate = new DateTime(2023, 06, 05),
//            Status = EnumAssignmentStatus.Accepted
//        },
//        new Assignment
//        {
//            Asset = new Asset { AssetCode = "ABC123", AssetName = "Monitor" },
//            UserTo = new User { Username = "johnsmith" },
//            UserBy = new User { Username = "adminuser" },
//            AssignedDate = new DateTime(2023, 06, 01),
//            Status = EnumAssignmentStatus.WaitingForAcceptance
//        }
//    };

//            // Act
//            var orderBy = _assignmentService.GetOrderQuery(sortOrder, sortBy);
//            var sortedAssignments = orderBy(assignments.AsQueryable()).ToList();

//            // Assert
//            Assert.Equal(value1, GetPropertyValue(sortedAssignments[0], sortBy));
//            Assert.Equal(value2, GetPropertyValue(sortedAssignments[1], sortBy));
//        }

//        private object GetPropertyValue(Assignment assignment, string propertyName)
//        {
//            return propertyName switch
//            {
//                "assetcode" => assignment.Asset.AssetCode,
//                "assetname" => assignment.Asset.AssetName,
//                "assignedto" => assignment.UserTo.Username,
//                "assignedby" => assignment.UserBy.Username,
//                "assigneddate" => assignment.AssignedDate.ToString("yyyy-MM-dd"),
//                "state" => assignment.Status,
//                _ => throw new ArgumentException($"Invalid property name: {propertyName}"),
//            };
//        }

//        [Fact]
//        public async Task GetAllAssignmentAsync_ReturnsAssignmentResponses()
//        {
//            // Arrange
//            int pageNumber = 1;
//            string state = null;
//            DateTime? assignedDate = null;
//            string search = null;
//            string sortOrder = null;
//            string sortBy = "assetCode";
//            string includeProperties = "UserTo,UserBy,Asset";
//            Guid? newAssignmentId = null;

//            var assignments = new List<Assignment>
//    {
//        new Assignment
//        {
//            Id = Guid.NewGuid(),
//            AssetId = Guid.NewGuid(),
//            Asset = new Asset { AssetCode = "ABC123", AssetName = "Laptop" },
//            AssignedTo = Guid.NewGuid(),
//            UserTo = new User { Username = "johnsmith" },
//            AssignedBy = Guid.NewGuid(),
//            UserBy = new User { Username = "adminuser" },
//            AssignedDate = DateTime.UtcNow,
//            Note = "Sample note",
//            Status = EnumAssignmentStatus.Accepted,
//            IsDeleted = false
//        },
//        new Assignment
//        {
//            Id = Guid.NewGuid(),
//            AssetId = Guid.NewGuid(),
//            Asset = new Asset { AssetCode = "XYZ789", AssetName = "Monitor" },
//            AssignedTo = Guid.NewGuid(),
//            UserTo = new User { Username = "janesmith" },
//            AssignedBy = Guid.NewGuid(),
//            UserBy = new User { Username = "assetmanager" },
//            AssignedDate = DateTime.UtcNow.AddDays(-1),
//            Note = "Another note",
//            Status = EnumAssignmentStatus.WaitingForAcceptance,
//            IsDeleted = false
//        }
//    };

//            var mockAssignmentRepository = new Mock<IAssignmentRepository>();
//            mockAssignmentRepository
//                .Setup(x => x.GetAllAsync(
//                    It.IsAny<int>(),
//                    It.IsAny<Expression<Func<Assignment, bool>>>(),
//                    It.IsAny<Func<IQueryable<Assignment>, IOrderedQueryable<Assignment>>>(),
//                    It.IsAny<string>(),
//                    It.IsAny<Expression<Func<Assignment, bool>>>()))
//                .ReturnsAsync((assignments, assignments.Count));

//            _mockUnitOfWork.Setup(x => x.AssignmentRepository).Returns(mockAssignmentRepository.Object);

//            // Act
//            var result = await _assignmentService.GetAllAssignmentAsync(pageNumber, state, assignedDate, search, sortOrder, sortBy, includeProperties, newAssignmentId);

//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal(assignments.Count, result.totalCount);
//            Assert.Equal(assignments.Count, result.data.Count());

//            var assignmentResponse = result.data.First();
//            var assignment = assignments.First();
//            Assert.Equal(assignment.Id, assignmentResponse.Id);
//            Assert.Equal(assignment.Asset.AssetCode, assignmentResponse.AssetCode);
//            Assert.Equal(assignment.Asset.AssetName, assignmentResponse.AssetName);
//            Assert.Equal(assignment.UserTo.Username, assignmentResponse.To);
//            Assert.Equal(assignment.UserBy.Username, assignmentResponse.By);
//            Assert.Equal(assignment.AssignedDate, assignmentResponse.AssignedDate);
//            Assert.Equal(assignment.Note, assignmentResponse.Note);
//            Assert.Equal(assignment.Status, assignmentResponse.Status);
//        }

//        [Fact]
//        public async Task GetAssignmentDetailAsync_ReturnsAssignmentResponse_WhenAssignmentExists()
//        {
//            // Arrange
//            var assignmentId = Guid.NewGuid();
//            var assignment = new Assignment
//            {
//                Id = assignmentId,
//                AssetId = Guid.NewGuid(),
//                Asset = new Asset { AssetCode = "ABC123", AssetName = "Laptop" },
//                AssignedTo = Guid.NewGuid(),
//                UserTo = new User { Username = "johnsmith" },
//                AssignedBy = Guid.NewGuid(),
//                UserBy = new User { Username = "adminuser" },
//                AssignedDate = DateTime.UtcNow,
//                Note = "Sample note",
//                Status = EnumAssignmentStatus.Accepted,
//                IsDeleted = false
//            };

//            var mockAssignmentRepository = new Mock<IAssignmentRepository>();
//            mockAssignmentRepository
//                .Setup(x => x.GetAsync(
//                    It.IsAny<Expression<Func<Assignment, bool>>>(),
//                    It.IsAny<Expression<Func<Assignment, object>>[]>()))
//                .ReturnsAsync(assignment);

//            _mockUnitOfWork.Setup(x => x.AssignmentRepository).Returns(mockAssignmentRepository.Object);

//            // Act
//            var result = await _assignmentService.GetAssignmentDetailAsync(assignmentId);

//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal(assignment.Id, result.Id);
//            Assert.Equal(assignment.Asset.AssetCode, result.AssetCode);
//            Assert.Equal(assignment.Asset.AssetName, result.AssetName);
//            Assert.Equal(assignment.UserTo.Username, result.To);
//            Assert.Equal(assignment.UserBy.Username, result.By);
//            Assert.Equal(assignment.AssignedDate, result.AssignedDate);
//            Assert.Equal(assignment.Note, result.Note);
//            Assert.Equal(assignment.Status, result.Status);
//        }

//        [Fact]
//        public async Task GetAssignmentDetailAsync_ReturnsNull_WhenAssignmentIsDeleted()
//        {
//            // Arrange
//            var assignmentId = Guid.NewGuid();
//            var assignment = new Assignment
//            {
//                Id = assignmentId,
//                AssetId = Guid.NewGuid(),
//                Asset = new Asset { AssetCode = "ABC123", AssetName = "Laptop" },
//                AssignedTo = Guid.NewGuid(),
//                UserTo = new User { Username = "johnsmith" },
//                AssignedBy = Guid.NewGuid(),
//                UserBy = new User { Username = "adminuser" },
//                AssignedDate = DateTime.UtcNow,
//                Note = "Sample note",
//                Status = EnumAssignmentStatus.Accepted,
//                IsDeleted = true
//            };

//            var mockAssignmentRepository = new Mock<IAssignmentRepository>();
//            mockAssignmentRepository
//                .Setup(x => x.GetAsync(
//                    It.IsAny<Expression<Func<Assignment, bool>>>(),
//                    It.IsAny<Expression<Func<Assignment, object>>[]>()))
//                .ReturnsAsync(assignment);

//            _mockUnitOfWork.Setup(x => x.AssignmentRepository).Returns(mockAssignmentRepository.Object);

//            // Act
//            var result = await _assignmentService.GetAssignmentDetailAsync(assignmentId);

//            // Assert
//            Assert.Null(result);
//        }
//    }
//}
