using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading.Tasks;
using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Application.Services;
using AssetManagement.Domain.Models;
using AssetManagement.WebAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AssetManagement.Test.Unit.ReturnRequestControllerTest
{
    [ExcludeFromCodeCoverage]
    public class RequestReturningControllerTest
    {
        private readonly Mock<IRequestReturnService> _requestReturnServiceMock;
        private readonly RequestReturningController _requestReturningController;

        public RequestReturningControllerTest()
        {
            _requestReturnServiceMock = new Mock<IRequestReturnService>();
            _requestReturningController = new RequestReturningController(_requestReturnServiceMock.Object);
        }

        [Fact]
        public async Task UserCreateReturnRequest_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var assignmentId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var returnResponse = new ReturnRequestResponse { Id = Guid.NewGuid(), AssignmentId = assignmentId };

            _requestReturnServiceMock.Setup(service => service.UserCreateReturnRequestAsync(assignmentId, userId))
                .ReturnsAsync(returnResponse);

            _requestReturningController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Actor, userId.ToString()),
                        new Claim(ClaimTypes.NameIdentifier, "testuser")
                    }, "mock"))
                }
            };

            // Act
            var result = await _requestReturningController.UserCreateReturnRequest(assignmentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralCreateResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Create return request successfully.", response.Message);
            Assert.Equal(returnResponse, response.Data);
        }

        [Fact]
        public async Task UserCreateReturnRequest_ArgumentException_ReturnsConflict()
        {
            // Arrange
            var assignmentId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var exceptionMessage = "Invalid argument.";

            _requestReturnServiceMock.Setup(service => service.UserCreateReturnRequestAsync(assignmentId, userId))
                .ThrowsAsync(new ArgumentException(exceptionMessage));

            _requestReturningController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Actor, userId.ToString()),
                        new Claim(ClaimTypes.NameIdentifier, "testuser")
                    }, "mock"))
                }
            };

            // Act
            var result = await _requestReturningController.UserCreateReturnRequest(assignmentId);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralCreateResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Message);
        }

        [Fact]
        public async Task UserCreateReturnRequest_InvalidOperationException_ReturnsConflict()
        {
            // Arrange
            var assignmentId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var exceptionMessage = "Invalid operation.";

            _requestReturnServiceMock.Setup(service => service.UserCreateReturnRequestAsync(assignmentId, userId))
                .ThrowsAsync(new InvalidOperationException(exceptionMessage));

            _requestReturningController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Actor, userId.ToString()),
                        new Claim(ClaimTypes.NameIdentifier, "testuser")
                    }, "mock"))
                }
            };

            // Act
            var result = await _requestReturningController.UserCreateReturnRequest(assignmentId);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Message);
        }

        [Fact]
        public async Task UserCreateReturnRequest_Exception_ReturnsInternalServerError()
        {
            // Arrange
            var assignmentId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var exceptionMessage = "An unexpected error occurred.";

            _requestReturnServiceMock.Setup(service => service.UserCreateReturnRequestAsync(assignmentId, userId))
                .ThrowsAsync(new Exception(exceptionMessage));

            _requestReturningController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Actor, userId.ToString()),
                        new Claim(ClaimTypes.NameIdentifier, "testuser")
                    }, "mock"))
                }
            };

            // Act
            var result = await _requestReturningController.UserCreateReturnRequest(assignmentId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var response = Assert.IsType<GeneralBoolResponse>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Equal("An error occurred while registering the user.", response.Message);
        }

        [Fact]
        public async Task CreateReturnRequestAsync_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var assignmentId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var returnRequest = new ReturnRequestResponse { Id = Guid.NewGuid(), AssignmentId = assignmentId };

            _requestReturnServiceMock.Setup(service => service.AddReturnRequestAsync(adminId, assignmentId))
                .ReturnsAsync(returnRequest);

            _requestReturningController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Actor, adminId.ToString()),
                        new Claim(ClaimTypes.NameIdentifier, "testuser")
                    }, "mock"))
                }
            };

            // Act
            var result = await _requestReturningController.CreateReturnRequestAsync(assignmentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralCreateResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Return Request created successfully.", response.Message);
            Assert.Equal(returnRequest, response.Data);
        }

        [Fact]
        public async Task CreateReturnRequestAsync_Exception_ReturnsConflict()
        {
            // Arrange
            var assignmentId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var exceptionMessage = "An unexpected error occurred.";

            _requestReturnServiceMock.Setup(service => service.AddReturnRequestAsync(adminId, assignmentId))
                .ThrowsAsync(new Exception(exceptionMessage));

            _requestReturningController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                                {
                        new Claim(ClaimTypes.Actor, adminId.ToString()),
                        new Claim(ClaimTypes.NameIdentifier, "testuser")
                                }, "mock"))
                }
            };
            // Act
            var result = await _requestReturningController.CreateReturnRequestAsync(assignmentId);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Message);
        }

        [Fact]
        public async Task CreateReturnRequestAsync_ReturnRequestIsNull_ReturnsConflict()
        {
            // Arrange
            var assignmentId = Guid.NewGuid();
            var adminId = Guid.NewGuid();

            _requestReturnServiceMock.Setup(service => service.AddReturnRequestAsync(adminId, assignmentId))
                .ReturnsAsync((ReturnRequestResponse)null);

            _requestReturningController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                                {
                        new Claim(ClaimTypes.Actor, adminId.ToString()),
                        new Claim(ClaimTypes.NameIdentifier, "testuser")
                                }, "mock"))
                }
            };

            // Act
            var result = await _requestReturningController.CreateReturnRequestAsync(assignmentId);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Return Request creation failed.", response.Message);
        }

        [Fact]
        public async Task GetReturnRequests_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            Guid locationId = Guid.NewGuid();
            var requestFilter = new ReturnFilterRequest();
            var returnRequests = new List<ReturnRequestResponse> { new ReturnRequestResponse() };
            var totalCount = 1;

            _requestReturnServiceMock.Setup(service => service.GetReturnRequestResponses(It.IsAny<Guid>(), requestFilter, It.IsAny<int>()))
                .ReturnsAsync((returnRequests, totalCount));

            _requestReturningController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Locality, locationId.ToString()),
                        new Claim(ClaimTypes.NameIdentifier, "testuser")
                    }, "mock"))
                }
            };

            // Act
            var result = await _requestReturningController.GetReturnRequests(requestFilter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralGetsResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Get return requests successfully", response.Message);
            Assert.Equal(returnRequests, response.Data);
            Assert.Equal(totalCount, response.TotalCount);
        }

        [Fact]
        public async Task GetReturnRequests_Exception_ReturnsConflict()
        {
            // Arrange
            Guid locationId = Guid.NewGuid();
            var requestFilter = new ReturnFilterRequest();
            var exceptionMessage = "An unexpected error occurred.";

            _requestReturnServiceMock.Setup(service => service.GetReturnRequestResponses(It.IsAny<Guid>(), requestFilter, It.IsAny<int>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            _requestReturningController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Locality, locationId.ToString()),
                        new Claim(ClaimTypes.NameIdentifier, "testuser")
                    }, "mock"))
                }
            };

            // Act
            var result = await _requestReturningController.GetReturnRequests(requestFilter);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Message);
        }

        [Fact]
        public async Task CompleteRequest_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _requestReturnServiceMock.Setup(service => service.CompleteReturnRequest(id, userId))
                .Returns(Task.CompletedTask);

            _requestReturningController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                                {
                        new Claim(ClaimTypes.Actor, userId.ToString()),
                        new Claim(ClaimTypes.NameIdentifier, "testuser")
                                }, "mock"))
                }
            };
            // Act
            var result = await _requestReturningController.CompleteRequest(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Complete return requests successfully", response.Message);
        }

        [Fact]
        public async Task CompleteRequest_Exception_ReturnsConflict()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var exceptionMessage = "An unexpected error occurred.";

            _requestReturnServiceMock.Setup(service => service.CompleteReturnRequest(id, userId))
                .ThrowsAsync(new Exception(exceptionMessage));

            _requestReturningController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                                {
                        new Claim(ClaimTypes.Actor, userId.ToString()),
                        new Claim(ClaimTypes.NameIdentifier, "testuser")
                                }, "mock"))
                }
            };
            // Act
            var result = await _requestReturningController.CompleteRequest(id);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Message);
        }

        [Fact]
        public async Task CancelRequest_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var id = Guid.NewGuid();

            _requestReturnServiceMock.Setup(service => service.CancelRequest(id))
                .ReturnsAsync(true);

            // Act
            var result = await _requestReturningController.CancelRequest(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Request cancel successfully.", response.Message);
        }

        [Fact]
        public async Task CancelRequest_InvalidRequest_ReturnsConflict()
        {
            // Arrange
            var id = Guid.NewGuid();

            _requestReturnServiceMock.Setup(service => service.CancelRequest(id))
                .ReturnsAsync(false);

            // Act
            var result = await _requestReturningController.CancelRequest(id);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Request cancel failed", response.Message);
        }

        [Fact]
        public async Task CancelRequest_Exception_ReturnsConflict()
        {
            // Arrange
            var id = Guid.NewGuid();
            var exceptionMessage = "An unexpected error occurred.";

            _requestReturnServiceMock.Setup(service => service.CancelRequest(id))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _requestReturningController.CancelRequest(id);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Message);
        }
    }
}
