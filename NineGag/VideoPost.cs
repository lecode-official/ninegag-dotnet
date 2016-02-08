
#region Using Directives

using AngleSharp.Dom.Html;
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
        /// Gets the URI of the thumbnail of the video.
        /// </summary>
        public Uri ThumbnailUri { get; internal set; }

        #endregion

        #region Post Implementation

        /// <summary>
        /// Parses the detail information of the post.
        /// </summary>
        /// <param name="htmlDocument">The HTML document, which contains the details page of the post.</param>
        protected override void ParseDetailInformation(IHtmlDocument htmlDocument) { }

        #endregion
    }
}