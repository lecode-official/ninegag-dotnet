
#region Using Directives

using System;

#endregion


namespace NineGag
{
    /// <summary>
    /// Represents a content of a video post.
    /// </summary>
    public class VideoContent
    {
        #region Public Properties

        /// <summary>
        /// Gets the URI of the video.
        /// </summary>
        public Uri Uri { get; internal set; }

        /// <summary>
        /// Gets or sets the MIME type of the video.
        /// </summary>
        public string MimeType { get; internal set; }

        #endregion
    }
}