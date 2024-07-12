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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace AssetManagement.Test.Unit.ReturnRequestServiceTest
{
    [ExcludeFromCodeCoverage]
    public class ReturnRequestServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly RequestReturnService _requestReturnService;

        public ReturnRequestServiceTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _requestReturnService = new RequestReturnService(_unitOfWorkMock.Object, _mapperMock.Object);
        }


        [Fact]
        public async Task UserCreateReturnRequestAsync_ValidRequest_ReturnsReturnRequestResponse()
        {
            // Arrange
            var assignmentId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var assignment = new Assignment
            {
                Id = assignmentId,
                Status = EnumAssignmentStatus.Accepted,
                AssignedTo = userId,
                UserTo = new User(),
                UserBy = new User(),
                Asset = new Asset()
            };
            var returnRequestResponse = new ReturnRequestResponse();

            _unitOfWorkMock.Setup(u => u.AssignmentRepository.GetAsync(
                It.IsAny<Expression<Func<Assignment, bool>>>()
            )).ReturnsAsync(assignment);
            _unitOfWorkMock.Setup(u => u.ReturnRequestRepository.AddAsync(It.IsAny<ReturnRequest>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CommitAsync()).ReturnsAsync(1);
            _mapperMock.Setup(m => m.Map<ReturnRequestResponse>(It.IsAny<ReturnRequest>()))
                .Returns(returnRequestResponse);

            // Act
            var result = await _requestReturnService.UserCreateReturnRequestAsync(assignmentId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(returnRequestResponse, result);
            _unitOfWorkMock.Verify(u => u.AssignmentRepository.GetAsync(
                It.IsAny<Expression<Func<Assignment, bool>>>()
            ), Times.Once);
            _unitOfWorkMock.Verify(u => u.ReturnRequestRepository.AddAsync(It.IsAny<ReturnRequest>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map<ReturnRequestResponse>(It.IsAny<ReturnRequest>()), Times.Once);
        }

        [Fact]
        public async Task AddReturnRequestAsync_ValidRequest_ReturnsReturnRequestResponse()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var assignmentId = Guid.NewGuid();
            var assignment = new Assignment { Id = assignmentId, Status = EnumAssignmentStatus.Accepted };
            var returnRequestResponse = new ReturnRequestResponse();

            _unitOfWorkMock.Setup(u => u.AssignmentRepository.GetAsync(It.IsAny<Expression<Func<Assignment, bool>>>()))
                .ReturnsAsync(assignment);
            _unitOfWorkMock.Setup(u => u.ReturnRequestRepository.AddAsync(It.IsAny<ReturnRequest>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CommitAsync()).ReturnsAsync(1);
            _mapperMock.Setup(m => m.Map<ReturnRequestResponse>(It.IsAny<ReturnRequest>()))
                .Returns(returnRequestResponse);

            // Act
            var result = await _requestReturnService.AddReturnRequestAsync(adminId, assignmentId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(returnRequestResponse, result);
            _unitOfWorkMock.Verify(u => u.AssignmentRepository.GetAsync(It.IsAny<Expression<Func<Assignment, bool>>>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.ReturnRequestRepository.AddAsync(It.IsAny<ReturnRequest>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map<ReturnRequestResponse>(It.IsAny<ReturnRequest>()), Times.Once);
        }

        [Fact]
        public async Task GetReturnRequestResponses_ValidRequest_ReturnsReturnRequestResponses()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var requestFilter = new ReturnFilterRequest();
            var returnRequests = (
                items: new List<ReturnRequest>
                {
                    new ReturnRequest(),
                    new ReturnRequest()
                },
                totalCount: 2
            );
            var returnRequestResponses = new List<ReturnRequestResponse>
            {
                new ReturnRequestResponse(),
                new ReturnRequestResponse()
            };

            _unitOfWorkMock.Setup(u => u.ReturnRequestRepository.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<Expression<Func<ReturnRequest, bool>>>(),
                It.IsAny<Func<IQueryable<ReturnRequest>, IOrderedQueryable<ReturnRequest>>>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<ReturnRequest, bool>>>(),
                It.IsAny<int>()
            )).ReturnsAsync(returnRequests);
            _mapperMock.Setup(m => m.Map<IEnumerable<ReturnRequestResponse>>(It.IsAny<IEnumerable<ReturnRequest>>()))
                .Returns(returnRequestResponses);

            // Act
            var result = await _requestReturnService.GetReturnRequestResponses(locationId, requestFilter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(returnRequestResponses, result.Item1);
            Assert.Equal(returnRequests.totalCount, result.totalCount);
            _unitOfWorkMock.Verify(u => u.ReturnRequestRepository.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<Expression<Func<ReturnRequest, bool>>>(),
                It.IsAny<Func<IQueryable<ReturnRequest>, IOrderedQueryable<ReturnRequest>>>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<ReturnRequest, bool>>>(),
                It.IsAny<int>()
            ), Times.Once);
            _mapperMock.Verify(m => m.Map<IEnumerable<ReturnRequestResponse>>(It.IsAny<IEnumerable<ReturnRequest>>()), Times.Once);
        }

        [Fact]
        public async Task CompleteReturnRequest_ValidRequest_CompletesReturnRequest()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var returnRequest = new ReturnRequest
            {
                Id = id,
                ReturnStatus = EnumReturnRequestStatus.WaitingForReturning,
                Assignment = new Assignment
                {
                    AssetId = Guid.NewGuid()
                }
            };
            var asset = new Asset();
            var assignment = new Assignment();

            _unitOfWorkMock.Setup(u => u.ReturnRequestRepository.GetAsync(
                It.IsAny<Expression<Func<ReturnRequest, bool>>>(),
                It.IsAny<Expression<Func<ReturnRequest, object>>>()
            )).ReturnsAsync(returnRequest);
            _unitOfWorkMock.Setup(u => u.AssetRepository.GetAsync(It.IsAny<Expression<Func<Asset, bool>>>()))
                .ReturnsAsync(asset);
            _unitOfWorkMock.Setup(u => u.AssignmentRepository.GetAsync(It.IsAny<Expression<Func<Assignment, bool>>>()))
                .ReturnsAsync(assignment);
            _unitOfWorkMock.Setup(u => u.CommitAsync()).ReturnsAsync(1);

            // Act
            await _requestReturnService.CompleteReturnRequest(id, userId);

            // Assert
            _unitOfWorkMock.Verify(u => u.ReturnRequestRepository.GetAsync(
                It.IsAny<Expression<Func<ReturnRequest, bool>>>(),
                It.IsAny<Expression<Func<ReturnRequest, object>>>()
            ), Times.Once);
            _unitOfWorkMock.Verify(u => u.AssetRepository.GetAsync(It.IsAny<Expression<Func<Asset, bool>>>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.AssignmentRepository.GetAsync(It.IsAny<Expression<Func<Assignment, bool>>>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.AssetRepository.Update(It.IsAny<Asset>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.AssignmentRepository.Update(It.IsAny<Assignment>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task CancelRequest_ValidRequest_CancelsRequest()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new ReturnRequest { Id = id, Assignment = new Assignment { Status = EnumAssignmentStatus.WaitingForAcceptance } };

            _unitOfWorkMock.Setup(u => u.ReturnRequestRepository.GetAsync(It.IsAny<Expression<Func<ReturnRequest, bool>>>(), r => r.Assignment))
                .ReturnsAsync(request);
            _unitOfWorkMock.Setup(u => u.CommitAsync()).ReturnsAsync(1);

            // Act
            var result = await _requestReturnService.CancelRequest(id);

            // Assert
            Assert.True(result);
            _unitOfWorkMock.Verify(u => u.ReturnRequestRepository.GetAsync(It.IsAny<Expression<Func<ReturnRequest, bool>>>(), r => r.Assignment), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }


        [Fact]
        public async Task UserCreateReturnRequestAsync_AssignmentNotFound_ThrowsArgumentException()
        {
            // Arrange
            var assignmentId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _unitOfWorkMock.Setup(u => u.AssignmentRepository.GetAsync(
                It.IsAny<Expression<Func<Assignment, bool>>>(),
                It.IsAny<Expression<Func<Assignment, object>>[]>()
            )).ReturnsAsync((Assignment)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _requestReturnService.UserCreateReturnRequestAsync(assignmentId, userId));
        }

        [Fact]
        public async Task UserCreateReturnRequestAsync_AssignmentNotAccepted_ThrowsArgumentException()
        {
            // Arrange
            var assignmentId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var assignment = new Assignment { Id = assignmentId, Status = EnumAssignmentStatus.WaitingForAcceptance };

            _unitOfWorkMock.Setup(u => u.AssignmentRepository.GetAsync(
                It.IsAny<Expression<Func<Assignment, bool>>>(),
                It.IsAny<Expression<Func<Assignment, object>>[]>()
            )).ReturnsAsync(assignment);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _requestReturnService.UserCreateReturnRequestAsync(assignmentId, userId));
        }

        [Fact]
        public async Task UserCreateReturnRequestAsync_AssignmentNotAssignedToUser_ThrowsArgumentException()
        {
            // Arrange
            var assignmentId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var assignment = new Assignment
            {
                Id = assignmentId,
                Status = EnumAssignmentStatus.Accepted,
                AssignedTo = Guid.NewGuid() // Different user
            };

            _unitOfWorkMock.Setup(u => u.AssignmentRepository.GetAsync(
                It.IsAny<Expression<Func<Assignment, bool>>>()
            )).ReturnsAsync(assignment);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _requestReturnService.UserCreateReturnRequestAsync(assignmentId, userId)
            );
            Assert.Equal("Not your assignment!", exception.Message);
        }

        [Fact]
        public async Task UserCreateReturnRequestAsync_CommitFailed_ThrowsArgumentException()
        {
            // Arrange
            var assignmentId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var assignment = new Assignment
            {
                Id = assignmentId,
                Status = EnumAssignmentStatus.Accepted,
                AssignedTo = userId,
                UserTo = new User(),
                UserBy = new User(),
                Asset = new Asset(),
            };

            _unitOfWorkMock.Setup(u => u.AssignmentRepository.GetAsync(
                It.IsAny<Expression<Func<Assignment, bool>>>()
            )).ReturnsAsync(assignment);
            _unitOfWorkMock.Setup(u => u.ReturnRequestRepository.AddAsync(It.IsAny<ReturnRequest>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CommitAsync()).ReturnsAsync(0); // Simulating a failed commit

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _requestReturnService.UserCreateReturnRequestAsync(assignmentId, userId));
            Assert.Equal("An error occurred while create return request.", exception.Message);
        }

        [Fact]
        public async Task AddReturnRequestAsync_CommitFailed_ThrowsArgumentException()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var assignmentId = Guid.NewGuid();
            var assignment = new Assignment { Id = assignmentId, Status = EnumAssignmentStatus.Accepted };

            _unitOfWorkMock.Setup(u => u.AssignmentRepository.GetAsync(It.IsAny<Expression<Func<Assignment, bool>>>()))
                .ReturnsAsync(assignment);
            _unitOfWorkMock.Setup(u => u.ReturnRequestRepository.AddAsync(It.IsAny<ReturnRequest>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CommitAsync()).ReturnsAsync(0); // Simulating a failed commit

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _requestReturnService.AddReturnRequestAsync(adminId, assignmentId));
            Assert.Equal("An error occurred while create return request.", exception.Message);
        }

        [Fact]
        public async Task AddReturnRequestAsync_AssignmentNotAvailable_ThrowsArgumentException()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var assignmentId = Guid.NewGuid();
            var assignment = new Assignment { Id = assignmentId, Status = EnumAssignmentStatus.WaitingForAcceptance };

            _unitOfWorkMock.Setup(u => u.AssignmentRepository.GetAsync(It.IsAny<Expression<Func<Assignment, bool>>>()))
                .ReturnsAsync(assignment);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _requestReturnService.AddReturnRequestAsync(adminId, assignmentId));
        }

        [Theory]
        [InlineData("AssetName")]
        [InlineData("RequestedBy")]
        [InlineData("AssignedDate")]
        [InlineData("AcceptedBy")]
        [InlineData("ReturnedDate")]
        [InlineData("State")]
        [InlineData("Default")]
        public async Task GetReturnRequestResponses_ValidSortBy_AppliesCorrectOrderBy(string sortBy)
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var requestFilter = new ReturnFilterRequest { SortBy = sortBy };
            var returnRequests = (
                items: new List<ReturnRequest>
                {
            new ReturnRequest(),
            new ReturnRequest()
                },
                totalCount: 2
            );

            _unitOfWorkMock.Setup(u => u.ReturnRequestRepository.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<Expression<Func<ReturnRequest, bool>>>(),
                It.IsAny<Func<IQueryable<ReturnRequest>, IOrderedQueryable<ReturnRequest>>>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<ReturnRequest, bool>>>(),
                It.IsAny<int>()
            )).ReturnsAsync(returnRequests);

            // Act
            await _requestReturnService.GetReturnRequestResponses(locationId, requestFilter);

            // Assert
            _unitOfWorkMock.Verify(u => u.ReturnRequestRepository.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<Expression<Func<ReturnRequest, bool>>>(),
                It.IsAny<Func<IQueryable<ReturnRequest>, IOrderedQueryable<ReturnRequest>>>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<ReturnRequest, bool>>>(),
                It.IsAny<int>()
            ), Times.Once);
        }

        [Fact]
        public async Task CompleteReturnRequest_ReturnRequestNotFound_ThrowsArgumentException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _unitOfWorkMock.Setup(u => u.ReturnRequestRepository.GetAsync(
                It.IsAny<Expression<Func<ReturnRequest, bool>>>(),
                It.IsAny<Expression<Func<ReturnRequest, object>>>()
            )).ReturnsAsync((ReturnRequest)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _requestReturnService.CompleteReturnRequest(id, userId));
        }

        [Fact]
        public async Task CompleteReturnRequest_InvalidReturnStatus_ThrowsArgumentException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var returnRequest = new ReturnRequest
            {
                Id = id,
                ReturnStatus = EnumReturnRequestStatus.Completed,
                Assignment = new Assignment { AssetId = Guid.NewGuid() }
            };

            _unitOfWorkMock.Setup(u => u.ReturnRequestRepository.GetAsync(
                It.IsAny<Expression<Func<ReturnRequest, bool>>>(),
                It.IsAny<Expression<Func<ReturnRequest, object>>>()
            )).ReturnsAsync(returnRequest);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _requestReturnService.CompleteReturnRequest(id, userId));
        }

        [Fact]
        public async Task CompleteReturnRequest_AssetNotFound_ThrowsArgumentException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var returnRequest = new ReturnRequest
            {
                Id = id,
                ReturnStatus = EnumReturnRequestStatus.WaitingForReturning,
                Assignment = new Assignment { AssetId = Guid.NewGuid() }
            };

            _unitOfWorkMock.Setup(u => u.ReturnRequestRepository.GetAsync(
                It.IsAny<Expression<Func<ReturnRequest, bool>>>(),
                It.IsAny<Expression<Func<ReturnRequest, object>>>()
            )).ReturnsAsync(returnRequest);
            _unitOfWorkMock.Setup(u => u.AssetRepository.GetAsync(It.IsAny<Expression<Func<Asset, bool>>>()))
                .ReturnsAsync((Asset)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _requestReturnService.CompleteReturnRequest(id, userId));
        }

        [Fact]
        public async Task CompleteReturnRequest_AssignmentNotFound_ThrowsArgumentException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var returnRequest = new ReturnRequest
            {
                Id = id,
                ReturnStatus = EnumReturnRequestStatus.WaitingForReturning,
                Assignment = new Assignment { AssetId = Guid.NewGuid() }
            };
            var asset = new Asset();

            _unitOfWorkMock.Setup(u => u.ReturnRequestRepository.GetAsync(
                It.IsAny<Expression<Func<ReturnRequest, bool>>>(),
                It.IsAny<Expression<Func<ReturnRequest, object>>>()
            )).ReturnsAsync(returnRequest);
            _unitOfWorkMock.Setup(u => u.AssetRepository.GetAsync(It.IsAny<Expression<Func<Asset, bool>>>()))
                .ReturnsAsync(asset);
            _unitOfWorkMock.Setup(u => u.AssignmentRepository.GetAsync(It.IsAny<Expression<Func<Assignment, bool>>>()))
                .ReturnsAsync((Assignment)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _requestReturnService.CompleteReturnRequest(id, userId));
        }

        [Fact]
        public async Task CancelRequest_RequestNotFound_ThrowsArgumentException()
        {
            // Arrange
            var id = Guid.NewGuid();

            _unitOfWorkMock.Setup(u => u.ReturnRequestRepository.GetAsync(It.IsAny<Expression<Func<ReturnRequest, bool>>>()))
                .ReturnsAsync((ReturnRequest)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _requestReturnService.CancelRequest(id));
        }

        [Fact]
        public async Task CancelRequest_RequestAlreadyCompleted_ThrowsArgumentException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new ReturnRequest { Id = id, ReturnStatus = EnumReturnRequestStatus.Completed };

            _unitOfWorkMock.Setup(u => u.ReturnRequestRepository.GetAsync(It.IsAny<Expression<Func<ReturnRequest, bool>>>()))
                .ReturnsAsync(request);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _requestReturnService.CancelRequest(id));
        }

        [Fact]
        public async Task UserCreateReturnRequestAsync_InvalidAssignmentStatus_ThrowsArgumentException()
        {
            // Arrange
            var assignmentId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var assignment = new Assignment
            {
                Id = assignmentId,
                Status = EnumAssignmentStatus.WaitingForAcceptance,
                AssignedTo = userId
            };

            _unitOfWorkMock.Setup(u => u.AssignmentRepository.GetAsync(
                It.IsAny<Expression<Func<Assignment, bool>>>()
            )).ReturnsAsync(assignment);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _requestReturnService.UserCreateReturnRequestAsync(assignmentId, userId)
            );
            Assert.Equal("Invalid assignment!", exception.Message);
        }

        [Fact]
        public async Task UserCreateReturnRequestAsync_ReturnRequestAlreadyExists_ThrowsArgumentException()
        {
            // Arrange
            var assignmentId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var assignment = new Assignment
            {
                Id = assignmentId,
                Status = EnumAssignmentStatus.Accepted,
                AssignedTo = userId
            };

            _unitOfWorkMock.Setup(u => u.AssignmentRepository.GetAsync(
                It.IsAny<Expression<Func<Assignment, bool>>>()
            )).ReturnsAsync(assignment);

            _unitOfWorkMock.Setup(u => u.ReturnRequestRepository.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<Expression<Func<ReturnRequest, bool>>>(),
                It.IsAny<Func<IQueryable<ReturnRequest>, IOrderedQueryable<ReturnRequest>>>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<ReturnRequest, bool>>>(),
                It.IsAny<int>()  // Add this line to explicitly provide the pageSize parameter
            )).ReturnsAsync((new List<ReturnRequest> { new ReturnRequest() }, 1));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _requestReturnService.UserCreateReturnRequestAsync(assignmentId, userId)
            );
            Assert.Equal("A return request for this assignment already exists.", exception.Message);
        }

        [Fact]
        public async Task UserCreateReturnRequestAsync_ReturnRequestAlreadyExisted_ThrowsException()
        {
            // Arrange
            var assignmentId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var assignment = new Assignment
            {
                Id = assignmentId,
                Status = EnumAssignmentStatus.Accepted,
                AssignedTo = userId
            };

            _unitOfWorkMock.Setup(u => u.AssignmentRepository.GetAsync(
                It.IsAny<Expression<Func<Assignment, bool>>>()
            )).ReturnsAsync(assignment);

            _unitOfWorkMock.Setup(u => u.ReturnRequestRepository.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<Expression<Func<ReturnRequest, bool>>>(),
                It.IsAny<Func<IQueryable<ReturnRequest>, IOrderedQueryable<ReturnRequest>>>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<ReturnRequest, bool>>>(),
                It.IsAny<int>()  // Add this line to explicitly provide the pageSize parameter
            )).ReturnsAsync((new List<ReturnRequest>(), 0));

            _unitOfWorkMock.Setup(u => u.ReturnRequestRepository.GetAsync(
                It.IsAny<Expression<Func<ReturnRequest, bool>>>()
            )).ReturnsAsync(new ReturnRequest());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _requestReturnService.UserCreateReturnRequestAsync(assignmentId, userId)
            );
            Assert.Equal("Return request already existed!", exception.Message);
        }

        [Fact]
        public async Task AddReturnRequestAsync_ReturnRequestAlreadyExists_ThrowsArgumentException()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var assignmentId = Guid.NewGuid();
            var assignment = new Assignment { Id = assignmentId, Status = EnumAssignmentStatus.Accepted };

            _unitOfWorkMock.Setup(u => u.AssignmentRepository.GetAsync(It.IsAny<Expression<Func<Assignment, bool>>>()))
                .ReturnsAsync(assignment);

            _unitOfWorkMock.Setup(u => u.ReturnRequestRepository.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<Expression<Func<ReturnRequest, bool>>>(),
                It.IsAny<Func<IQueryable<ReturnRequest>, IOrderedQueryable<ReturnRequest>>>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<ReturnRequest, bool>>>(),
                It.IsAny<int>()  // Add this line to explicitly provide the pageSize parameter
            )).ReturnsAsync((new List<ReturnRequest> { new ReturnRequest() }, 1));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _requestReturnService.AddReturnRequestAsync(adminId, assignmentId)
            );
            Assert.Equal("A return request for this assignment already exists.", exception.Message);
        }

        
    }
}