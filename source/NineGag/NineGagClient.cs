
#region Using Directives

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

#endregion

namespace NineGag
{
    /// <summary>
    /// Represents the client which acts as the interface to the 9GAG website.
    /// </summary>
    public class NineGagClient : IDisposable
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="NineGagClient"/> instance.
        /// </summary>
        public NineGagClient()
        {
            this.cookieContainer = new CookieContainer();
            this.httpClient = new HttpClient(new HttpClientHandler
            {
                CookieContainer = cookieContainer,
                AllowAutoRedirect = false
            });
            this.httpClient.DefaultRequestHeaders.Add("User-Agent", $"9GAG.NET/{Assembly.GetEntryAssembly().GetName().Version}");
            this.httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            this.httpClient.BaseAddress = NineGagClient.baseUrl;
        }

        #endregion

        #region Private Static Fields

        /// <summary>
        /// Contains the base URL of 9GAG.
        /// </summary>
        private static readonly Uri baseUrl = new Uri("https://9gag.com", UriKind.Absolute);

        /// <summary>
        /// Contains the root path of the website.
        /// </summary>
        private static readonly string rootPath = "/";

        /// <summary>
        /// Contains the path of the 9GAG API to retrieve posts.
        /// </summary>
        private static readonly string postsPath = "/v1/group-posts/group/{0}/type/{1}";

        #endregion

        #region Private Fields

        /// <summary>
        /// Contains the cookie container of the HTTP client. It automatically stores the cookies that were received from the 9GAG server
        /// and sends them to the 9GAG server everytime a request is made.
        /// </summary>
        private readonly CookieContainer cookieContainer;

        /// <summary>
        /// Contains an HTTP client, which is used to make HTTP requests to the 9GAG server.
        /// </summary>
        private readonly HttpClient httpClient;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a value that determines whether the <see cref="NineGagClient"/> has already been disposed of.
        /// </summary>
        public bool IsDisposed { get; private set; }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the posts of the specified section and post kind.
        /// </summary>
        /// <param name="section">The ID of the section from which the posts are to be retrieved.</param>
        /// <param name="postKind">
        /// The post kind, which can either be "hot", "trending", or "fresh". The value trending is only allowed for the "default" section.
        /// </param>
        /// <param name="count">The number of posts that are to be retrieved.</param>
        /// <param name="cursor">
        /// The cursor from the previous call to the API. The cursor uniquely identifies the last post from the previous request and can be
        /// used to get the next batch of posts after that. This can be used for paging or infinite scrolling.
        /// </param>
        /// <param name="cancellationToken">The cancellation token, which can be used to cancel the retrieval of the posts.</param>
        /// <exception cref="NineGagException">
        /// If anything goes wrong during the retrieval of the posts, a <see cref="NineGagException"/> is thrown.
        /// </exception>
        /// <returns>Returns the retrieved posts.</returns>
        private async Task<IEnumerable<Post>> GetPostsAsync(
            string section,
            string postKind,
            int count,
            string cursor,
            CancellationToken cancellationToken)
        {
            // Makes a request to the 9GAG API to retrieve the posts
            string postsContent;
            try
            {
                // Generates the path to the posts of the specified section and post kind
                string path = string.Format(CultureInfo.InvariantCulture, NineGagClient.postsPath, section, postKind);

                // Prepares the query parameters for the request
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("c", count.ToString());
                if (!string.IsNullOrWhiteSpace(cursor))
                    parameters.Add("after", cursor);
                string queryParameters = string.Join("&", parameters.Select(keyValuePair => $"{keyValuePair.Key}={keyValuePair.Value}"));

                // Makes the request to the API to retrieve the posts
                HttpResponseMessage responseMessage = await this.httpClient.GetAsync(
                    $"{path}?{queryParameters}",
                    cancellationToken
                );
                responseMessage.EnsureSuccessStatusCode();
                postsContent = await responseMessage.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException exception)
            {
                throw new NineGagException("The 9GAG posts could not be retrieved.", exception);
            }

            // Parses the JSON containing the posts and returns them
            JObject posts = JObject.Parse(postsContent);
            return posts["data"]["posts"]
                .Children()
                .Select(post => post.ToObject<Post>())
                .ToList();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets all the sections of 9GAG. Sections are like categories and contain the actual content.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token, which can be used to cancel the retrieval of the sections.</param>
        /// <exception cref="NineGagException">
        /// If anything goes wrong during the retrieval of the sections, a <see cref="NineGagException"/> is thrown.
        /// </exception>
        /// <returns>Returns all the sections that are currently available on 9GAG.</returns>
        public async Task<SectionResult> GetSectionsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            // Retrieves the index page of the 9GAG website, which contains a complete list of all the sections of 9GAG
            string indexPageContent;
            try
            {
                HttpResponseMessage responseMessage = await this.httpClient.GetAsync(NineGagClient.rootPath, cancellationToken);
                responseMessage.EnsureSuccessStatusCode();
                indexPageContent = await responseMessage.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException exception)
            {
                throw new NineGagException("The sections could not be retrieved.", exception);
            }

            // Retrieves the 9GAG configuration (including the sections) from the server
            Match match = Regex.Match(indexPageContent, "window\\._config = JSON.parse\\(\"(?<sections>.*?)\"\\)");
            if (!match.Success)
                throw new NineGagException("The sections could not be retrieved.");
            string configurationJson = match.Groups["sections"].Value
                .Replace("\\\"", "\"")
                .Replace("\\\\/", "/");
            JObject configuration = JObject.Parse(configurationJson);

            // A function, which parses a list of sections
            IEnumerable<Section> parseSections(string kind)
            {
                return configuration["page"][kind]
                    .Children()
                    .Select(section => section.Children().First())
                    .Select(section => section.ToObject<Section>())
                    .ToList();
            }

            // Parses the different categories of sections
            IEnumerable<Section> sections = parseSections("sections");
            IEnumerable<Section> featuredSections = parseSections("sections");
            IEnumerable<Section> localSections = parseSections("sections");
            Section currentLocalSection = configuration["page"]["geoSection"].ToObject<Section>();

            // Returns the sections
            return new SectionResult(sections, featuredSections, localSections, currentLocalSection);
        }

        /// <summary>
        /// Gets the hot posts.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token, which can be used to cancel the retrieval of the posts.</param>
        /// <exception cref="NineGagException">
        /// If anything goes wrong during the retrieval of the posts, a <see cref="NineGagException"/> is thrown.
        /// </exception>
        /// <returns>Returns the retrieved hot posts.</returns>
        public Task<IEnumerable<Post>> GetHotPostsAsync(
            CancellationToken cancellationToken = default(CancellationToken)
        ) => this.GetPostsAsync("default", "hot", 10, null, cancellationToken);

        /// <summary>
        /// Gets the trending posts.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token, which can be used to cancel the retrieval of the posts.</param>
        /// <exception cref="NineGagException">
        /// If anything goes wrong during the retrieval of the posts, a <see cref="NineGagException"/> is thrown.
        /// </exception>
        /// <returns>Returns the retrieved trending posts.</returns>
        public Task<IEnumerable<Post>> GetTrendingPostsAsync(
            CancellationToken cancellationToken = default(CancellationToken)
        ) => this.GetPostsAsync("default", "trending", 10, null, cancellationToken);

        /// <summary>
        /// Gets the fresh posts.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token, which can be used to cancel the retrieval of the posts.</param>
        /// <exception cref="NineGagException">
        /// If anything goes wrong during the retrieval of the posts, a <see cref="NineGagException"/> is thrown.
        /// </exception>
        /// <returns>Returns the retrieved fresh posts.</returns>
        public Task<IEnumerable<Post>> GetFreshPostsAsync(
            CancellationToken cancellationToken = default(CancellationToken)
        ) => this.GetPostsAsync("default", "fresh", 10, null, cancellationToken);

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
        /// Disposes of all the resources acquired by the <see cref="NineGagClient"/>. This method can be overridden by sub-classes to
        /// dispose of further resources.
        /// </summary>
        /// <param name="disposingManagedResources">
        /// Determines whether managed resources should be disposed of or only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposingManagedResources)
        {
            // Checks if the 9GAG client has already been disposed of
            if (this.IsDisposed)
                throw new ObjectDisposedException("The 9GAG client has already been disposed of.");
            this.IsDisposed = true;

            // Checks if managed resources should be disposed of
            if (disposingManagedResources)
            {
                // Disposes of the HTTP client
                if (this.httpClient != null)
                    this.httpClient.Dispose();
            }
        }

        #endregion
    }
}
