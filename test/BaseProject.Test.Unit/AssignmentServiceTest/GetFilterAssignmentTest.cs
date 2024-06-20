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

        [Fact]
        public async Task GetAssignmentDetailAsync_ValidId_ReturnsAssignmentResponse()
        {
            var id = Guid.NewGuid();

            var assignment = new Assignment
            {
                Id = id,
                Asset = new Asset
                {
                    AssetCode = "Test Code",
                    AssetName = "Test Name"
                },
                UserTo = new User { Username = "User1" },
                UserBy = new User { Username = "User2" }
            };

            _mockUnitOfWork.Setup(u => u.AssignmentRepository.GetAssignmentDetailAsync(id))
                .ReturnsAsync(assignment);

            var result = await _assignmentService.GetAssignmentDetailAsync(id);

            Assert.NotNull(result);
            Assert.IsType<AssignmentResponse>(result);
            Assert.Equal(assignment.Id, result.Id);
            Assert.Equal(assignment.Asset.AssetCode, result.AssetCode);
            Assert.Equal(assignment.Asset.AssetName, result.AssetName);
            Assert.Equal(assignment.UserTo.Username, result.To);
            Assert.Equal(assignment.UserBy.Username, result.By);
        }
    }
}
