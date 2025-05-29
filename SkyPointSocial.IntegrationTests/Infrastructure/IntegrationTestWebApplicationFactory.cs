using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Respawn;
using SkyPointSocial.Application.Data;
using Testcontainers.PostgreSql;

namespace SkyPointSocial.IntegrationTests.Infrastructure
{
    public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private PostgreSqlContainer _postgres = null!;
        private Respawner _respawner = null!;
        private string _connectionString = null!;

        public async Task InitializeAsync()
        {
            // Start PostgreSQL container first
            _postgres = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .WithDatabase("skypoint_social_test")
                .WithUsername("skypoint_user")
                .WithPassword("skypoint_password")
                .Build();

            await _postgres.StartAsync();
            
            _connectionString = _postgres.GetConnectionString();

            // Create the database schema
            using (var scope = Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await dbContext.Database.EnsureCreatedAsync();
            }

            // Initialize Respawner
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            
            _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = new[] { "public" }
            });
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Configure test environment
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((context, config) =>
            {                
                // Add test configuration
                var testConfig = new Dictionary<string, string>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;Username=test;Password=test",
                    ["Jwt:Key"] = "your-super-secret-key-for-testing-at-least-32-characters-long",
                    ["Jwt:Issuer"] = "SkyPointSocial",
                    ["Jwt:Audience"] = "SkyPointSocialUsers",
                    ["Logging:LogLevel:Default"] = "Warning"
                };
                
                config.AddInMemoryCollection(testConfig);
            });

            builder.ConfigureServices(services =>
            {
                // Remove the app's DbContext
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add DbContext using in-memory database for now
                // This will be replaced with PostgreSQL connection after container starts
                services.AddDbContext<AppDbContext>(options =>
                {
                    if (!string.IsNullOrEmpty(_connectionString))
                    {
                        options.UseNpgsql(_connectionString);
                    }
                });

                // Replace the DbContext again after all services are configured
                services.PostConfigure<DbContextOptions<AppDbContext>>(options =>
                {
                    if (!string.IsNullOrEmpty(_connectionString))
                    {
                        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                        optionsBuilder.UseNpgsql(_connectionString);
                    }
                });
            });
        }

        public async Task ResetDatabaseAsync()
        {
            if (_respawner != null && !string.IsNullOrEmpty(_connectionString))
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                await _respawner.ResetAsync(connection);
            }
        }

        public new async Task DisposeAsync()
        {
            if (_postgres != null)
            {
                await _postgres.DisposeAsync();
            }
        }
    }
}