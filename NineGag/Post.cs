
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
        /// <exception cref="NineGagException">If anything goes wrong during the retrieval of the details, an <see cref="NineGagException"/> exception is thrown.</exception>
        internal async Task FetchDetailsAsync(HttpClient httpClient, CancellationToken cancellationToken)
        {
            // Tries to get the details page of the 9GAG post, if it could not be retrieved, then an exception is thrown
            string nineGagPostDetailsPageContent;
            try
            {
                HttpResponseMessage responseMessage = await httpClient.GetAsync(new Uri(Post.postBaseUri, this.Id), cancellationToken);
                nineGagPostDetailsPageContent = await responseMessage.Content.ReadAsStringAsync();

            }
            catch (Exception exception)
            {
                throw new NineGagException("The 9GAG post details page could not be retrieved. Maybe there is no internet connection available.", exception);
            }

            // Tries to parse the HTML of the 9GAG post details page, if the HTML could not be parsed, then an exception is thrown
            IHtmlDocument htmlDocument;
            try
            {
                HtmlParser htmlParser = new HtmlParser();
                htmlDocument = await htmlParser.ParseAsync(nineGagPostDetailsPageContent);
            }
            catch (Exception exception)
            {
                throw new NineGagException("The HTML of the 9GAG post details page could not be parsed. This could be an indicator, that the 9GAG website is down or its content has changed. If this problem keeps coming, then please report this problem to 9GAG or the maintainer of the library.", exception);
            }
            
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