
#region Using Directives

using AngleSharp.Dom;
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
    public class Post
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="Post"/> instance,
        /// </summary>
        /// <param name="httpClient">The HTTP client, which is used to call the 9GAG website.</param>
        internal Post(HttpClient httpClient)
        {
            this.HttpClient = httpClient;
        }

        #endregion

        #region Private Static Fields

        /// <summary>
        /// Contains the 9GAG base URI for posts.
        /// </summary>
        private static readonly Uri postBaseUri = new Uri("http://9gag.com/gag/", UriKind.Absolute);

        #endregion

        #region Internal Properties

        /// <summary>
        /// Gets an HTTP client, which is used to call the 9GAG website.
        /// </summary>
        internal HttpClient HttpClient { get; private set; }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the ID of the post.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets or sets the title of the post.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Gets the URI of the MP4 video content of the post.
        /// </summary>
        public IEnumerable<Content> Content { get; protected set; }

        /// <summary>
        /// Gets a value that determines whether this post is not safe for work, i.e. it contains explicit content and may only be viewed by signed in users.
        /// </summary>
        public bool IsNotSafeForWork { get; private set; }

        /// <summary>
        /// Gets the number of up-votes that the post received.
        /// </summary>
        public int NumberOfUpVotes { get; private set; }

        /// <summary>
        /// Gets or sets the number of comments of the post.
        /// </summary>
        public int NumberOfComments { get; private set; }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Parses the specified post element and parses the general information about the post.
        /// </summary>
        /// <param name="postElement">The post element, which is to be parsed.</param>
        internal virtual void ParseGeneralInformation(IElement postElement)
        {
            // Parses the ID and the title of the post
            this.Id = postElement.GetAttribute("data-entry-id");
            this.Title = postElement.QuerySelector("header").TextContent.Trim();

            // Checks if the post is a NSFW post
            this.IsNotSafeForWork = postElement.QuerySelector(".nsfw-post") != null;

            // Parses the number of comments and the number of upvotes of the post
            int numberOfComments, numberOfUpVotes;
            int.TryParse(postElement.GetAttribute("data-entry-comments").Trim(), out numberOfComments);
            int.TryParse(postElement.GetAttribute("data-entry-votes").Trim(), out numberOfUpVotes);
            this.NumberOfComments = numberOfComments;
            this.NumberOfUpVotes = numberOfUpVotes;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Parses the HTML document for detail information. This can be overridden by sub-classes to implement custom detail information.
        /// </summary>
        /// <param name="htmlDocument">The HTML document, which is to be parsed.</param>
        protected virtual void ParseDetailInformation(IHtmlDocument htmlDocument) { }

        #endregion

        #region Public Methods

        /// <summary>
        /// Fetches the detail information about the post.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token, which can be used to cancel the fetching of the details.</param>
        /// <exception cref="NineGagException">If anything goes wrong during the retrieval of the details, an <see cref="NineGagException"/> exception is thrown.</exception>
        public async Task FetchDetailsAsync(CancellationToken cancellationToken)
        {
            // Tries to get the details page of the 9GAG post, if it could not be retrieved, then an exception is thrown
            string nineGagPostDetailsPageContent;
            try
            {
                HttpResponseMessage responseMessage = await this.HttpClient.GetAsync(new Uri(Post.postBaseUri, this.Id), cancellationToken);
                responseMessage.EnsureSuccessStatusCode();
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

        /// <summary>
        /// Fetches the detail information about the post.
        /// </summary>
        /// <exception cref="NineGagException">If anything goes wrong during the retrieval of the details, an <see cref="NineGagException"/> exception is thrown.</exception>
        public Task FetchDetailsAsync() => this.FetchDetailsAsync(new CancellationTokenSource().Token);

        #endregion
    }
}