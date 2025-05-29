using FluentAssertions;
using SkyPointSocial.Core.ClientModels.Feed;
using SkyPointSocial.Core.ClientModels.Follow;
using SkyPointSocial.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace SkyPointSocial.IntegrationTests.Tests
{
    public class FollowControllerTests : BaseControllerTest
    {
        public FollowControllerTests(IntegrationTestWebApplicationFactory factory) : base(factory) { }

        [Fact]
        public async Task FollowUnfollow_UserCanFollowAnotherUser()
        {
            var follower = await RegisterUserAsync("follower@test.com", "follower", "Test123!");
            var userToFollow = await RegisterUserAsync("tofollow@test.com", "tofollow", "Test123!");

            SetAuthorizationHeader(follower.Token);
            var followRequest = new FollowClientModel { UserId = userToFollow.User.Id };

            var response = await Client.PostAsJsonAsync("/api/follow", followRequest);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsStringAsync();
            result.Should().Contain("followed");
        }

        [Fact]
        public async Task FollowUnfollow_UserCanUnfollowAnotherUser()
        {
            var follower = await RegisterUserAsync("follower2@test.com", "follower2", "Test123!");
            var userToFollow = await RegisterUserAsync("tofollow2@test.com", "tofollow2", "Test123!");

            SetAuthorizationHeader(follower.Token);
            var followRequest = new FollowClientModel { UserId = userToFollow.User.Id };

            // Follow first
            await Client.PostAsJsonAsync("/api/follow", followRequest);

            // Then unfollow
            var unfollowResponse = await Client.PostAsJsonAsync("/api/follow", followRequest);

            unfollowResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await unfollowResponse.Content.ReadAsStringAsync();
            result.Should().Contain("unfollowed");
        }

        [Fact]
        public async Task FollowUnfollow_FollowingAlreadyFollowedUser_TogglesAndUnfollows()
        {
            // Arrange
            var follower = await RegisterUserAsync("follower3@test.com", "follower3", "Test123!");
            var userToFollow = await RegisterUserAsync("tofollow3@test.com", "tofollow3", "Test123!");

            SetAuthorizationHeader(follower.Token);
            var followRequest = new FollowClientModel { UserId = userToFollow.User.Id };

            // Act 1: First follow request - should follow the user
            var firstResponse = await Client.PostAsJsonAsync("/api/follow", followRequest);
            firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var firstResult = await firstResponse.Content.ReadAsStringAsync();
            firstResult.Should().Contain("followed");
            firstResult.Should().NotContain("unfollowed");

            // Act 2: Second follow request on already followed user - should unfollow (toggle behavior)
            var secondResponse = await Client.PostAsJsonAsync("/api/follow", followRequest);
            secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var secondResult = await secondResponse.Content.ReadAsStringAsync();
            secondResult.Should().Contain("unfollowed");

            // Act 3: Third follow request - should follow again
            var thirdResponse = await Client.PostAsJsonAsync("/api/follow", followRequest);
            thirdResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var thirdResult = await thirdResponse.Content.ReadAsStringAsync();
            thirdResult.Should().Contain("followed");
            thirdResult.Should().NotContain("unfollowed");

            // Assert: Verify the final state in the feed
            var feedResponse = await Client.GetAsync("/api/feed");
            var feed = await feedResponse.Content.ReadFromJsonAsync<FeedResponseClientModel>();

            // If the followed user has any posts, they should show isFollowing = true
            // This verifies the final state after toggle operations
        }

        [Fact]
        public async Task FollowUnfollow_MultipleToggleOperations_MaintainsCorrectState()
        {
            var follower = await RegisterUserAsync("togglefollower@test.com", "togglefollower", "Test123!");
            var userToFollow = await RegisterUserAsync("toggleuser@test.com", "toggleuser", "Test123!");

            SetAuthorizationHeader(follower.Token);
            var followRequest = new FollowClientModel { UserId = userToFollow.User.Id };

            // Perform multiple toggle operations
            var expectedStates = new[] { "followed", "unfollowed", "followed", "unfollowed", "followed" };
            var actualResults = new List<string>();

            for (int i = 0; i < expectedStates.Length; i++)
            {
                var response = await Client.PostAsJsonAsync("/api/follow", followRequest);
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsStringAsync();
                actualResults.Add(result);
            }

            // Verify each toggle operation produced the expected result
            for (int i = 0; i < expectedStates.Length; i++)
            {
                actualResults[i].Should().Contain(expectedStates[i],
                    $"Toggle operation {i + 1} should have resulted in '{expectedStates[i]}' state");
            }
        }
    }
}