using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SkyPointSocial.Application.Data;
using SkyPointSocial.Core.ClientModels.Auth;
using SkyPointSocial.Core.ClientModels.User;
using SkyPointSocial.Core.ClientModels.Post;
using SkyPointSocial.Core.ClientModels.Comment;
using SkyPointSocial.Core.ClientModels.Vote;
using SkyPointSocial.Core.ClientModels.Follow;
using SkyPointSocial.Core.Entities;
using SkyPointSocial.IntegrationTests.Infrastructure;

namespace SkyPointSocial.IntegrationTests
{
    public abstract class BaseControllerTest : IClassFixture<IntegrationTestWebApplicationFactory>, IAsyncLifetime
    {
        protected readonly IntegrationTestWebApplicationFactory Factory;
        protected readonly HttpClient Client;
        protected readonly JsonSerializerOptions JsonOptions;

        protected BaseControllerTest(IntegrationTestWebApplicationFactory factory)
        {
            Factory = factory;
            Client = factory.CreateClient();
            JsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task InitializeAsync()
        {
            await Factory.ResetDatabaseAsync();
            Client.DefaultRequestHeaders.Authorization = null;
        }

        public Task DisposeAsync() => Task.CompletedTask;

        #region Helper Methods

        protected async Task<AuthResponseClientModel> RegisterUserAsync(
            string email, 
            string username, 
            string password, 
            string firstName = "Test", 
            string lastName = "User")
        {
            var request = new CreateUserClientModel
            {
                Email = email,
                Username = username,
                Password = password,
                FirstName = firstName,
                LastName = lastName
            };

            var response = await Client.PostAsJsonAsync("/api/signup", request);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<AuthResponseClientModel>(JsonOptions) 
                ?? throw new InvalidOperationException("Failed to register user");
        }

        protected async Task<AuthResponseClientModel> LoginUserAsync(string email, string password)
        {
            var request = new LoginClientModel
            {
                Email = email,
                Password = password
            };

            var response = await Client.PostAsJsonAsync("/api/login", request);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<AuthResponseClientModel>(JsonOptions)
                ?? throw new InvalidOperationException("Failed to login user");
        }

        protected void SetAuthorizationHeader(string token)
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        protected void ClearAuthorizationHeader()
        {
            Client.DefaultRequestHeaders.Authorization = null;
        }

        protected async Task<PostClientModel> CreatePostAsync(string content)
        {
            var request = new CreatePostClientModel { Content = content };
            var response = await Client.PostAsJsonAsync("/api/post", request);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<PostClientModel>(JsonOptions)
                ?? throw new InvalidOperationException("Failed to create post");
        }

        protected async Task<CommentClientModel> CreateCommentAsync(Guid postId, string content, Guid? parentCommentId = null)
        {
            var request = new CreateCommentClientModel 
            { 
                Content = content,
                ParentCommentId = parentCommentId
            };
            
            var response = await Client.PostAsJsonAsync($"/api/comment/{postId}", request);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<CommentClientModel>(JsonOptions)
                ?? throw new InvalidOperationException("Failed to create comment");
        }

        protected async Task VoteOnPostAsync(Guid postId, VoteType voteType)
        {
            var request = new CreateVoteClientModel
            {
                PostId = postId,
                VoteType = (int)voteType
            };
            
            var response = await Client.PostAsJsonAsync("/api/vote", request);
            response.EnsureSuccessStatusCode();
        }

        protected async Task FollowUserAsync(Guid userId)
        {
            var request = new FollowClientModel { UserId = userId };
            var response = await Client.PostAsJsonAsync("/api/follow", request);
            response.EnsureSuccessStatusCode();
        }

        protected async Task<T> GetAsync<T>(string url)
        {
            var response = await Client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<T>(JsonOptions)
                ?? throw new InvalidOperationException($"Failed to get {typeof(T).Name}");
        }

        protected IServiceScope CreateScope() => Factory.Services.CreateScope();

        #endregion
    }
}