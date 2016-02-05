
#region Using Directives

using System.Collections.Generic;

#endregion

namespace NineGag
{
    /// <summary>
    /// Represents a page of 9GAG posts.
    /// </summary>
    public class Page
    {
        #region Public Properties

        /// <summary>
        /// Gets the ID of the current page. If the ID of the current page is <c>null</c>, then this is the first page.
        /// </summary>
        public string CurrentPageId { get; internal set; }

        /// <summary>
        /// Gets the ID of the next page.
        /// </summary>
        public string NextPageId { get; internal set; }

        /// <summary>
        /// Gets the number of posts that are to be retrieved per page.
        /// </summary>
        public int NumberOfPostsToRetrieve { get; internal set; }

        /// <summary>
        /// Gets the post of the page.
        /// </summary>
        public IEnumerable<Post> Posts { get; internal set; }

        #endregion
    }
}