
#region Using Directives

using System;
using System.Collections.Generic;

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
        public IEnumerable<VideoContent> Content { get; internal set; }

        /// <summary>
        /// Gets the URI of the thumbnail of the video.
        /// </summary>
        public Uri ThumbnailUri { get; internal set; }

        #endregion
    }
}