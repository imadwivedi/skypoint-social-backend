using FluentAssertions;
using SkyPointSocial.Core.ClientModels.Auth;
using SkyPointSocial.Core.ClientModels.User;
using SkyPointSocial.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace SkyPointSocial.IntegrationTests.Tests
{
    public class AuthControllerTests : IClassFixture<IntegrationTestWebApplicationFactory>
    {
        private readonly IntegrationTestWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public AuthControllerTests(IntegrationTestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task SignUp_WithValidData_CreatesUserAndReturnsToken()
        {
            var request = new CreateUserClientModel
            {
                Email = "newuser@test.com",
                Username = "newuser",
                Password = "Test123!@#",
                FirstName = "New",
                LastName = "User"
            };

            var response = await _client.PostAsJsonAsync("/api/signup", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<AuthResponseClientModel>();
            result.Should().NotBeNull();
            result!.Token.Should().NotBeNullOrEmpty();
            result.User.Email.Should().Be(request.Email);
            result.User.Username.Should().Be(request.Username);
        }

        [Fact]
        public async Task SignUp_WithDuplicateEmail_ReturnsConflict()
        {
            var request = new CreateUserClientModel
            {
                Email = "duplicate@test.com",
                Username = "user1",
                Password = "Test123!",
                FirstName = "Test",
                LastName = "User"
            };

            await _client.PostAsJsonAsync("/api/signup", request);

            request.Username = "user2";
            var response = await _client.PostAsJsonAsync("/api/signup", request);
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task SignUp_WithDuplicateUsername_ReturnsConflict()
        {
            var request = new CreateUserClientModel
            {
                Email = "user1@test.com",
                Username = "duplicateusername",
                Password = "Test123!",
                FirstName = "Test",
                LastName = "User"
            };

            await _client.PostAsJsonAsync("/api/signup", request);
            request.Email = "user2@test.com";
            var response = await _client.PostAsJsonAsync("/api/signup", request);

            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsToken()
        {
            var email = "login@test.com";
            var password = "Test123!";
            
            await _client.PostAsJsonAsync("/api/signup", new CreateUserClientModel
            {
                Email = email,
                Username = "loginuser",
                Password = password,
                FirstName = "Test",
                LastName = "User"
            });

            var loginRequest = new LoginClientModel
            {
                Email = email,
                Password = password
            };

            var response = await _client.PostAsJsonAsync("/api/login", loginRequest);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<AuthResponseClientModel>();
            result.Should().NotBeNull();
            result!.Token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            var request = new LoginClientModel
            {
                Email = "nonexistent@test.com",
                Password = "WrongPassword"
            };
            var response = await _client.PostAsJsonAsync("/api/login", request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Logout_WithValidSession_ReturnsSessionDuration()
        {
            var signupRequest = new CreateUserClientModel
            {
                Email = "logout@test.com",
                Username = "logoutuser",
                Password = "Test123!",
                FirstName = "Test",
                LastName = "User"
            };

            var signupResponse = await _client.PostAsJsonAsync("/api/signup", signupRequest);
            var auth = await signupResponse.Content.ReadFromJsonAsync<AuthResponseClientModel>();
            
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth!.Token);
            
            await Task.Delay(1000);
            var response = await _client.PostAsync("/api/logout", null);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsStringAsync();
            result.Should().Contain("sessionDuration");
            result.Should().Contain("Logged out successfully");
        }
    }
}