using System.Collections.Generic;

namespace SkyPointSocial.Core.ClientModels.Post
{
    /// <summary>
    /// Result of a post search with vector search capabilities (paginated)
    /// </summary>
    public class PostSearchResultClientModel
    {
        /// <summary>
        /// List of posts matching the search (optimized for search results)
        /// </summary>
        public List<PostSearchItemClientModel> Posts { get; set; } = new();

        /// <summary>
        /// Total number of posts matching the search (for pagination)
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Indicates if there are more posts available for pagination
        /// </summary>
        public bool HasMore { get; set; }
    }
}
