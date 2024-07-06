using AssetManagement.Application.Services.Implementations;
using AssetManagement.Domain.Entities;
using AssetManagement.Infrastructure.Helpers;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace AssetManagement.Test.Unit.AuthServiceTest
{
    public class TokenServiceTest
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ICryptographyHelper> _cryptographyHelperMock;
        private readonly TokenService _tokenService;

        public TokenServiceTest()
        {
            _configurationMock = new Mock<IConfiguration>();
            _cryptographyHelperMock = new Mock<ICryptographyHelper>();
            _tokenService = new TokenService(_configurationMock.Object, _cryptographyHelperMock.Object);
        }

        [Fact]
        public void GenerateToken_ValidUser_ShouldReturnToken()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Role = new Role { Name = "Admin" },
                LocationId = Guid.NewGuid()
            };

            var key = Encoding.UTF8.GetBytes(new string('a', 32)); // Use a 32-character key
            _configurationMock.Setup(c => c["Jwt:Key"]).Returns(Encoding.UTF8.GetString(key));
            _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("myissuer");

            // Act
            var token = _tokenService.GenerateToken(user);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);

            var handler = new JwtSecurityTokenHandler();
            var decodedToken = handler.ReadJwtToken(token);

            Assert.Equal(user.Username, decodedToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            Assert.Equal(user.Role.Name, decodedToken.Claims.First(c => c.Type == ClaimTypes.Role).Value);
            Assert.Equal(user.LocationId.ToString(), decodedToken.Claims.First(c => c.Type == ClaimTypes.Locality).Value);
            Assert.Equal(user.Id.ToString(), decodedToken.Claims.First(c => c.Type == ClaimTypes.Actor).Value);
        }
    }
}
