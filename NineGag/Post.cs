
#region Using Directives

using System;

#endregion

namespace NineGag
{
    /// <summary>
    /// Represents a post on 9GAG.
    /// </summary>
    public class Post
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the title of the post.
        /// </summary>
        public string Title { get; internal set; }

        /// <summary>
        /// Gets the kind of the post.
        /// </summary>
        public PostKind Kind { get; internal set; }

        /// <summary>
        /// Gets the URI of the content of the post. When the post kind is image, then this is the URI to the image, if the post kind is video then this is the
        /// URI to the MP4 version of the video.
        /// </summary>
        public Uri ContentUri { get; internal set; }

        /// <summary>
        /// Gets the URI of the thumbnail of the post. When the post kind is video then this is the video thumbnail, otherwise it is <c>null</c>.
        /// </summary>
        public Uri ThumbnailUri { get; internal set; }

        /// <summary>
        /// Gets the number of up-votes that the post received.
        /// </summary>
        public int NumberOfUpVotes { get; internal set; }

        /// <summary>
        /// Gets or sets the number of comments of the post.
        /// </summary>
        public int NumberOfComments { get; internal set; }

        #endregion
    }
}