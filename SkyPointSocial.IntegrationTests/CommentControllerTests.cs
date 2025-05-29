using FluentAssertions;
using SkyPointSocial.Core.ClientModels.Comment;
using SkyPointSocial.Core.ClientModels.Feed;
using SkyPointSocial.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace SkyPointSocial.IntegrationTests.Tests
{
    public class CommentControllerTests : BaseControllerTest
    {
        public CommentControllerTests(IntegrationTestWebApplicationFactory factory) : base(factory) { }

        [Fact]
        public async Task Comments_UserCanCommentOnPost()
        {
            var poster = await RegisterUserAsync("poster@test.com", "poster", "Test123!");
            var commenter = await RegisterUserAsync("commenter@test.com", "commenter", "Test123!");

            SetAuthorizationHeader(poster.Token);
            var post = await CreatePostAsync("Please comment on this!");

            SetAuthorizationHeader(commenter.Token);
            var commentRequest = new CreateCommentClientModel
            {
                Content = "This is my comment on your post!"
            };

            var response = await Client.PostAsJsonAsync($"/api/comment/{post.Id}", commentRequest);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var comment = await response.Content.ReadFromJsonAsync<CommentClientModel>();
            comment!.Content.Should().Be(commentRequest.Content);
            comment.User.Username.Should().Be("commenter");
        }

        [Fact]
        public async Task Comments_UpdatesCommentCountInFeed()
        {
            var poster = await RegisterUserAsync("poster2@test.com", "poster2", "Test123!");
            var commenter1 = await RegisterUserAsync("commenter1@test.com", "commenter1", "Test123!");
            var commenter2 = await RegisterUserAsync("commenter2@test.com", "commenter2", "Test123!");

            SetAuthorizationHeader(poster.Token);
            var post = await CreatePostAsync("Comment count test");

            SetAuthorizationHeader(commenter1.Token);
            await CreateCommentAsync(post.Id, "First comment");

            SetAuthorizationHeader(commenter2.Token);
            await CreateCommentAsync(post.Id, "Second comment");

            SetAuthorizationHeader(poster.Token);
            var feed = await GetAsync<FeedResponseClientModel>("/api/feed");
            var commentedPost = feed.Posts.First(p => p.Id == post.Id);
            
            commentedPost.CommentCount.Should().Be(2);
        }

        [Fact]
        public async Task Comments_DeepNestedReplies_MaintainCorrectHierarchy()
        {
            // Arrange - Create users for a conversation thread
            var poster = await RegisterUserAsync("poster@test.com", "poster", "Test123!");
            var user1 = await RegisterUserAsync("user1@test.com", "user1", "Test123!");
            var user2 = await RegisterUserAsync("user2@test.com", "user2", "Test123!");
            var user3 = await RegisterUserAsync("user3@test.com", "user3", "Test123!");

            // Create post
            SetAuthorizationHeader(poster.Token);
            var post = await CreatePostAsync("Deep nested comment thread test");

            // Level 1: user1 comments on post
            SetAuthorizationHeader(user1.Token);
            var level1Comment = await CreateCommentAsync(post.Id, "Level 1 comment", null);
            level1Comment.ParentCommentId.Should().BeNull();

            // Level 2: user2 replies to user1
            SetAuthorizationHeader(user2.Token);
            var level2Comment = await CreateCommentAsync(post.Id, "Level 2 reply to user1", level1Comment.Id);
            level2Comment.ParentCommentId.Should().Be(level1Comment.Id);

            // Level 3: user3 replies to user2
            SetAuthorizationHeader(user3.Token);
            var level3Comment = await CreateCommentAsync(post.Id, "Level 3 reply to user2", level2Comment.Id);
            level3Comment.ParentCommentId.Should().Be(level2Comment.Id);

            // Level 4: poster replies to the deepest comment
            SetAuthorizationHeader(poster.Token);
            var level4Comment = await CreateCommentAsync(post.Id, "Level 4 reply from post creator", level3Comment.Id);
            level4Comment.ParentCommentId.Should().Be(level3Comment.Id);
            level4Comment.User.Username.Should().Be("poster");

            // Verify all comments belong to the same post
            level1Comment.PostId.Should().Be(post.Id);
            level2Comment.PostId.Should().Be(post.Id);
            level3Comment.PostId.Should().Be(post.Id);
            level4Comment.PostId.Should().Be(post.Id);

            // Verify total count
            var feed = await GetAsync<FeedResponseClientModel>("/api/feed");
            var threadPost = feed.Posts.First(p => p.Id == post.Id);
            threadPost.CommentCount.Should().Be(4);
        }

        [Fact]
        public async Task Comments_NestedCommentsAndReplies_WorkCorrectly()
        {
            // Arrange - Create users
            var postCreator = await RegisterUserAsync("postcreator@test.com", "postcreator", "Test123!");
            var commenter1 = await RegisterUserAsync("commenter1@test.com", "commenter1", "Test123!");
            var commenter2 = await RegisterUserAsync("commenter2@test.com", "commenter2", "Test123!");

            // Create post
            SetAuthorizationHeader(postCreator.Token);
            var post = await CreatePostAsync("Let's have a discussion with nested comments!");

            // First level comment by commenter1
            SetAuthorizationHeader(commenter1.Token);
            var firstCommentRequest = new CreateCommentClientModel
            {
                Content = "This is the first comment on the post",
                ParentCommentId = null // Direct comment on post
            };
            var firstCommentResponse = await Client.PostAsJsonAsync($"/api/comment/{post.Id}", firstCommentRequest);
            firstCommentResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var firstComment = await firstCommentResponse.Content.ReadFromJsonAsync<CommentClientModel>();

            // Verify first comment
            firstComment!.Content.Should().Be(firstCommentRequest.Content);
            firstComment.ParentCommentId.Should().BeNull();
            firstComment.PostId.Should().Be(post.Id);

            // Reply to first comment by commenter2
            SetAuthorizationHeader(commenter2.Token);
            var replyToFirstRequest = new CreateCommentClientModel
            {
                Content = "This is a reply to the first comment",
                ParentCommentId = firstComment.Id
            };
            var replyToFirstResponse = await Client.PostAsJsonAsync($"/api/comment/{post.Id}", replyToFirstRequest);
            replyToFirstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var replyToFirst = await replyToFirstResponse.Content.ReadFromJsonAsync<CommentClientModel>();

            // Verify reply
            replyToFirst!.Content.Should().Be(replyToFirstRequest.Content);
            replyToFirst.ParentCommentId.Should().Be(firstComment.Id);
            replyToFirst.PostId.Should().Be(post.Id);

            // Post creator replies to the nested comment
            SetAuthorizationHeader(postCreator.Token);
            var creatorReplyRequest = new CreateCommentClientModel
            {
                Content = "Thanks for the discussion! This is the post creator replying.",
                ParentCommentId = replyToFirst.Id
            };
            var creatorReplyResponse = await Client.PostAsJsonAsync($"/api/comment/{post.Id}", creatorReplyRequest);
            creatorReplyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var creatorReply = await creatorReplyResponse.Content.ReadFromJsonAsync<CommentClientModel>();

            // Verify creator's reply
            creatorReply!.Content.Should().Be(creatorReplyRequest.Content);
            creatorReply.ParentCommentId.Should().Be(replyToFirst.Id);
            creatorReply.PostId.Should().Be(post.Id);
            creatorReply.User.Username.Should().Be("postcreator");

            // Verify total comment count
            var feed = await GetAsync<FeedResponseClientModel>("/api/feed");
            var discussionPost = feed.Posts.First(p => p.Id == post.Id);
            discussionPost.CommentCount.Should().Be(3); // All comments count towards the total
        }
    }
}