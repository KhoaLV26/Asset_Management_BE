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
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AssetManagement.Test.Unit.AssignmentServiceTest
{
    public class DeleteAssignmentTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperUnitOfWork;
        private readonly AssignmentService _assignmentService;

        public DeleteAssignmentTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperUnitOfWork = new Mock<IMapper>();
            _assignmentService = new AssignmentService(_unitOfWorkMock.Object, _mapperUnitOfWork.Object);
        }

        [Fact]
        public async Task DeleteAssignment_WhenAssignmentNotFound_ReturnsFalse()
        {
            //Arrange
            var id = Guid.NewGuid();
            _unitOfWorkMock.Setup(x => x.AssignmentRepository.GetAsync(It.IsAny<Expression<Func<Assignment, bool>>>(), It.IsAny<Expression<Func<Assignment, object>>[]>())).ReturnsAsync((Assignment)null);

            //Act
            var result = await _assignmentService.DeleteAssignment(id);

            //Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAssignment_WhenSuccess_ReturnsTrue()
        {
            //Arrange
            var id = Guid.NewGuid();
            var assignment = new Assignment
            {
                Id = id,
                IsDeleted = false,
                Status = EnumAssignmentStatus.WaitingForAcceptance,
                Asset = new Asset
                {
                    Id = Guid.NewGuid(),
                    Status = EnumAssetStatus.Assigned
                }
            };
            _unitOfWorkMock.Setup(x => x.AssignmentRepository.GetAsync(It.IsAny<Expression<Func<Assignment, bool>>>(), It.IsAny<Expression<Func<Assignment, object>>[]>())).ReturnsAsync(assignment);
            _unitOfWorkMock.Setup(x => x.AssignmentRepository.SoftDelete(assignment)).Verifiable();
            _unitOfWorkMock.Setup(x => x.CommitAsync()).ReturnsAsync(1);

            //Act
            var result = await _assignmentService.DeleteAssignment(id);

            //Assert
            Assert.True(result);
        }
    }
}