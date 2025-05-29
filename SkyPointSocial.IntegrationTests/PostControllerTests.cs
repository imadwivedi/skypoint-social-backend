using FluentAssertions;
using SkyPointSocial.Core.ClientModels.Post;
using SkyPointSocial.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace SkyPointSocial.IntegrationTests.Tests
{
    public class PostControllerTests : BaseControllerTest
    {
        public PostControllerTests(IntegrationTestWebApplicationFactory factory) : base(factory) { }

        [Fact]
        public async Task PostCreation_UserCanCreateShortTextPost()
        {
            var auth = await RegisterUserAsync("poster@test.com", "poster", "Test123!");
            SetAuthorizationHeader(auth.Token);

            var request = new CreatePostClientModel
            {
                Content = "This is my short text-based post!"
            };

            var response = await Client.PostAsJsonAsync("/api/post", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var post = await response.Content.ReadFromJsonAsync<PostClientModel>();
            post!.Content.Should().Be(request.Content);
            post.User.Username.Should().Be("poster");
        }

        [Fact]
        public async Task PostCreation_RequiresAuthentication()
        {
            ClearAuthorizationHeader();
            
            var request = new CreatePostClientModel
            {
                Content = "Unauthorized post attempt"
            };

            var response = await Client.PostAsJsonAsync("/api/post", request);
            
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task PostCreation_EmptyContent_NotAllowed()
        {
            var auth = await RegisterUserAsync("empty@test.com", "emptypost", "Test123!");
            SetAuthorizationHeader(auth.Token);

            var request = new CreatePostClientModel { Content = "" };

            var response = await Client.PostAsJsonAsync("/api/post", request);
            
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}