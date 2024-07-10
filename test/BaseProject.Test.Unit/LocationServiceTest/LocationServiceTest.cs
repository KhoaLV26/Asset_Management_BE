using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Application.Services.Implementations;
using AssetManagement.Domain.Constants;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Interfaces;
using AssetManagement.Infrastructure.Helpers;
using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AssetManagement.Test.Unit.LocationServiceTest
{
    public class LocationServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ICryptographyHelper> _mockCryptographyHelper;
        private readonly LocationService _locationService;

        public LocationServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockCryptographyHelper = new Mock<ICryptographyHelper>();
            _locationService = new LocationService(_mockUnitOfWork.Object, _mockMapper.Object, _mockCryptographyHelper.Object);
        }

        [Fact]
        public async Task GetAllLocationAsync_ReturnsLocations()
        {
            // Arrange
            var locations = new List<Location> { new Location(), new Location() };
            var locationResponses = new List<LocationResponse> { new LocationResponse(), new LocationResponse() };
            _mockUnitOfWork.Setup(uow => uow.LocationRepository.GetAllAsync(
                It.IsAny<int>(), It.IsAny<Expression<Func<Location, bool>>>(),
                It.IsAny<Func<IQueryable<Location>, IOrderedQueryable<Location>>>(),
                It.IsAny<string>(), It.IsAny<Expression<Func<Location, bool>>>(),
                It.IsAny<int>()))
                .ReturnsAsync((locations, 2));
            _mockMapper.Setup(m => m.Map<IEnumerable<LocationResponse>>(It.IsAny<IEnumerable<Location>>()))
                .Returns(locationResponses);

            // Act
            var result = await _locationService.GetAllLocationAsync(1, null);

            // Assert
            Assert.Equal(locationResponses, result.data);
            Assert.Equal(2, result.totalCount);
        }

        [Fact]
        public async Task CreateLocationAsync_CreatesLocationAndUser()
        {
            // Arrange
            var request = new LocationCreateRequest { Name = "Test Location", Code = "TL" };
            var newLocation = new Location { Id = Guid.NewGuid(), Name = "Test Location", Code = "TL" };
            _mockUnitOfWork.Setup(uow => uow.LocationRepository.GetAsync(It.IsAny<Expression<Func<Location, bool>>>()))
                .ReturnsAsync((Location)null);
            _mockUnitOfWork.Setup(uow => uow.UserRepository.GetAllAsync())
                .ReturnsAsync(new List<User>());
            _mockCryptographyHelper.Setup(ch => ch.GenerateSalt()).Returns("salt");
            _mockCryptographyHelper.Setup(ch => ch.HashPassword(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("hashedPassword");
            _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(1);

            // Act
            var result = await _locationService.CreateLocationAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newLocation.Name, result.Name);
            Assert.Equal(newLocation.Code, result.Code);
            _mockUnitOfWork.Verify(uow => uow.LocationRepository.AddAsync(It.IsAny<Location>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.UserRepository.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task CreateLocationAsync_ThrowsException_WhenLocationNameExists()
        {
            // Arrange
            var request = new LocationCreateRequest { Name = "Existing Location", Code = "EL" };
            _mockUnitOfWork.Setup(uow => uow.LocationRepository.GetAsync(It.IsAny<Expression<Func<Location, bool>>>()))
                .ReturnsAsync(new Location());

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _locationService.CreateLocationAsync(request));
        }

        [Fact]
        public async Task UpdateLocationAsync_UpdatesLocation()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var request = new LocationUpdateRequest { Name = "Updated Location", Code = "UL" };
            var existingLocation = new Location { Id = locationId, Name = "Old Location", Code = "OL" };
            _mockUnitOfWork.Setup(uow => uow.LocationRepository.GetAsync(It.IsAny<Expression<Func<Location, bool>>>()))
                .ReturnsAsync(existingLocation);
            _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(1);
            _mockMapper.Setup(m => m.Map<LocationResponse>(It.IsAny<Location>()))
                .Returns(new LocationResponse { Id = locationId, Name = "Updated Location", Code = "UL" });

            // Act
            var result = await _locationService.UpdateLocationAsync(locationId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.Name, result.Name);
            Assert.Equal(request.Code, result.Code);
            _mockUnitOfWork.Verify(uow => uow.LocationRepository.Update(It.IsAny<Location>()), Times.Once);
        }

        [Fact]
        public async Task UpdateLocationAsync_ThrowsException_WhenLocationNotFound()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var request = new LocationUpdateRequest { Name = "Updated Location", Code = "UL" };
            _mockUnitOfWork.Setup(uow => uow.LocationRepository.GetAsync(It.IsAny<Expression<Func<Location, bool>>>()))
                .ReturnsAsync((Location)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _locationService.UpdateLocationAsync(locationId, request));
        }

        [Fact]
        public async Task GetLocationByIdAsync_ReturnsLocation()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var location = new Location { Id = locationId, Name = "Test Location", Code = "TL" };
            _mockUnitOfWork.Setup(uow => uow.LocationRepository.GetAsync(It.IsAny<Expression<Func<Location, bool>>>()))
                .ReturnsAsync(location);
            _mockMapper.Setup(m => m.Map<LocationResponse>(It.IsAny<Location>()))
                .Returns(new LocationResponse { Id = locationId, Name = "Test Location", Code = "TL" });

            // Act
            var result = await _locationService.GetLocationByIdAsync(locationId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(location.Id, result.Id);
            Assert.Equal(location.Name, result.Name);
            Assert.Equal(location.Code, result.Code);
        }

        [Fact]
        public async Task GetLocationByIdAsync_ThrowsException_WhenLocationNotFound()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            _mockUnitOfWork.Setup(uow => uow.LocationRepository.GetAsync(It.IsAny<Expression<Func<Location, bool>>>()))
                .ReturnsAsync((Location)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _locationService.GetLocationByIdAsync(locationId));
        }

        [Theory]
        [InlineData("asc", true)]
        [InlineData("desc", false)]
        public async Task GetAllLocationAsync_SortsByCode(string sortOrder, bool ascending)
        {
            // Arrange
            var locations = new List<Location>
        {
            new Location { Code = "B" },
            new Location { Code = "A" },
            new Location { Code = "C" }
        };
            _mockUnitOfWork.Setup(uow => uow.LocationRepository.GetAllAsync(
                It.IsAny<int>(), It.IsAny<Expression<Func<Location, bool>>>(),
                It.IsAny<Func<IQueryable<Location>, IOrderedQueryable<Location>>>(),
                It.IsAny<string>(), It.IsAny<Expression<Func<Location, bool>>>(),
                It.IsAny<int>()))
                .Callback<int, Expression<Func<Location, bool>>, Func<IQueryable<Location>, IOrderedQueryable<Location>>, string, Expression<Func<Location, bool>>, int>(
                    (page, filter, orderBy, includeProperties, prioritize, pageSize) =>
                    {
                        var orderedLocations = orderBy(locations.AsQueryable());
                        locations = orderedLocations.ToList();
                    })
                .ReturnsAsync((locations, locations.Count));

            // Act
            await _locationService.GetAllLocationAsync(1, null, sortOrder, SortConstants.Location.SORT_BY_CODE);

            // Assert
            if (ascending)
            {
                Assert.Equal("A", locations[0].Code);
                Assert.Equal("C", locations[2].Code);
            }
            else
            {
                Assert.Equal("C", locations[0].Code);
                Assert.Equal("A", locations[2].Code);
            }
        }

        [Fact]
        public async Task GetAllLocationAsync_PrioritizesNewLocationCode()
        {
            // Arrange
            var newLocationCode = "NEW";
            var locations = new List<Location>
        {
            new Location { Code = "B" },
            new Location { Code = newLocationCode },
            new Location { Code = "A" }
        };
            _mockUnitOfWork.Setup(uow => uow.LocationRepository.GetAllAsync(
                It.IsAny<int>(), It.IsAny<Expression<Func<Location, bool>>>(),
                It.IsAny<Func<IQueryable<Location>, IOrderedQueryable<Location>>>(),
                It.IsAny<string>(), It.IsAny<Expression<Func<Location, bool>>>(),
                It.IsAny<int>()))
                .Callback<int, Expression<Func<Location, bool>>, Func<IQueryable<Location>, IOrderedQueryable<Location>>, string, Expression<Func<Location, bool>>, int>(
                    (page, filter, orderBy, includeProperties, prioritize, pageSize) =>
                    {
                        locations = locations.AsQueryable().OrderByDescending(prioritize.Compile()).ThenBy(l => l.Code).ToList();
                    })
                .ReturnsAsync((locations, locations.Count));

            // Act
            await _locationService.GetAllLocationAsync(1, null, "asc", SortConstants.Location.SORT_BY_CODE, newLocationCode);

            // Assert
            Assert.Equal(newLocationCode, locations[0].Code);
        }

        [Fact]
        public async Task CreateLocationAsync_ThrowsException_WhenLocationCodeExists()
        {
            // Arrange
            var request = new LocationCreateRequest { Name = "New Location", Code = "NL" };
            _mockUnitOfWork.Setup(uow => uow.LocationRepository.GetAsync(It.Is<Expression<Func<Location, bool>>>(expr => expr.Compile()(new Location { Code = "NL" }))))
                .ReturnsAsync(new Location());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _locationService.CreateLocationAsync(request));
            Assert.Equal("Location code already existed", exception.Message);
        }

        [Fact]
        public async Task CreateLocationAsync_AppendsPaddedNumberToUsername_WhenUserExists()
        {
            // Arrange
            var request = new LocationCreateRequest { Name = "Test Location", Code = "TL" };
            _mockUnitOfWork.Setup(uow => uow.LocationRepository.GetAsync(It.IsAny<Expression<Func<Location, bool>>>()))
                .ReturnsAsync((Location)null);
            _mockUnitOfWork.Setup(uow => uow.UserRepository.GetAllAsync())
                .ReturnsAsync(new List<User> { new User { Username = "admintl" }, new User { Username = "admintl1" } });
            _mockCryptographyHelper.Setup(ch => ch.GenerateSalt()).Returns("salt");
            _mockCryptographyHelper.Setup(ch => ch.HashPassword(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("hashedPassword");
            _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(1);

            // Act
            var result = await _locationService.CreateLocationAsync(request);

            // Assert
            Assert.Equal("admintl2", result.UserName);
        }

        [Fact]
        public async Task UpdateLocationAsync_ThrowsException_WhenLocationNameExists()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var request = new LocationUpdateRequest { Name = "Existing Location", Code = "EL" };
            _mockUnitOfWork.Setup(uow => uow.LocationRepository.GetAsync(It.Is<Expression<Func<Location, bool>>>(expr => expr.Compile()(new Location { Id = locationId }))))
                .ReturnsAsync(new Location { Id = locationId });
            _mockUnitOfWork.Setup(uow => uow.LocationRepository.GetAsync(It.Is<Expression<Func<Location, bool>>>(expr => expr.Compile()(new Location { Name = "Existing Location" }))))
                .ReturnsAsync(new Location { Id = Guid.NewGuid() });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _locationService.UpdateLocationAsync(locationId, request));
            Assert.Equal("Location name already existed", exception.Message);
        }

        [Fact]
        public async Task UpdateLocationAsync_ThrowsException_WhenLocationCodeExists()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var request = new LocationUpdateRequest { Name = "Updated Location", Code = "EL" };
            _mockUnitOfWork.Setup(uow => uow.LocationRepository.GetAsync(It.Is<Expression<Func<Location, bool>>>(expr => expr.Compile()(new Location { Id = locationId }))))
                .ReturnsAsync(new Location { Id = locationId });
            _mockUnitOfWork.Setup(uow => uow.LocationRepository.GetAsync(It.Is<Expression<Func<Location, bool>>>(expr => expr.Compile()(new Location { Code = "EL" }))))
                .ReturnsAsync(new Location { Id = Guid.NewGuid() });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _locationService.UpdateLocationAsync(locationId, request));
            Assert.Equal("Location code already existed", exception.Message);
        }

        [Fact]
        public async Task UpdateLocationAsync_ThrowsException_WhenCommitFails()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var request = new LocationUpdateRequest { Name = "Updated Location", Code = "UL" };
            var existingLocation = new Location { Id = locationId, Name = "Old Location", Code = "OL" };
            _mockUnitOfWork.Setup(uow => uow.LocationRepository.GetAsync(It.IsAny<Expression<Func<Location, bool>>>()))
                .ReturnsAsync(existingLocation);
            _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(0); // Simulate failed commit

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _locationService.UpdateLocationAsync(locationId, request)
            );
            Assert.Equal("Failed to update location", exception.Message);
        }

        [Fact]
        public async Task CreateLocationAsync_ThrowsException_WhenCommitFails()
        {
            // Arrange
            var request = new LocationCreateRequest { Name = "Test Location", Code = "TL" };
            _mockUnitOfWork.Setup(uow => uow.LocationRepository.GetAsync(It.IsAny<Expression<Func<Location, bool>>>()))
                .ReturnsAsync((Location)null);
            _mockUnitOfWork.Setup(uow => uow.UserRepository.GetAllAsync())
                .ReturnsAsync(new List<User>());
            _mockCryptographyHelper.Setup(ch => ch.GenerateSalt()).Returns("salt");
            _mockCryptographyHelper.Setup(ch => ch.HashPassword(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("hashedPassword");
            _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(0); // Simulate failed commit

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _locationService.CreateLocationAsync(request)
            );
            Assert.Equal("Failed to create location", exception.Message);
        }
    }
}
