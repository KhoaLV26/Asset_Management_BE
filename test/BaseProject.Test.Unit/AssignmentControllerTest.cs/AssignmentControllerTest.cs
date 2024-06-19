using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Application.Services;
using AssetManagement.Domain.Models;
using AssetManagement.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AssetManagement.Test.Unit.AssignmentControllerTest.cs
{
    public class AssignmentControllerTest
    {
        private readonly Mock<IAssignmentService> _assignmentServiceMock;
        private readonly AssignmentController _assignmentController;

        public AssignmentControllerTest()
        {
            _assignmentServiceMock = new Mock<IAssignmentService>();
            _assignmentController = new AssignmentController(_assignmentServiceMock.Object);
        }

        [Fact]
        public async Task CreateAssignment_ValidRequest_ReturnsOkResult()
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

            var expectedResponse = new AssignmentResponse
            {
                Id = Guid.NewGuid(),
                AssignedTo = request.AssignedTo,
                AssignedBy = request.AssignedBy,
                AssignedDate = request.AssignedDate,
                AssetId = request.AssetId,
                Status = Domain.Enums.EnumAssignmentStatus.WaitingForAcceptance
            };

            _assignmentServiceMock.Setup(service => service.AddAssignmentAsync(request))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _assignmentController.CreateAssignment(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralCreateResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Create assignment successfully.", response.Message);
            Assert.Equal(expectedResponse, response.Data);
        }

        [Fact]
        public async Task CreateAssignment_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var request = new AssignmentRequest();
            _assignmentController.ModelState.AddModelError("AssignedTo", "AssignedTo is required.");

            // Act
            var result = await _assignmentController.CreateAssignment(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<GeneralCreateResponse>(badRequestResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Invalid request data.", response.Message);
            Assert.IsType<ModelStateDictionary>(response.Data);
        }

        [Fact]
        public async Task CreateAssignment_ArgumentException_ReturnsConflict()
        {
            // Arrange
            var request = new AssignmentRequest();
            var exceptionMessage = "Invalid argument.";
            _assignmentServiceMock.Setup(service => service.AddAssignmentAsync(request))
                .ThrowsAsync(new ArgumentException(exceptionMessage));

            // Act
            var result = await _assignmentController.CreateAssignment(request);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralCreateResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Message);
        }

        [Fact]
        public async Task CreateAssignment_InvalidOperationException_ReturnsConflict()
        {
            // Arrange
            var request = new AssignmentRequest();
            var exceptionMessage = "Invalid operation.";
            _assignmentServiceMock.Setup(service => service.AddAssignmentAsync(request))
                .ThrowsAsync(new InvalidOperationException(exceptionMessage));

            // Act
            var result = await _assignmentController.CreateAssignment(request);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exceptionMessage, response.Message);
        }

        [Fact]
        public async Task CreateAssignment_Exception_ReturnsInternalServerError()
        {
            // Arrange
            var request = new AssignmentRequest();
            var exceptionMessage = "An unexpected error occurred.";
            _assignmentServiceMock.Setup(service => service.AddAssignmentAsync(request))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _assignmentController.CreateAssignment(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var response = Assert.IsType<GeneralBoolResponse>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Equal("An error occurred while registering the user.", response.Message);
        }
    }
}
