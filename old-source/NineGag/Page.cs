
#region Using Directives

using System;
using System.Collections.Generic;

#endregion

namespace NineGag
{
    /// <summary>
///     /// Represents a page of 9GAG posts.
    /// </summary>
    public class Page
    {
        #region Internal Properties

        /// <summary>
        /// Gets or sets the URI, which was used to retrieve the current page. If the URI for the current page is <c>null</c>, then this is the first page.
        /// </summary>
        internal Uri CurrentPageUri { get; set; }

        /// <summary>
        /// Gets or sets the URI, which is needed to retrieve the next page.
        /// </summary>
        internal Uri NextPageUri { get; set; }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the post of the page.
        /// </summary>
        public IEnumerable<Post> Posts { get; internal set; }

        #endregion
    }
}
