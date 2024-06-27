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

        [Fact]
        public async Task DeleteAssignment_WhenAssignmentNotFound_ReturnsFalse()
        {
            //Arrange
            var id = Guid.NewGuid();
            _assignmentServiceMock.Setup(x => x.DeleteAssignment(id)).ReturnsAsync(false);

            //Act
            var result = await _assignmentController.DeleteAssignment(id);

            //Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
            Assert.False(response.Success);
        }

        [Fact]
        public async Task DeleteAssignment_WhenSuccess_ReturnsTrue()
        {
            //Arrange
            var id = Guid.NewGuid();
            _assignmentServiceMock.Setup(x => x.DeleteAssignment(id)).ReturnsAsync(true);

            //Act
            var result = await _assignmentController.DeleteAssignment(id);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(okResult.Value);
            Assert.True(response.Success);
        }

        [Fact]
        public async Task DeleteAssignment_WhenException_ReturnsError()
        {
            //Arrange
            var id = Guid.NewGuid();
            _assignmentServiceMock.Setup(x => x.DeleteAssignment(id)).ThrowsAsync(new Exception());

            //Act
            var result = await _assignmentController.DeleteAssignment(id);

            //Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
            Assert.False(response.Success);
        }

        //[Fact]
        //public async Task UpdateAssignment_ReturnsOkResult_WhenUpdateSucceeds()
        //{
        //    // Arrange
        //    var id = Guid.NewGuid();
        //    var request = new AssignmentRequest();
        //    _assignmentServiceMock.Setup(x => x.UpdateAssignment(id, request)).ReturnsAsync(true);

        //    // Act
        //    var result = await _assignmentController.UpdateAssignment(id, request);

        //    // Assert
        //    var okResult = Assert.IsType<OkObjectResult>(result);
        //    var response = Assert.IsType<GeneralBoolResponse>(okResult.Value);
        //    Assert.True(response.Success);
        //    Assert.Equal("Update successfully", response.Message);
        //}

        //[Fact]
        //public async Task UpdateAssignment_ReturnsConflict_WhenUpdateFails()
        //{
        //    // Arrange
        //    var id = Guid.NewGuid();
        //    var request = new AssignmentRequest();
        //    _assignmentServiceMock.Setup(x => x.UpdateAssignment(id, request)).ReturnsAsync(false);

        //    // Act
        //    var result = await _assignmentController.UpdateAssignment(id, request);

        //    // Assert
        //    var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        //    var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
        //    Assert.False(response.Success);
        //    Assert.Equal("Update fail", response.Message);
        //}

        [Fact]
        public async Task AdminGetUserAssignmentAsync_ReturnsOkResult_WhenAssignmentsExist()
        {
            // Arrange
            int pageNumber = 1;
            var userId = Guid.NewGuid();
            var assignments = new List<AssignmentResponse> { new AssignmentResponse() };
            _assignmentServiceMock.Setup(x => x.GetUserAssignmentAsync(pageNumber, userId, "", ""))
                .ReturnsAsync((assignments, 1));

            // Act
            var result = await _assignmentController.AdminGetUserAssignmentAsync(pageNumber, userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralGetsResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Assignments of user retrieved successfully.", response.Message);
            Assert.Equal(assignments, response.Data);
            Assert.Equal(1, response.TotalCount);
        }

        [Fact]
        public async Task AdminGetUserAssignmentAsync_ReturnsConflict_WhenNoAssignments()
        {
            // Arrange
            int pageNumber = 1;
            var userId = Guid.NewGuid();
            _assignmentServiceMock.Setup(x => x.GetUserAssignmentAsync(pageNumber, userId, "", ""))
                .ReturnsAsync((new List<AssignmentResponse>(), 0));

            // Act
            var result = await _assignmentController.AdminGetUserAssignmentAsync(pageNumber, userId);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralGetsResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal("No data.", response.Message);
        }

        [Fact]
        public async Task GetAssignmentDetail_ReturnsOkResult_WhenAssignmentExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var assignment = new AssignmentResponse();
            _assignmentServiceMock.Setup(x => x.GetAssignmentDetailAsync(id)).ReturnsAsync(assignment);

            // Act
            var result = await _assignmentController.GetAssignmentDetail(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralGetResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Assignment retrived successfully.", response.Message);
            Assert.Equal(assignment, response.Data);
        }

        [Fact]
        public async Task GetAssignmentDetail_ReturnsConflict_WhenAssignmentNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            _assignmentServiceMock.Setup(x => x.GetAssignmentDetailAsync(id)).ReturnsAsync((AssignmentResponse)null);

            // Act
            var result = await _assignmentController.GetAssignmentDetail(id);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Assignment not found.", response.Message);
        }
    }
}