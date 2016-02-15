
#region Using Directives

using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace NineGag
{
    /// <summary>
    /// Represents the 9GAG client, which can be used to access 9GAG and get raw data from it.
    /// </summary>
    public class NineGagClient : IDisposable
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="NineGagClient"/> instance.
        /// </summary>
        public NineGagClient()
        {
            // Creates a new cookie container as well as the HTTP client, which are used to communicate with the 9GAG server
            this.cookieContainer = new CookieContainer();
            this.httpClient = new HttpClient(new HttpClientHandler
            {
                CookieContainer = cookieContainer,
                AllowAutoRedirect = false
            });
            this.httpClient.BaseAddress = NineGagClient.baseUri;
        }

        #endregion

        #region Private Static Fields

        /// <summary>
        /// Contains the 9GAG base URI.
        /// </summary>
        private static readonly Uri baseUri = new Uri("http://9gag.com", UriKind.Absolute);

        /// <summary>
        /// Contains the root path of the website.
        /// </summary>
        private static readonly string rootPath = "/";

        /// <summary>
        /// Contains the path where users are able to sign in to 9GAG.
        /// </summary>
        private static readonly string signInPath = "/login";

        /// <summary>
        /// Contains the path where users are able to sign out of 9GAG.
        /// </summary>
        private static readonly string signOutPath = "/logout";

        /// <summary>
        /// Contains the path for the hot page or sub-section.
        /// </summary>
        private static readonly string hotPath = "/hot";

        /// <summary>
        /// Contains the path for the trending page.
        /// </summary>
        private static readonly string trendingPath = "/trending";

        /// <summary>
        /// Contains the path for the fresh page or sub-section.
        /// </summary>
        private static readonly string freshPath = "/fresh";

        #endregion

        #region Private Fields

        /// <summary>
        /// Contains the cookie container for the HTTP client, which can be used to receive server cookies as well as send cookies to the server.
        /// </summary>
        private readonly CookieContainer cookieContainer;

        /// <summary>
        /// Contains an HTTP client, which is used to call the 9GAG website.
        /// </summary>
        private readonly HttpClient httpClient;

        /// <summary>
        /// Contains an HTML parser, which is used to parse the HTML retrieved from the 9GAG website.
        /// </summary>
        private readonly HtmlParser htmlParser = new HtmlParser();

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a value that determines whether the <see cref="NineGagClient"/> has already been disposed of.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets a value, which determines whether the user is signed in or not.
        /// </summary>
        public bool IsUserSignedIn { get; private set; }

        #endregion

        #region Private Methods

        /// <summary>
        /// Ges the page of posts at the specified URI.
        /// </summary>
        /// <param name="pageUri">The URI of the page from which the posts are to be retrieved.</param>
        /// <param name="cancellationToken">The cancellation token, which can be used to cancel the retrieval of the page.</param>
        /// <returns>Returns the page from the specified URI.</returns>
        private async Task<Page> GetPostsAsync(Uri pageUri, CancellationToken cancellationToken)
        {
            // Tries to get the section page of the 9GAG website, if it could not be retrieved, then an exception is thrown
            string nineGagSectionPageContent;
            try
            {
                HttpResponseMessage responseMessage = await this.httpClient.GetAsync(pageUri, cancellationToken);
                responseMessage.EnsureSuccessStatusCode();
                nineGagSectionPageContent = await responseMessage.Content.ReadAsStringAsync();
            }
            catch (Exception exception)
            {
                throw new NineGagException("The 9GAG section page could not be retrieved. Maybe there is no internet connection available.", exception);
            }

            // Tries to parse the HTML of the 9GAG main page, if the HTML could not be parsed, then an exception is thrown
            IHtmlDocument htmlDocument;
            try
            {
                htmlDocument = await this.htmlParser.ParseAsync(nineGagSectionPageContent);
            }
            catch (Exception exception)
            {
                throw new NineGagException("The HTML of the 9GAG section page could not be parsed. This could be an indicator, that the 9GAG website is down or its content has changed. If this problem keeps coming, then please report this problem to 9GAG or the maintainer of the library.", exception);
            }

            // Tries to parse all the posts and adds them to the result set, if the posts could not be parsed, then an exception is thrown
            List<Post> posts = new List<Post>();
            try
            {
                // Gets the article elements from the HTML, which contain the posts, and cycles over them to parse the posts
                IHtmlCollection<IElement> postElements = htmlDocument.QuerySelectorAll("article");
                foreach (IElement postElement in postElements)
                {
                    // Checks the kind of the post and creates the corresponding post object
                    Post post;
                    if (postElement.QuerySelector("video") != null)
                        post = new VideoPost(this.httpClient);
                    else if (postElement.QuerySelector("img") != null)
                        post = new ImagePost(this.httpClient);
                    else
                        post = new Post(this.httpClient);

                    // Parses the general information about the post
                    post.ParseGeneralInformation(postElement);

                    // Adds the newly created post to the result set
                    posts.Add(post);
                }
            }
            catch (Exception exception)
            {
                throw new NineGagException("The posts of the 9GAG section page could not be parsed. Maybe the website structure of 9GAG has changed. If so, please report this error to the maintainer of the library.", exception);
            }

            // Tries to parse the URI of the next page to retrieve the ID of the next page as well as the number of posts to retrieve
            Uri nextPageQuery = null;
            try
            {
                nextPageQuery = new Uri(htmlDocument.QuerySelector("a.badge-load-more-post").GetAttribute("href").Trim(), UriKind.Relative);
            }
            catch (Exception) { }

            // Returns the result set, which contains all the posts
            return new Page
            {
                CurrentPageUri = pageUri,
                NextPageUri = nextPageQuery,
                Posts = posts
            };
        }

        #endregion

        #region Public Section Methods

        /// <summary>
        /// Gets all the sections of 9GAG. Sections are like categories and contain the actual content.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token, which can be used to cancel the retrieval of the sections.</param>
        /// <exception cref="NineGagException">If anything goes wrong during the retrieval of the sections, an <see cref="NineGagException"/> exception is thrown.</exception>
        /// <returns>Returns a list of all the sections that are currently available on 9GAG.</returns>
        public async Task<IEnumerable<Section>> GetSectionsAsync(CancellationToken cancellationToken)
        {
            // Creates a new list for the result set of sections
            List<Section> sections = new List<Section>();

            // Tries to get the main page of the 9GAG website, if it could not be retrieved, then an exception is thrown
            string nineGagMainPageContent;
            try
            {
                HttpResponseMessage responseMessage = await this.httpClient.GetAsync(NineGagClient.rootPath, cancellationToken);
                responseMessage.EnsureSuccessStatusCode();
                nineGagMainPageContent = await responseMessage.Content.ReadAsStringAsync();
            }
            catch (Exception exception)
            {
                throw new NineGagException("The 9GAG main page could not be retrieved. Maybe there is no internet connection available.", exception);
            }

            // Tries to parse the HTML of the 9GAG main page, if the HTML could not be parsed, then an exception is thrown
            IHtmlDocument htmlDocument;
            try
            {
                htmlDocument = await this.htmlParser.ParseAsync(nineGagMainPageContent);
            }
            catch (Exception exception)
            {
                throw new NineGagException("The HTML of the 9GAG main page could not be parsed. This could be an indicator, that the 9GAG website is down or its content has changed. If this problem keeps coming, then please report this problem to 9GAG or the maintainer of the library.", exception);
            }

            // Tries to retrieve the sections of 9GAG, if they could not be retrieved, then an exception is thrown
            try
            {
                IHtmlCollection<IElement> otherSections = htmlDocument.QuerySelectorAll("li.badge-section-menu-items > a");
                foreach (IElement otherSection in otherSections)
                    sections.Add(Section.FromHtml(otherSection));
            }
            catch (Exception exception)
            {
                throw new NineGagException("One of the 9GAG sections could not be retrieved. Maybe the website structure of 9GAG has changed. If so, please report this error to the maintainer of the library.", exception);
            }
            
            // Returns the parsed sections
            return sections;
        }

        /// <summary>
        /// Gets all the sections of 9GAG. Sections are like categories and contain the actual content.
        /// </summary>
        /// <exception cref="NineGagException">If anything goes wrong during the retrieval of the sections, an <see cref="NineGagException"/> exception is thrown.</exception>
        /// <returns>Returns a list of all the sections that are currently available on 9GAG.</returns>
        public Task<IEnumerable<Section>> GetSectionsAsync() => this.GetSectionsAsync(new CancellationTokenSource().Token);

        #endregion

        #region Public Post Methods

        /// <summary>
        /// Gets the first page of posts for the specified section and sub-section.
        /// </summary>
        /// <param name="section">The section for which the posts are to be retrieved.</param>
        /// <param name="subSection">The sub-section for which the posts are to be retrieved.</param>
        /// <param name="cancellationToken">The cancellation token, which can be used to cancel the retrieval of the page.</param>
        /// <exception cref="NineGagException">If anything goes wrong during the retrieval of the page, an <see cref="NineGagException"/> exception is thrown.</exception>
        /// <returns>Returns the first page of the specified section.</returns>
        public Task<Page> GetPostsAsync(Section section, SubSection subSection, CancellationToken cancellationToken) => this.GetPostsAsync(new Uri(string.Concat(section.RelativeUri.OriginalString, subSection == SubSection.Hot ? NineGagClient.hotPath : NineGagClient.freshPath), UriKind.Relative), cancellationToken);

        /// <summary>
        /// Gets the first page of posts for the specified section and sub-section.
        /// </summary>
        /// <param name="section">The section for which the posts are to be retrieved.</param>
        /// <param name="subSection">The sub-section for which the posts are to be retrieved.</param>
        /// <exception cref="NineGagException">If anything goes wrong during the retrieval of the page, an <see cref="NineGagException"/> exception is thrown.</exception>
        /// <returns>Returns the first page of the specified section.</returns>
        public Task<Page> GetPostsAsync(Section section, SubSection subSection) => this.GetPostsAsync(section, subSection, new CancellationTokenSource().Token);

        /// <summary>
        /// Gets the first page of posts for the specified section.
        /// </summary>
        /// <param name="section">The section for which the posts are to be retrieved.</param>
        /// <param name="cancellationToken">The cancellation token, which can be used to cancel the retrieval of the page.</param>
        /// <exception cref="NineGagException">If anything goes wrong during the retrieval of the page, an <see cref="NineGagException"/> exception is thrown.</exception>
        /// <returns>Returns the first page of the specified section.</returns>
        public Task<Page> GetPostsAsync(Section section, CancellationToken cancellationToken) => this.GetPostsAsync(section, SubSection.Hot, cancellationToken);

        /// <summary>
        /// Gets the first page of posts for the specified section.
        /// </summary>
        /// <param name="section">The section for which the posts are to be retrieved.</param>
        /// <exception cref="NineGagException">If anything goes wrong during the retrieval of the page, an <see cref="NineGagException"/> exception is thrown.</exception>
        /// <returns>Returns the first page of the specified section.</returns>
        public Task<Page> GetPostsAsync(Section section) => this.GetPostsAsync(section, new CancellationTokenSource().Token);

        /// <summary>
        /// Gets the first page of posts of the specified actuality.
        /// </summary>
        /// <param name="postActuality">The actuality of the posts that are to be retrieved.</param>
        /// <param name="cancellationToken">The cancellation token, which can be used to cancel the retrieval of the posts.</param>
        /// <returns>Returns the first page of posts of the specified actuality.</returns>
        public Task<Page> GetPostsAsync(PostActuality postActuality, CancellationToken cancellationToken)
        {
            switch (postActuality)
            {
                case PostActuality.Hot:
                    return this.GetPostsAsync(new Uri(NineGagClient.hotPath, UriKind.Relative), cancellationToken);
                case PostActuality.Trending:
                    return this.GetPostsAsync(new Uri(NineGagClient.trendingPath, UriKind.Relative), cancellationToken);
                default:
                    return this.GetPostsAsync(new Uri(NineGagClient.freshPath, UriKind.Relative), cancellationToken);
            }
        }

        /// <summary>
        /// Gets the first page of posts of the specified actuality.
        /// </summary>
        /// <param name="postActuality">The actuality of the posts that are to be retrieved.</param>
        /// <returns>Returns the first page of posts of the specified actuality.</returns>
        public Task<Page> GetPostsAsync(PostActuality postActuality) => this.GetPostsAsync(postActuality, new CancellationTokenSource().Token);

        /// <summary>
        /// Gets the first page of hot posts.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token, which can be used to cancel the retrieval of the posts.</param>
        /// <returns>Returns the first page of hot posts.</returns>
        public Task<Page> GetPostsAsync(CancellationToken cancellationToken) => this.GetPostsAsync(PostActuality.Hot, cancellationToken);

        /// <summary>
        /// Gets the first page of hot posts.
        /// </summary>
        /// <returns>Returns the first page of hot posts.</returns>
        public Task<Page> GetPostsAsync() => this.GetPostsAsync(new CancellationTokenSource().Token);

        /// <summary>
        /// Gets the page of posts after the specified page.
        /// </summary>
        /// <param name="page">The page for which the succeding page is to be retrieved.</param>
        /// <param name="cancellationToken">The cancellation token, which can be used to cancel the retrieval of the page.</param>
        /// <exception cref="NineGagException">If anything goes wrong during the retrieval of the page, an <see cref="NineGagException"/> exception is thrown.</exception>
        /// <returns>Returns the page of posts after the specified page.</returns>
        public Task<Page> GetPostsAsync(Page page, CancellationToken cancellationToken) => this.GetPostsAsync(page.NextPageUri, cancellationToken);

        /// <summary>
        /// Gets the page of posts after the specified page.
        /// </summary>
        /// <param name="page">The page for which the succeding page is to be retrieved.</param>
        /// <exception cref="NineGagException">If anything goes wrong during the retrieval of the page, an <see cref="NineGagException"/> exception is thrown.</exception>
        /// <returns>Returns the page of posts after the specified page.</returns>
        public Task<Page> GetPostsAsync(Page page) => this.GetPostsAsync(page, new CancellationTokenSource().Token);

        #endregion

        #region Public Authentication Methods

        /// <summary>
        /// Signs the user in to 9GAG and makes it possible to view "not safe for work" posts, as well as comment on posts and upvote posts.
        /// </summary>
        /// <param name="emailAddress">The email address of the user, which is the user name used for the sign in.</param>
        /// <param name="password">The password of the user, which is used for the sign in.</param>
        /// <param name="cancellationToken">The cancellation token, which can be used to cancel the sign in process.</param>
        /// <returns>Returns a value that determines whether the sign in process was successful.</returns>
        public async Task<bool> SignInAsync(string emailAddress, string password, CancellationToken cancellationToken)
        {
            // Checks if the user is already signed in, in that case he is signed out first
            if (this.IsUserSignedIn)
                await this.SignOutAsync();

            // Tries to get the sign in page of the 9GAG website, if it could not be retrieved, then an exception is thrown
            string nineGagSignInPageContent;
            try
            {
                HttpResponseMessage responseMessage = await this.httpClient.GetAsync(NineGagClient.signInPath, cancellationToken);
                nineGagSignInPageContent = await responseMessage.Content.ReadAsStringAsync();
            }
            catch (Exception exception)
            {
                throw new NineGagException("The 9GAG sign in page could not be retrieved. Maybe there is no internet connection available.", exception);
            }

            // Tries to parse the HTML of the 9GAG sing in page, if the HTML could not be parsed, then an exception is thrown
            IHtmlDocument htmlDocument;
            try
            {
                htmlDocument = await this.htmlParser.ParseAsync(nineGagSignInPageContent);
            }
            catch (Exception exception)
            {
                throw new NineGagException("The HTML of the 9GAG sing in page could not be parsed. This could be an indicator, that the 9GAG website is down or its content has changed. If this problem keeps coming, then please report this problem to 9GAG or the maintainer of the library.", exception);
            }

            // Tries to parse the sign in form, which contains the fields, which are needed to sign in the user, if the sign in form could not be parsed, then an exception is thrown
            string crossSiteRequestForgeryToken, location;
            try
            {
                IElement signInForm = htmlDocument.QuerySelector("#login-email");
                crossSiteRequestForgeryToken = signInForm.QuerySelector("#jsid-login-form-csrftoken").GetAttribute("value");
                location = signInForm.Children.FirstOrDefault(child => child.GetAttribute("name") == "location").GetAttribute("value");
            }
            catch (Exception exception)
            {
                throw new NineGagException("The sign in form could not be parsed. Maybe the website structure of 9GAG has changed. If so, please report this error to the maintainer of the library.", exception);
            }

            // Tries to send a request to 9GAG in order to sign in the user, if the user could not be signed in, then an exception is thrown
            try
            {
                // Sends the request to sign the user in
                HttpResponseMessage responseMessage = await this.httpClient.PostAsync(NineGagClient.signInPath, new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["csrftoken"] = crossSiteRequestForgeryToken,
                    ["next"] = NineGagClient.baseUri.OriginalString,
                    ["location"] = location,
                    ["username"] = emailAddress,
                    ["password"] = password
                }), cancellationToken);

                // Validates that the user is actually signed in, which is when the status code is 302 Found, when the user could not be signed in, then the status code is 200 OK
                this.IsUserSignedIn = responseMessage.StatusCode == HttpStatusCode.Found;
            }
            catch (Exception)
            {
                // Since an exception was thrown, the user could not be signed in successfully
                this.IsUserSignedIn = false;
            }

            // Returns true if the user was signed in successfully and false otherwise
            return this.IsUserSignedIn;
        }

        /// <summary>
        /// Signs the user in to 9GAG and makes it possible to view "not safe for work" posts, as well as comment on posts and upvote posts.
        /// </summary>
        /// <param name="emailAddress">The email address of the user, which is the user name used for the sign in.</param>
        /// <param name="password">The password of the user, which is used for the sign in.</param>
        /// <returns>Returns a value that determines whether the sign in process was successful.</returns>
        public Task<bool> SignInAsync(string emailAddress, string password) => this.SignInAsync(emailAddress, password, new CancellationTokenSource().Token);

        /// <summary>
        /// Signs the user out of 9GAG.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token, which can be used to cancel the sign out action.</param>
        /// <returns>Returns <c>true</c> if the user was signed out successfully.</returns>
        public async Task<bool> SignOutAsync(CancellationToken cancellationToken)
        {
            // Checks if the user is signed in, if not, then nothing needs to be done
            if (!this.IsUserSignedIn)
                return true;

            // Tries to sign the user out, if anything went wrong, then an exception is thrown
            try
            {
                // Makes the request to sign the user out
                HttpResponseMessage responseMessage = await this.httpClient.GetAsync(NineGagClient.signOutPath, cancellationToken);

                // Validates that the user was successfully signed out, which is when the status code is 302 Found, if the user could not be signed out, then an HTTP 200 OK is returned
                this.IsUserSignedIn = responseMessage.StatusCode != HttpStatusCode.Found;
            }
            catch (Exception) { }

            // Returns true if the user was signed out successfully and false otherwise
            return !this.IsUserSignedIn;
        }

        /// <summary>
        /// Signs the user out of 9GAG.
        /// </summary>
        /// <returns>Returns <c>true</c> if the user was signed out successfully.</returns>
        public Task<bool> SignOutAsync() => this.SignOutAsync(new CancellationTokenSource().Token);

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Disposes of the resources acquired by the <see cref="NineGagClient"/>.
        /// </summary>
        public void Dispose()
        {
            // Calls the dispose method, which can be overridden by sub-classes to dispose of further resources
            this.Dispose(true);

            // Suppresses the finalization of this object by the garbage collector, because the resources have already been disposed of
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// Disposes of all the resources acquired by the <see cref="NineGagClient"/>. This method can be overridden by sub-classes to dispose of further resources.
        /// </summary>
        /// <param name="disposingManagedResources">Determines whether managed resources should be disposed of or only unmanaged resources.</param>
        protected virtual void Dispose(bool disposingManagedResources)
        {
            // Checks if the 9GAG client has already been disposed of
            if (this.IsDisposed)
                throw new ObjectDisposedException("The 9GAG client has already been disposed of.");
            this.IsDisposed = true;

            // Checks if unmanaged resources should be disposed of
            if (disposingManagedResources)
            {
                // Checks if the HTTP client has already been disposed of, if not then it is disposed of
                if (this.httpClient != null)
                    this.httpClient.Dispose();
            }
        }

        #endregion
    }
}