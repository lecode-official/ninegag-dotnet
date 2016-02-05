
#region Using Directives

using System;

#endregion

namespace NineGag
{
    /// <summary>
    /// Represents a post on 9GAG with an image content.
    /// </summary>
    public class ImagePost : Post
    {
        #region Public Properties

        /// <summary>
        /// Gets the URI of picture content of the post.
        /// </summary>
        public Uri PictureUri { get; internal set; }
        
        #endregion
    }
}