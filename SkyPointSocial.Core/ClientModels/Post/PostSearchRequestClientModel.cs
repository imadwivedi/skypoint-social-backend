using System;
using System.Collections.Generic;

namespace SkyPointSocial.Core.ClientModels.Post
{
    /// <summary>
    /// Parameters for searching posts (extensible for future filters)
    /// </summary>
    public class PostSearchRequestClientModel
    {
        /// <summary>
        /// Free-text search query
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Page number (default 1)
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Page size (default 20)
        /// </summary>
        public int PageSize { get; set; } = 20;
    }
}
