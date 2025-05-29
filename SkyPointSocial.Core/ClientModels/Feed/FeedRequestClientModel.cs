namespace SkyPointSocial.Core.ClientModels.Feed
{
    /// <summary>
    /// Request model for fetching personalized feed
    /// </summary>
    public class FeedRequestClientModel
    {
        /// <summary>
        /// Page number for pagination (default: 1)
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Number of posts per page (default: 20)
        /// </summary>
        public int PageSize { get; set; } = 20;
    }
}