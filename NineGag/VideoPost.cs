
#region Using Directives

using System;

#endregion

namespace NineGag
{
    /// <summary>
    /// Represents a post on 9GAG with a video content.
    /// </summary>
    public class VideoPost : Post
    {
        #region Public Properties
        
        /// <summary>
        /// Gets the URI of the MP4 video content of the post.
        /// </summary>
        public Uri VideoUri { get; internal set; }

        /// <summary>
        /// Gets the URI of the thumbnail of the video.
        /// </summary>
        public Uri ThumbnailUri { get; internal set; }

        #endregion
    }
}