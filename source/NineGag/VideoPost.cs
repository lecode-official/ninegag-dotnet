
#region Using Directives

using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using System;
using System.Linq;
using System.Net.Http;

#endregion

namespace NineGag
{
    /// <summary>
    /// Represents a post on 9GAG with a video content.
    /// </summary>
    public class VideoPost : Post
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="VideoPost"/> instance.
        /// </summary>
        /// <param name="httpClient">The HTTP client, which is used to call the 9GAG website.</param>
        internal VideoPost(HttpClient httpClient)
            : base(httpClient)
        { }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the URI of the thumbnail of the video.
        /// </summary>
        public Uri ThumbnailUri { get; private set; }

        #endregion

        #region Post Implementation

        /// <summary>
        /// Parses the specified post element and parses the general information about the post.
        /// </summary>
        /// <param name="postElement">The post element, which is to be parsed.</param>
        internal override void ParseGeneralInformation(IElement postElement)
        {
            // Calls the base implementation
            base.ParseGeneralInformation(postElement);

            // Parses the content of the video post
            IElement contentElement = postElement.QuerySelector("video");
            this.Content = contentElement.GetElementsByTagName("source").Select(child => new Content
            {
                Uri = new Uri(child.GetAttribute("src"), UriKind.Absolute),
                Kind = child.GetAttribute("type").ToUpperInvariant() == "VIDEO/MP4" ? ContentKind.Mp4 : ContentKind.WebM
            }).ToList();

            // Parses and retrieves the thumbnail of the video post
            this.ThumbnailUri = new Uri(contentElement.GetAttribute("poster"), UriKind.Absolute);
        }

        /// <summary>
        /// Parses the detail information of the post.
        /// </summary>
        /// <param name="htmlDocument">The HTML document, which contains the details page of the post.</param>
        protected override void ParseDetailInformation(IHtmlDocument htmlDocument) => base.ParseDetailInformation(htmlDocument);

        #endregion
    }
}
