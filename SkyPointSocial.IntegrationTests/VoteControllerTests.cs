using FluentAssertions;
using SkyPointSocial.Core.ClientModels.Feed;
using SkyPointSocial.Core.ClientModels.Vote;
using SkyPointSocial.Core.Entities;
using SkyPointSocial.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace SkyPointSocial.IntegrationTests.Tests
{
    public class VoteControllerTests : BaseControllerTest
    {
        public VoteControllerTests(IntegrationTestWebApplicationFactory factory) : base(factory) { }

        [Fact]
        public async Task Voting_UserCanUpvotePost()
        {
            var poster = await RegisterUserAsync("poster@test.com", "poster", "Test123!");
            var voter = await RegisterUserAsync("voter@test.com", "voter", "Test123!");

            SetAuthorizationHeader(poster.Token);
            var post = await CreatePostAsync("Please upvote this post!");

            SetAuthorizationHeader(voter.Token);
            var voteRequest = new CreateVoteClientModel
            {
                PostId = post.Id,
                VoteType = (int)VoteType.Upvote
            };

            var response = await Client.PostAsJsonAsync("/api/vote", voteRequest);
            
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Voting_UserCanDownvotePost()
        {
            var poster = await RegisterUserAsync("poster2@test.com", "poster2", "Test123!");
            var voter = await RegisterUserAsync("voter2@test.com", "voter2", "Test123!");

            SetAuthorizationHeader(poster.Token);
            var post = await CreatePostAsync("This might get downvoted");

            SetAuthorizationHeader(voter.Token);
            var voteRequest = new CreateVoteClientModel
            {
                PostId = post.Id,
                VoteType = (int)VoteType.Downvote
            };

            var response = await Client.PostAsJsonAsync("/api/vote", voteRequest);
            
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Voting_DisplaysScoreAsUpvotesMinusDownvotes()
        {
            var poster = await RegisterUserAsync("poster3@test.com", "poster3", "Test123!");
            var upvoter1 = await RegisterUserAsync("upvoter1@test.com", "upvoter1", "Test123!");
            var upvoter2 = await RegisterUserAsync("upvoter2@test.com", "upvoter2", "Test123!");
            var downvoter = await RegisterUserAsync("downvoter@test.com", "downvoter", "Test123!");

            SetAuthorizationHeader(poster.Token);
            var post = await CreatePostAsync("Check the score calculation");

            SetAuthorizationHeader(upvoter1.Token);
            await VoteOnPostAsync(post.Id, VoteType.Upvote);

            SetAuthorizationHeader(poster.Token);
            await VoteOnPostAsync(post.Id, VoteType.Upvote);

            SetAuthorizationHeader(upvoter2.Token);
            await VoteOnPostAsync(post.Id, VoteType.Upvote);

            SetAuthorizationHeader(downvoter.Token);
            await VoteOnPostAsync(post.Id, VoteType.Downvote);

            SetAuthorizationHeader(poster.Token);
            var feed = await GetAsync<FeedResponseClientModel>("/api/feed");
            var votedPost = feed.Posts.First(p => p.Id == post.Id);
            
            votedPost.Score.Should().Be(2); // 3 upvotes - 1 downvote = 1
        }
    }
}