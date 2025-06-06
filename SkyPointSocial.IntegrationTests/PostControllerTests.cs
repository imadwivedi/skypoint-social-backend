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

        [Fact]
        public async Task PostSearch_WithQuery_ReturnsFilteredResults()
        {
            var user1 = await RegisterUserAsync("searcher1@test.com", "searcher1", "Test123!");
            var user2 = await RegisterUserAsync("searcher2@test.com", "searcher2", "Test123!");

            SetAuthorizationHeader(user1.Token);
            var post1 = await CreatePostAsync("This is about cats and animals");
            var post2 = await CreatePostAsync("Programming in C# is amazing");

            SetAuthorizationHeader(user2.Token);
            var post3 = await CreatePostAsync("I love cats and dogs");
            var post4 = await CreatePostAsync("JavaScript programming tutorial");

            SetAuthorizationHeader(user1.Token);

            var response = await Client.GetAsync("/api/posts/search?query=cats");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var searchResult = await response.Content.ReadFromJsonAsync<PostSearchResultClientModel>();
            searchResult!.Posts.Should().HaveCount(2);
            searchResult.TotalCount.Should().Be(2);
            searchResult.HasMore.Should().BeFalse();
            searchResult.Posts.All(p => p.Content.ToLower().Contains("cats")).Should().BeTrue();
        }

        [Fact]
        public async Task PostSearch_WithPagination_ReturnsCorrectResults()
        {
            var user = await RegisterUserAsync("paginator@test.com", "paginator", "Test123!");
            SetAuthorizationHeader(user.Token);

            var posts = new List<PostClientModel>();
            for (int i = 1; i <= 15; i++)
            {
                var post = await CreatePostAsync($"Search result post number {i}");
                posts.Add(post);
                await Task.Delay(10);
            }

            // Test first page
            var page1Response = await Client.GetAsync("/api/posts/search?query=Search&page=1&pageSize=5");
            page1Response.StatusCode.Should().Be(HttpStatusCode.OK);

            var page1Result = await page1Response.Content.ReadFromJsonAsync<PostSearchResultClientModel>();
            page1Result!.Posts.Should().HaveCount(5);
            page1Result.TotalCount.Should().Be(15);
            page1Result.HasMore.Should().BeTrue();

            var page2Response = await Client.GetAsync("/api/posts/search?query=Search&page=2&pageSize=5");
            var page2Result = await page2Response.Content.ReadFromJsonAsync<PostSearchResultClientModel>();
            page2Result!.Posts.Should().HaveCount(5);
            page2Result.TotalCount.Should().Be(15);
            page2Result.HasMore.Should().BeTrue();

            // Test last page
            var page3Response = await Client.GetAsync("/api/posts/search?query=Search&page=3&pageSize=5");
            var page3Result = await page3Response.Content.ReadFromJsonAsync<PostSearchResultClientModel>();
            page3Result!.Posts.Should().HaveCount(5);
            page3Result.TotalCount.Should().Be(15);
            page3Result.HasMore.Should().BeFalse();

            // Verify no duplicate posts across pages
            var allPostIds = page1Result.Posts.Concat(page2Result.Posts).Concat(page3Result.Posts)
                .Select(p => p.Id).ToList();
            allPostIds.Should().OnlyHaveUniqueItems();
        }
    }
}