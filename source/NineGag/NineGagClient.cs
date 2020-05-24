
#region Using Directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
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
            this.httpClient.BaseAddress = NineGagClient.baseUrl;
        }

        #endregion

        #region Private Static Fields

        /// <summary>
        /// Contains the base URL of 9GAG.
        /// </summary>
        private static readonly Url baseUrl = new Url("https://9gag.com");

        /// <summary>
        /// Contains the root path of the website.
        /// </summary>
        private static readonly string rootPath = "/";

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

        #region Public Methods

        /// <summary>
        /// Gets all the sections of 9GAG. Sections are like categories and contain the actual content.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token, which can be used to cancel the retrieval of the sections.</param>
        /// <exception cref="NineGagException">
        /// If anything goes wrong during the retrieval of the sections, a <see cref="NineGagException"/> is thrown.
        /// </exception>
        /// <returns>Returns a list of all the sections that are currently available on 9GAG.</returns>
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
            catch (Exception exception)
            {
                throw new NineGagException("The 9GAG main page could not be retrieved.", exception);
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
        /// Gets the posts of the specified section.
        /// </summary>
        /// <param name="section">The section for which the posts are to be retrieved.</param>
        /// <param name="cancellationToken">The cancellation token, which can be used to cancel the retrieval of the posts.</param>
        /// <exception cref="NineGagException">
        /// If anything goes wrong during the retrieval of the posts, a <see cref="NineGagException"/> is thrown.
        /// </exception>
        /// <returns>Returns a list of all the posts of the specified section.</returns>
        public async Task<IEnumerable<string>> GetPostsAsync(
            Section section,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // https://9gag.com/v1/group-posts/group/funny/kind/hot?after=a8GW4mY%2Can4Y1XB%2CaXgbwbV&c=10

            List<string> posts = new List<string>();

            return posts;
        }

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
