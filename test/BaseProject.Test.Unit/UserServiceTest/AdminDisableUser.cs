using AssetManagement.Application.Services.Implementations;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Domain.Interfaces;
using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AssetManagement.Test.Unit.UserServiceTest
{
    public class AdminDisableUser
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly UserService _userService;

        public AdminDisableUser()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _userService = new UserService(_mockUnitOfWork.Object, null, null, null);
        }

        [Fact]
        public async Task DisableUserAsync_UserNotFound_ThrowsArgumentException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockUnitOfWork.Setup(uow => uow.UserRepository.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
                .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _userService.DisableUser(userId));
        }

        [Fact]
        public async Task DisableUserAsync_UserHasActiveAssignments_ReturnsFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId };
            var activeAssignment = new Assignment { Status = EnumAssignmentStatus.Accepted };

            _mockUnitOfWork.Setup(uow => uow.UserRepository.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
                .ReturnsAsync(user);
            _mockUnitOfWork.Setup(uow => uow.AssignmentRepository.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Assignment, bool>>>()))
                .ReturnsAsync(new List<Assignment> { activeAssignment });

            // Act
            var result = await _userService.DisableUser(userId);

            // Assert
            Assert.False(result);
            Assert.False(user.IsDeleted);
            _mockUnitOfWork.Verify(uow => uow.UserRepository.Update(It.IsAny<User>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(), Times.Never);
        }

        //[Fact]
        //public async Task DisableUserAsync_UserHasOnlyReturnedAssignments_ReturnsTrue()
        //{
        //    // Arrange
        //    var userId = Guid.NewGuid();
        //    var user = new User { Id = userId };
        //    var returnedAssignment = new Assignment { Status = EnumAssignmentStatus.Returned };

        //    _mockUnitOfWork.Setup(uow => uow.UserRepository.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
        //        .ReturnsAsync(user);
        //    _mockUnitOfWork.Setup(uow => uow.AssignmentRepository.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Assignment, bool>>>()))
        //        .ReturnsAsync(new List<Assignment> { returnedAssignment });
        //    _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(1);

        //    // Act
        //    var result = await _userService.DisableUser(userId);

        //    // Assert
        //    Assert.True(result);
        //    Assert.True(user.IsDeleted);
        //    _mockUnitOfWork.Verify(uow => uow.UserRepository.Update(user), Times.Once);
        //    _mockUnitOfWork.Verify(uow => uow.CommitAsync(), Times.Once);
        //}

        [Fact]
        public async Task DisableUserAsync_UserHasNoAssignments_ReturnsTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId };

            _mockUnitOfWork.Setup(uow => uow.UserRepository.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
                .ReturnsAsync(user);
            _mockUnitOfWork.Setup(uow => uow.AssignmentRepository.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Assignment, bool>>>()))
                .ReturnsAsync(new List<Assignment>());
            _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(1);

            // Act
            var result = await _userService.DisableUser(userId);

            // Assert
            Assert.True(result);
            Assert.True(user.IsDeleted);
            _mockUnitOfWork.Verify(uow => uow.UserRepository.Update(user), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task DisableUserAsync_CommitFails_ReturnsFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId };

            _mockUnitOfWork.Setup(uow => uow.UserRepository.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
                .ReturnsAsync(user);
            _mockUnitOfWork.Setup(uow => uow.AssignmentRepository.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Assignment, bool>>>()))
                .ReturnsAsync(new List<Assignment>());
            _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(0);

            // Act
            var result = await _userService.DisableUser(userId);

            // Assert
            Assert.False(result);
            Assert.True(user.IsDeleted); // The user is marked as deleted, but the change isn't persisted
            _mockUnitOfWork.Verify(uow => uow.UserRepository.Update(user), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(), Times.Once);
        }
    }

}
