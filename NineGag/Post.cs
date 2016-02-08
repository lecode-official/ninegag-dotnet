
#region Using Directives

using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace NineGag
{
    /// <summary>
    /// Represents the abstract base class for all kinds of posts offered by 9GAG.
    /// </summary>
    public abstract class Post
    {
        #region Private Static Fields

        /// <summary>
        /// Contains the 9GAG base URI for posts.
        /// </summary>
        private static readonly Uri postBaseUri = new Uri("http://9gag.com/gag/", UriKind.Absolute);

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the ID of the post.
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// Gets or sets the title of the post.
        /// </summary>
        public string Title { get; internal set; }

        /// <summary>
        /// Gets the URI of the MP4 video content of the post.
        /// </summary>
        public IEnumerable<Content> Content { get; internal set; }

        /// <summary>
        /// Gets the number of up-votes that the post received.
        /// </summary>
        public int NumberOfUpVotes { get; internal set; }

        /// <summary>
        /// Gets or sets the number of comments of the post.
        /// </summary>
        public int NumberOfComments { get; internal set; }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Fetches the detail information about the post.
        /// </summary>
        /// <param name="httpClient">The HTTP client, which is used to fetch the detail information.</param>
        internal async Task FetchDetailsAsync(HttpClient httpClient, CancellationToken cancellationToken)
        {
            // Gets the details page of the post from the 9GAG website
            HttpResponseMessage responseMessage = await httpClient.GetAsync(new Uri(Post.postBaseUri, this.Id), cancellationToken);
            string responseMessageContent = await responseMessage.Content.ReadAsStringAsync();

            // Parses the HTML of the 9GAG post detail page
            HtmlParser htmlParser = new HtmlParser();
            IHtmlDocument htmlDocument = await htmlParser.ParseAsync(responseMessageContent);

            // Calls the parsing methods of the sub-classes to retrieve the detail information
            this.ParseDetailInformation(htmlDocument);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Parses the HTML document for detail information. This can be overridden by sub-classes to implement custom detail information.
        /// </summary>
        /// <param name="htmlDocument">The HTML document, which is to be parsed.</param>
        protected abstract void ParseDetailInformation(IHtmlDocument htmlDocument);

        #endregion
    }
}