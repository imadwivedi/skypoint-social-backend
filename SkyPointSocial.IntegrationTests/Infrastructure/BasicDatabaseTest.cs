using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SkyPointSocial.Application.Data;
using SkyPointSocial.Core.Entities;
using SkyPointSocial.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace SkyPointSocial.IntegrationTests
{
    public class BasicDatabaseTest : IClassFixture<IntegrationTestWebApplicationFactory>
    {
        private readonly IntegrationTestWebApplicationFactory _factory;

        public BasicDatabaseTest(IntegrationTestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CanConnectToDatabase_CreateUser_AndRetrieveIt()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                PasswordHash = "hashedpassword123",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Act - Create user
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            // Act - Retrieve user
            var retrievedUser = await dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == "test@example.com");

            // Assert
            retrievedUser.Should().NotBeNull();
            retrievedUser!.Username.Should().Be("testuser");
            retrievedUser.Email.Should().Be("test@example.com");
            retrievedUser.Id.Should().Be(user.Id);
        }
    }
}