
#region Using Directives

using AngleSharp.Dom.Html;
using System;
using System.Collections.Generic;
using System.Linq;

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
        /// Gets a value that determines whether this image post is a long post (this means, that the content of the image post is only the long post cover and not the full image).
        /// </summary>
        public bool IsLongPost { get; internal set; }

        #endregion

        #region Post Implementation

        /// <summary>
        /// Parses the detail information of the post.
        /// </summary>
        /// <param name="htmlDocument">The HTML document, which contains the details page of the post.</param>
        /// <exception cref="NineGagException">If anything goes wrong during the retrieval of the details, an <see cref="NineGagException"/> exception is thrown.</exception>
        protected override void ParseDetailInformation(IHtmlDocument htmlDocument)
        {
            // Tries to parse the the larger version of the image, if could not be parsed, then an exception is thrown
            try
            {
                this.Content = this.Content.Union(new List<Content>
                {
                    new Content
                    {
                        Uri = new Uri(htmlDocument.QuerySelector("article img").GetAttribute("src"), UriKind.Absolute),
                        Kind = ContentKind.Jpeg
                    }
                }).ToList();
            }
            catch (Exception exception)
            {
                throw new NineGagException("The larger version of the content of the image post could not be retrieved. Maybe there is no internet connection available.", exception);
            }
        }

        #endregion
    }
}