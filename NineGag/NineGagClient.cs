
#region Using Directives

using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
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
            this.httpClient = new HttpClient(new HttpClientHandler { CookieContainer = cookieContainer })
            {
                BaseAddress = NineGagClient.baseUri
            };
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
        /// Contains a regular expression, which is used to parse the URI for the next page and extract the ID of the next page and the number of posts to retrieve.
        /// </summary>
        private static readonly Regex uriQueryRegex = new Regex(@"^/[^/]*/?\?id=(?<PageId>([^&]+))&c=(?<Count>([0-9]+))$");

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
        public bool IsUserSignedIn
        {
            get
            {
                // Ensures that the sign in process was successful by checking the cookie container contains the ts1 cookie, which contains the authentication token
                CookieCollection cookieCollection = this.cookieContainer.GetCookies(NineGagClient.baseUri);
                foreach (Cookie cookie in cookieCollection)
                {
                    if (cookie.Name == "ts1" && !string.IsNullOrWhiteSpace(cookie.Value))
                        return true;
                }

                // Since the cookies could not be found, false is returned
                return false;
            }
        }

        #endregion

        #region Public Methods

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

            // Treis to retrieve the main sections of 9GAG (hot, trending, fresh), if they could not be retrieved, then an exception is thrown
            try
            {
                IElement hotSection = htmlDocument.QuerySelector("a.hot");
                sections.Add(new Section
                {
                    Title = hotSection.TextContent.Trim(),
                    Kind = SectionKind.Hot,
                    RelativeUri = NineGagClient.baseUri.MakeRelativeUri(new Uri(hotSection.GetAttribute("href"), UriKind.Absolute))
                });
                IElement trendingSection = htmlDocument.QuerySelector("a.trending");
                sections.Add(new Section
                {
                    Title = trendingSection.TextContent.Trim(),
                    Kind = SectionKind.Trending,
                    RelativeUri = NineGagClient.baseUri.MakeRelativeUri(new Uri(trendingSection.GetAttribute("href"), UriKind.Absolute))
                });
                IElement freshSection = htmlDocument.QuerySelector("a.fresh");
                sections.Add(new Section
                {
                    Title = freshSection.TextContent.Trim(),
                    Kind = SectionKind.Fresh,
                    RelativeUri = NineGagClient.baseUri.MakeRelativeUri(new Uri(freshSection.GetAttribute("href"), UriKind.Absolute))
                });
            }
            catch (Exception exception)
            {
                throw new NineGagException("One of the main sections (hot, trending, and fresh) of 9GAG could not be retrieved. Maybe the website structure of 9GAG has changed. If so, please report this error to the maintainer of the library.", exception);
            }

            // Tries to retrieve the other, less known 9GAG sections, if they could not be retrieved, then an exception is thrown
            try
            {
                IHtmlCollection<IElement> otherSections = htmlDocument.QuerySelectorAll("li.badge-section-menu-items > a");
                foreach (IElement otherSection in otherSections)
                {
                    // Parses the section kind, if the section kind could not be parsed, then the section kind is set to unknown
                    SectionKind sectionKind;
                    if (!Enum.TryParse(otherSection.TextContent.Trim().Replace(" ", string.Empty), true, out sectionKind))
                        sectionKind = SectionKind.Unknown;

                    // Creates the new section and adds it to the list of sections
                    sections.Add(new Section
                    {
                        Title = otherSection.TextContent.Trim(),
                        Kind = sectionKind,
                        RelativeUri = NineGagClient.baseUri.MakeRelativeUri(new Uri(otherSection.GetAttribute("href"), UriKind.Absolute))
                    });
                }
            }
            catch (Exception exception)
            {
                throw new NineGagException("One of the other sections of 9GAG could not be retrieved. Maybe the website structure of 9GAG has changed. If so, please report this error to the maintainer of the library.", exception);
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

        /// <summary>
        /// Gets the page of posts for the specified section and the next page of the specfied page.
        /// </summary>
        /// <param name="section">The section for which the posts are to be retrieved.</param>
        /// <param name="page">The page before the page that is to be retrieved.</param>
        /// <param name="cancellationToken">The cancellation token, which can be used to cancel the retrieval of the page.</param>
        /// <exception cref="NineGagException">If anything goes wrong during the retrieval of the page, an <see cref="NineGagException"/> exception is thrown.</exception>
        /// <returns>Returns the page of posts from the page after the specified page of the specified section.</returns>
        public async Task<Page> GetPostsAsync(Section section, Page page, CancellationToken cancellationToken)
        {
            // Tries to build the absolute URI for the section, if anything goes wrong during the creation of the URI, an exception is thrown
            Uri absoluteUri;
            try
            {
                if (page != null && !string.IsNullOrWhiteSpace(page.NextPageId))
                    absoluteUri = new Uri($"{section.RelativeUri.OriginalString}/?id={page.NextPageId}&c={page.NumberOfPostsToRetrieve}", UriKind.Relative);
                else
                    absoluteUri = section.RelativeUri;
            }
            catch (Exception exception)
            {
                throw new NineGagException("The absolute URI for the page could not be build. This may be due to an internal bug in 9GAG.NET. If so, please report this error to the maintainer of the library.", exception);
            }

            // Tries to get the section page of the 9GAG website, if it could not be retrieved, then an exception is thrown
            string nineGagSectionPageContent;
            try
            {
                HttpResponseMessage responseMessage = await this.httpClient.GetAsync(absoluteUri, cancellationToken);
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
                IHtmlCollection<IElement> postElements = htmlDocument.QuerySelectorAll("article");
                foreach (IElement postElement in postElements)
                {
                    // Checks if the post is a video or an image post
                    PostKind postKind;
                    IElement contentElement;
                    if ((contentElement = postElement.QuerySelector("video")) != null)
                        postKind = PostKind.Video;
                    else if ((contentElement = postElement.QuerySelector("img")) != null)
                        postKind = PostKind.Image;
                    else
                        postKind = PostKind.Unknown;

                    // Gets the content of the post
                    Post post;
                    if (postKind == PostKind.Video)
                    {
                        post = new VideoPost
                        {
                            Content = contentElement.GetElementsByTagName("source").Select(child => new Content
                            {
                                Uri = new Uri(child.GetAttribute("src"), UriKind.Absolute),
                                Kind = child.GetAttribute("type").ToUpperInvariant() == "VIDEO/MP4" ? ContentKind.Mp4 : ContentKind.WebM
                            }).ToList(),
                            ThumbnailUri = new Uri(contentElement.GetAttribute("poster"), UriKind.Absolute)
                        };
                    }
                    else if (postKind == PostKind.Image)
                    {
                        post = new ImagePost
                        {
                            Content = new List<Content>
                        {
                            new Content
                            {
                                Uri = new Uri(contentElement.GetAttribute("src"), UriKind.Absolute),
                                Kind = ContentKind.Jpeg
                            }
                        },
                            IsLongPost = contentElement.GetAttribute("src").ToUpperInvariant().Contains("LONG-POST")
                        };
                    }
                    else
                    {
                        post = new UnknownPost();
                    }

                    // Sets the general information of the post
                    post.Id = postElement.GetAttribute("data-entry-id");
                    post.Title = postElement.QuerySelector("header").TextContent.Trim();
                    post.IsNotSafeForWork = postElement.QuerySelector(".nsfw-post") != null;
                    int numberOfComments, numberOfUpVotes;
                    int.TryParse(postElement.GetAttribute("data-entry-comments").Trim(), out numberOfComments);
                    int.TryParse(postElement.GetAttribute("data-entry-votes").Trim(), out numberOfUpVotes);
                    post.NumberOfComments = numberOfComments;
                    post.NumberOfUpVotes = numberOfUpVotes;

                    // Adds the newly created post to the result set
                    posts.Add(post);
                }
            }
            catch (Exception exception)
            {
                throw new NineGagException("The posts of the 9GAG section page could not be parsed. Maybe the website structure of 9GAG has changed. If so, please report this error to the maintainer of the library.", exception);
            }

            // Tries to parse the URI of the next page to retrieve the ID of the next page as well as the number of posts to retrieve
            string nextPageId = null;
            int numberOfPostsToRetrieve = 0;
            try
            {
                string uriQuery = htmlDocument.QuerySelector("a.badge-load-more-post").GetAttribute("href").Trim();
                Match match = NineGagClient.uriQueryRegex.Match(uriQuery);
                nextPageId = match.Groups["PageId"].Value;
                int.TryParse(match.Groups["Count"].Value, out numberOfPostsToRetrieve);
            }
            catch (Exception) { }

            // Tries to fetch the details of the all the posts, if the details could not be fetched, then an exception is thrown
            try
            {
                await Task.WhenAll(posts.Select(post => post.FetchDetailsAsync(this.httpClient, cancellationToken)));
            }
            catch (Exception exception)
            {
                throw new NineGagException("The details of the posts could not be fetched. Maybe the website structure of 9GAG has changed. If so, please report this error to the maintainer of the library.", exception);
            }

            // Returns the result set, which contains all the posts
            return new Page
            {
                CurrentPageId = page?.NextPageId,
                NextPageId = nextPageId,
                NumberOfPostsToRetrieve = numberOfPostsToRetrieve,
                Posts = posts
            };
        }

        /// <summary>
        /// Gets the page of posts for the specified section and the next page of the specfied page.
        /// </summary>
        /// <param name="section">The section for which the posts are to be retrieved.</param>
        /// <param name="page">The page before the page that is to be retrieved.</param>
        /// <exception cref="NineGagException">If anything goes wrong during the retrieval of the page, an <see cref="NineGagException"/> exception is thrown.</exception>
        /// <returns>Returns the page of posts from the page after the specified page of the specified section.</returns>
        public Task<Page> GetPostsAsync(Section section, Page page) => this.GetPostsAsync(section, page, new CancellationTokenSource().Token);

        /// <summary>
        /// Gets the first page of posts for the specified section.
        /// </summary>
        /// <param name="section">The section for which the posts are to be retrieved.</param>
        /// <param name="cancellationToken">The cancellation token, which can be used to cancel the retrieval of the page.</param>
        /// <exception cref="NineGagException">If anything goes wrong during the retrieval of the page, an <see cref="NineGagException"/> exception is thrown.</exception>
        /// <returns>Returns the fist page of posts of the specified section.</returns>
        public Task<Page> GetPostsAsync(Section section, CancellationToken cancellationToken) => this.GetPostsAsync(section, null, cancellationToken);

        /// <summary>
        /// Gets the first page of posts for the specified section.
        /// </summary>
        /// <param name="section">The section for which the posts are to be retrieved.</param>
        /// <exception cref="NineGagException">If anything goes wrong during the retrieval of the page, an <see cref="NineGagException"/> exception is thrown.</exception>
        /// <returns>Returns the fist page of posts of the specified section.</returns>
        public Task<Page> GetPostsAsync(Section section) => this.GetPostsAsync(section, new CancellationTokenSource().Token);

        /// <summary>
        /// Signs the user in to 9GAG and makes it possible to view "not safe for work" posts, as well as comment on posts and upvote posts.
        /// </summary>
        /// <param name="emailAddress">The email address of the user, which is the user name used for the sign in.</param>
        /// <param name="password">The password of the user, which is used for the sign in.</param>
        /// <param name="cancellationToken">The cancellation token, which can be used to cancel the sign in process.</param>
        /// <returns>Returns a value that determines whether the sign in process was successful.</returns>
        public async Task<bool> SignInAsync(string emailAddress, string password, CancellationToken cancellationToken)
        {
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
                HttpResponseMessage responseMessage = await this.httpClient.PostAsync(NineGagClient.signInPath, new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["csrftoken"] = crossSiteRequestForgeryToken,
                    ["next"] = NineGagClient.baseUri.OriginalString,
                    ["location"] = location,
                    ["username"] = emailAddress,
                    ["password"] = password
                }), cancellationToken);
                responseMessage.EnsureSuccessStatusCode();
            }
            catch (Exception) { }

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

        public async Task<bool> SignOutAsync(CancellationToken cancellationToken)
        {
            // Tries to sign the user out, if anything went wrong, then an exception is thrown
            try
            {
                HttpResponseMessage responseMessage = await this.httpClient.GetAsync(NineGagClient.signOutPath, cancellationToken);
                responseMessage.EnsureSuccessStatusCode();
            }
            catch (Exception) { }

            return !this.IsUserSignedIn;
        }

        public Task<bool> SignOutAsync() => this.SignOutAsync(new CancellationTokenSource().Token);

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Disposes of the resources acquired by the <see cref="NineGagClient"/>.
        /// </summary>
        public void Dispose()
        {
            // Calls the dispose method, which can be overridden by sub-classes to dispose of further resources
            Dispose(true);

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
                {
                    this.httpClient.Dispose();
                }
            }
        }

        #endregion
    }
}