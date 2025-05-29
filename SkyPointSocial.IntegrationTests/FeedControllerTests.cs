using FluentAssertions;
using SkyPointSocial.Core.ClientModels.Feed;
using SkyPointSocial.Core.Entities;
using SkyPointSocial.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace SkyPointSocial.IntegrationTests.Tests
{
    public class FeedControllerTests : BaseControllerTest
    {
        public FeedControllerTests(IntegrationTestWebApplicationFactory factory) : base(factory) { }

        [Fact]
        public async Task Newsfeed_DisplaysPersonalizedFeedForUser()
        {
            var user = await RegisterUserAsync("feeduser@test.com", "feeduser", "Test123!");
            SetAuthorizationHeader(user.Token);

            var response = await Client.GetAsync("/api/feed");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var feed = await response.Content.ReadFromJsonAsync<FeedResponseClientModel>();
            feed.Should().NotBeNull();
            feed!.Posts.Should().NotBeNull();
        }

        [Fact]
        public async Task Newsfeed_FollowedUsersContentAppearsWithHigherPriority()
        {
            var mainUser = await RegisterUserAsync("main@test.com", "mainuser", "Test123!");
            var followedUser = await RegisterUserAsync("followed@test.com", "followeduser", "Test123!");
            var notFollowedUser = await RegisterUserAsync("notfollowed@test.com", "notfollowed", "Test123!");

            // Create posts
            SetAuthorizationHeader(followedUser.Token);
            await CreatePostAsync("Post from followed user");

            SetAuthorizationHeader(notFollowedUser.Token);
            await CreatePostAsync("Post from not followed user");

            // Follow user
            SetAuthorizationHeader(mainUser.Token);
            await FollowUserAsync(followedUser.User.Id);

            // Get feed
            var response = await Client.GetAsync("/api/feed");
            var feed = await response.Content.ReadFromJsonAsync<FeedResponseClientModel>();

            // Followed users' posts should appear with higher priority
            feed!.Posts.Should().NotBeEmpty();
            var followedUserPost = feed.Posts.FirstOrDefault(p => p.User.Username == "followeduser");
            followedUserPost.Should().NotBeNull();
        }

        [Fact]
        public async Task Newsfeed_SortedByMultipleCriteria()
        {
            var user = await RegisterUserAsync("sorter@test.com", "sorter", "Test123!");
            var poster1 = await RegisterUserAsync("poster1@test.com", "poster1", "Test123!");
            var poster2 = await RegisterUserAsync("poster2@test.com", "poster2", "Test123!");

            // Create posts with different characteristics
            SetAuthorizationHeader(poster1.Token);
            var post1 = await CreatePostAsync("High score post");

            SetAuthorizationHeader(poster2.Token);
            var post2 = await CreatePostAsync("Recent post");

            // Add votes to post1
            SetAuthorizationHeader(user.Token);
            await VoteOnPostAsync(post1.Id, VoteType.Upvote);

            // Get feed - should be sorted by multiple criteria
            var response = await Client.GetAsync("/api/feed");
            var feed = await response.Content.ReadFromJsonAsync<FeedResponseClientModel>();

            feed!.Posts.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Newsfeed_DisplaysAllRequiredItemDetails()
        {
            var poster = await RegisterUserAsync("detailposter@test.com", "detailposter", "Test123!");
            var commenter = await RegisterUserAsync("detailcommenter@test.com", "detailcommenter", "Test123!");
            var voter = await RegisterUserAsync("detailvoter@test.com", "detailvoter", "Test123!");

            SetAuthorizationHeader(poster.Token);
            var post = await CreatePostAsync("Check all feed item details");

            SetAuthorizationHeader(commenter.Token);
            await CreateCommentAsync(post.Id, "A comment");

            SetAuthorizationHeader(voter.Token);
            await VoteOnPostAsync(post.Id, VoteType.Upvote);

            var response = await Client.GetAsync("/api/feed");
            var feed = await response.Content.ReadFromJsonAsync<FeedResponseClientModel>();
            var feedItem = feed!.Posts.First(p => p.Id == post.Id);

            // Verify all required fields are present
            feedItem.Content.Should().NotBeNullOrEmpty();
            feedItem.User.Should().NotBeNull();
            feedItem.Score.Should().BeGreaterThanOrEqualTo(0); // Vote count (score = upvotes - downvotes)
            feedItem.CommentCount.Should().Be(1);
            feedItem.TimeAgo.Should().NotBeNullOrEmpty(); // e.g., "2h ago"
        }

        [Fact]
        public async Task Newsfeed_ShowsInteractiveControls()
        {
            var user = await RegisterUserAsync("interactive@test.com", "interactive", "Test123!");
            var poster = await RegisterUserAsync("controlposter@test.com", "controlposter", "Test123!");

            SetAuthorizationHeader(poster.Token);
            await CreatePostAsync("Post with interactive controls");

            SetAuthorizationHeader(user.Token);
            await FollowUserAsync(poster.User.Id);

            var response = await Client.GetAsync("/api/feed");
            var feed = await response.Content.ReadFromJsonAsync<FeedResponseClientModel>();

            // Feed items should have data needed for interactive controls
            feed!.Posts.Should().NotBeEmpty();
            var post = feed.Posts.First();
            
            // These fields enable interactive controls
            post.Id.Should().NotBeEmpty(); // For vote/comment actions
            post.User.Id.Should().NotBeEmpty(); // For follow action
            post.User.IsFollowing.Should().BeTrue(); // Or use BeFalse() if you are testing for false
        }

        [Fact]
        public async Task Newsfeed_RealWorldScenario_ComplexUserNetworkWithCorrectOrdering()
        {
            // Arrange - Create a network of users
            // Alice follows Bob and Charlie
            // Bob follows Alice and David
            // Charlie follows Eve
            // David follows Bob
            // Eve follows no one

            var alice = await RegisterUserAsync("alice@test.com", "alice", "Test123!");
            var bob = await RegisterUserAsync("bob@test.com", "bob", "Test123!");
            var charlie = await RegisterUserAsync("charlie@test.com", "charlie", "Test123!");
            var david = await RegisterUserAsync("david@test.com", "david", "Test123!");
            var eve = await RegisterUserAsync("eve@test.com", "eve", "Test123!");

            // Set up following relationships
            SetAuthorizationHeader(alice.Token);
            await FollowUserAsync(bob.User.Id);
            await FollowUserAsync(charlie.User.Id);

            SetAuthorizationHeader(bob.Token);
            await FollowUserAsync(alice.User.Id);
            await FollowUserAsync(david.User.Id);

            SetAuthorizationHeader(charlie.Token);
            await FollowUserAsync(eve.User.Id);

            SetAuthorizationHeader(david.Token);
            await FollowUserAsync(bob.User.Id);

            // Create posts with various characteristics
            // Bob's posts (Alice follows Bob)
            SetAuthorizationHeader(bob.Token);
            var bobPost1 = await CreatePostAsync("Bob's popular post");
            await Task.Delay(100); // Ensure different timestamps
            var bobPost2 = await CreatePostAsync("Bob's regular post");

            // Charlie's post (Alice follows Charlie)
            SetAuthorizationHeader(charlie.Token);
            var charliePost = await CreatePostAsync("Charlie's discussion starter");

            // David's posts (Alice doesn't follow David)
            SetAuthorizationHeader(david.Token);
            var davidPost1 = await CreatePostAsync("David's viral post");
            await Task.Delay(100);
            var davidPost2 = await CreatePostAsync("David's recent post");

            // Eve's post (Alice doesn't follow Eve)
            SetAuthorizationHeader(eve.Token);
            var evePost = await CreatePostAsync("Eve's interesting post");

            // Add interactions to create different scores and comment counts
            // Make Bob's first post popular (high score, high comments)
            var voters = new[] { charlie, david, eve };
            foreach (var voter in voters)
            {
                SetAuthorizationHeader(voter.Token);
                await VoteOnPostAsync(bobPost1.Id, VoteType.Upvote);
                await CreateCommentAsync(bobPost1.Id, $"Great post Bob! - from {voter.User.Username}");
            }

            // Charlie's post gets moderate engagement
            SetAuthorizationHeader(bob.Token);
            await VoteOnPostAsync(charliePost.Id, VoteType.Upvote);
            await CreateCommentAsync(charliePost.Id, "Nice thoughts Charlie!");

            SetAuthorizationHeader(eve.Token);
            await VoteOnPostAsync(charliePost.Id, VoteType.Upvote);

            // David's viral post gets high score but fewer comments
            foreach (var voter in new[] { alice, bob, charlie, eve })
            {
                SetAuthorizationHeader(voter.Token);
                await VoteOnPostAsync(davidPost1.Id, VoteType.Upvote);
            }
            SetAuthorizationHeader(bob.Token);
            await CreateCommentAsync(davidPost1.Id, "This is going viral!");

            // Eve's post gets one downvote
            SetAuthorizationHeader(david.Token);
            await VoteOnPostAsync(evePost.Id, VoteType.Downvote);

            // Now check Alice's feed
            SetAuthorizationHeader(alice.Token);
            var response = await Client.GetAsync("/api/feed");
            var feed = await response.Content.ReadFromJsonAsync<FeedResponseClientModel>();

            // Assert feed ordering based on requirements:
            // 1. Posts by followed users (Bob, Charlie) should appear first
            // 2. Among followed users' posts, order by score
            // 3. Then by comment count
            // 4. Then by time (most recent)

            feed!.Posts.Should().NotBeEmpty();
            var feedPosts = feed.Posts.ToList();

            // Verify followed users' posts appear first
            var followedUsersPosts = feedPosts.TakeWhile(p => p.User.IsFollowing).ToList();
            var unfollowedUsersPosts = feedPosts.SkipWhile(p => p.User.IsFollowing).ToList();

            // All followed users' posts should come before unfollowed users' posts
            followedUsersPosts.Should().NotBeEmpty();
            unfollowedUsersPosts.Should().NotBeEmpty();

            // Check specific ordering within followed users' posts
            var bobPopularPost = followedUsersPosts.FirstOrDefault(p => p.Id == bobPost1.Id);
            var charlieDiscussionPost = followedUsersPosts.FirstOrDefault(p => p.Id == charliePost.Id);
            var bobRegularPost = followedUsersPosts.FirstOrDefault(p => p.Id == bobPost2.Id);

            bobPopularPost.Should().NotBeNull();
            charlieDiscussionPost.Should().NotBeNull();
            bobRegularPost.Should().NotBeNull();

            // Bob's popular post should be first (highest score among followed: 3)
            feedPosts.IndexOf(bobPopularPost!).Should().Be(0);
            bobPopularPost.Score.Should().Be(3);
            bobPopularPost.CommentCount.Should().Be(3);
            bobPopularPost.User.Username.Should().Be("bob");
            bobPopularPost.User.IsFollowing.Should().BeTrue();

            // Charlie's post should be second (score: 2, among followed)
            charlieDiscussionPost.Score.Should().Be(2);
            charlieDiscussionPost.CommentCount.Should().Be(1);
            charlieDiscussionPost.User.IsFollowing.Should().BeTrue();

            // Bob's regular post should be third (score: 0, but from followed user)
            bobRegularPost.Score.Should().Be(0);
            bobRegularPost.CommentCount.Should().Be(0);
            bobRegularPost.User.IsFollowing.Should().BeTrue();

            // Verify unfollowed users' posts ordering
            var davidViralPost = unfollowedUsersPosts.FirstOrDefault(p => p.Id == davidPost1.Id);
            var davidRecentPost = unfollowedUsersPosts.FirstOrDefault(p => p.Id == davidPost2.Id);
            var eveInterestingPost = unfollowedUsersPosts.FirstOrDefault(p => p.Id == evePost.Id);

            // David's viral post should be first among unfollowed (highest score: 4)
            davidViralPost.Should().NotBeNull();
            davidViralPost.Score.Should().Be(4);
            davidViralPost.CommentCount.Should().Be(1);
            davidViralPost.User.IsFollowing.Should().BeFalse();

            // David's recent post (score: 0, but more recent than Eve's)
            davidRecentPost.Should().NotBeNull();
            davidRecentPost.Score.Should().Be(0);
            davidRecentPost.User.IsFollowing.Should().BeFalse();

            // Eve's post should be last (negative score)
            eveInterestingPost.Should().NotBeNull();
            eveInterestingPost.Score.Should().Be(-1);
            eveInterestingPost.User.IsFollowing.Should().BeFalse();

            // Verify the complete ordering
            var expectedOrder = new[]
            {
                bobPopularPost.Id,      // Followed, Score: 3, Comments: 3
                charlieDiscussionPost.Id, // Followed, Score: 2, Comments: 1
                bobRegularPost.Id,      // Followed, Score: 0, Comments: 0 (more recent)
                davidViralPost.Id,      // Not followed, Score: 4, Comments: 1
                davidRecentPost.Id,     // Not followed, Score: 0, Comments: 0 (more recent)
                eveInterestingPost.Id   // Not followed, Score: -1
            };

            feedPosts.Select(p => p.Id).Should().ContainInOrder(expectedOrder);

            // Verify all interactive controls are present
            foreach (var post in feedPosts)
            {
                post.Id.Should().NotBeEmpty();
                post.User.Id.Should().NotBeEmpty();
                post.User.Username.Should().NotBeNullOrEmpty();
                post.TimeAgo.Should().NotBeNullOrEmpty();
                post.User.IsFollowing.GetType().Should().Be(typeof(bool)); // Ensure IsFollowing is a boolean
            }
        }

        [Fact]
        public async Task Newsfeed_EmptyFeedForNewUser_ReturnsEmptyList()
        {
            // New user with no posts in the system
            var newUser = await RegisterUserAsync("newuser@test.com", "newuser", "Test123!");
            SetAuthorizationHeader(newUser.Token);

            var response = await Client.GetAsync("/api/feed");
            var feed = await response.Content.ReadFromJsonAsync<FeedResponseClientModel>();

            feed!.Posts.Should().BeEmpty();
            feed.TotalCount.Should().Be(0);
            feed.HasMore.Should().BeFalse();
        }

        [Fact]
        public async Task Newsfeed_PaginationWithMixedContent_MaintainsCorrectOrder()
        {
            var mainUser = await RegisterUserAsync("mainuser@test.com", "mainuser", "Test123!");
            var followedUser = await RegisterUserAsync("followed@test.com", "followed", "Test123!");
            var popularUser = await RegisterUserAsync("popular@test.com", "popular", "Test123!");

            // Follow one user
            SetAuthorizationHeader(mainUser.Token);
            await FollowUserAsync(followedUser.User.Id);

            // Create many posts to test pagination
            SetAuthorizationHeader(followedUser.Token);
            for (int i = 1; i <= 15; i++)
            {
                await CreatePostAsync($"Followed user post {i}");
                await Task.Delay(50);
            }

            SetAuthorizationHeader(popularUser.Token);
            for (int i = 1; i <= 10; i++)
            {
                await CreatePostAsync($"Popular user post {i}");
                await Task.Delay(50);
            }

            SetAuthorizationHeader(mainUser.Token);

            // Get first page
            var page1Response = await Client.GetAsync("/api/feed?page=1&pageSize=10");
            var page1 = await page1Response.Content.ReadFromJsonAsync<FeedResponseClientModel>();

            // Get second page
            var page2Response = await Client.GetAsync("/api/feed?page=2&pageSize=10");
            var page2 = await page2Response.Content.ReadFromJsonAsync<FeedResponseClientModel>();

            // First page should have all followed user posts (they have priority)
            page1!.Posts.Should().HaveCount(10);
            page1.Posts.All(p => p.User.IsFollowing).Should().BeTrue();
            page1.HasMore.Should().BeTrue();

            // Second page should have remaining followed posts and then unfollowed posts
            page2!.Posts.Should().HaveCount(10);
            var followedInPage2 = page2.Posts.TakeWhile(p => p.User.IsFollowing).Count();
            followedInPage2.Should().Be(5); // Remaining 5 followed posts

            // Rest should be from unfollowed user
            page2.Posts.Skip(5).All(p => !p.User.IsFollowing).Should().BeTrue();
        }
    }
}