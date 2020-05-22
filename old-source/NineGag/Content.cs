
#region Using Directives

using System;

#endregion

namespace NineGag
{
    /// <summary>
    /// Represents a content of a post.
    /// </summary>
    public class Content
    {
        #region Public Properties

        /// <summary>
        /// Gets the URI of the content.
        /// </summary>
        public Uri Uri { get; internal set; }

        /// <summary>
        /// Gets or sets the kind of the content.
        /// </summary>
        public ContentKind Kind { get; internal set; }

        #endregion
    }
}
