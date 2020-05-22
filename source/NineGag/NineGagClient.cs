
#region Using Directives

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;

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
            IConfiguration configuration = Configuration.Default
                .WithDefaultLoader();
            this.browsingContext = BrowsingContext.New(configuration);
        }

        #endregion

        #region Private Static Fields

        /// <summary>
        /// Contains the 9GAG base URI.
        /// </summary>
        private static readonly Url baseUrl = new Url("https://9gag.com");

        #endregion

        #region Private Fields

        /// <summary>
        /// Contains the browsing context, which is used to load and parse the 9GAG website.
        /// </summary>
        private readonly IBrowsingContext browsingContext;

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
        /// <exception cref="NineGagException">If anything goes wrong during the retrieval of the sections, a <see cref="NineGagException"/> is thrown.</exception>
        /// <returns>Returns a list of all the sections that are currently available on 9GAG.</returns>
        public async Task<IEnumerable<Section>> GetSectionsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            // Downloads the 9GAG index page, which, among other things, contains the links to all sections
            using (IDocument document = await this.browsingContext.OpenAsync(NineGagClient.baseUrl, cancellationToken))
            {
                // Creates a new list of sections, which will be returned
                List<Section> sections = new List<Section>();

                // Gets the HTML elements, which contains the section links (there are two: popular and (other) sections)
                IHtmlCollection<IElement> sectionContainers = document.QuerySelectorAll("#jsid-section-picker-sections, #jsid-section-picker-popular");
                if (sectionContainers.Length == 0)
                    throw new NineGagException("The sections could not be retrieved. Maybe the website structure of 9GAG has changed. If so, please report this error to the maintainer of this library.");

                // Goes through the section containers and retrieves the sections from them
                foreach (IElement sectionContainer in sectionContainers)
                {
                    // Gets all section links from the current container
                    IHtmlCollection<IElement> sectionLinks = sectionContainer.QuerySelectorAll("a.badge-upload-section-list-item");
                    if (sectionLinks.Length == 0)
                        throw new NineGagException("The sections could not be retrieved. Maybe the website structure of 9GAG has changed. If so, please report this error to the maintainer of this library.");

                    // Goes through all section links and extracts the section information from them
                    foreach (IElement sectionLink in sectionLinks)
                    {
                        // Parses the name of the section
                        IElement nameElement = sectionLink.QuerySelector("div.text > h3");
                        if (nameElement == null)
                            throw new NineGagException("An error occurred while retrieving a section: the name of the section could not be parsed.");
                        string name = nameElement.Text().Trim();

                        // Parses the description of the section
                        IElement descriptionElement = sectionLink.QuerySelector("div.text > p");
                        if (descriptionElement == null)
                            throw new NineGagException("An error occurred while retrieving a section: the description of the section could not be parsed.");
                        string description = descriptionElement.Text().Trim();

                        // Parses the relative URL of the section
                        IElement relativeUrlElement = sectionLink.QuerySelector("div.badge-upload-section-list-item-selector");
                        if (relativeUrlElement == null || !relativeUrlElement.HasAttribute("data-url"))
                            throw new NineGagException("An error occurred while retrieving a section: the URL of the section could not be parsed.");
                        string relativeUrl = relativeUrlElement.GetAttribute("data-url");

                        // Parses the URL of the icon of the section
                        IElement iconUrlElement = sectionLink.QuerySelector("div.icon");
                        if (iconUrlElement == null || !iconUrlElement.HasAttribute("style"))
                            throw new NineGagException("An error occurred while retrieving a section: the URL of the icon of the section could not be parsed.");
                        string iconUrl = iconUrlElement.GetAttribute("style");
                        Regex urlExtractionRegex = new Regex(@"background-image: url\((?<iconUrl>.*)\)");
                        Match urlExtractionMatch = urlExtractionRegex.Match(iconUrl);
                        if (!urlExtractionMatch.Success || !urlExtractionMatch.Groups["iconUrl"].Success)
                            throw new NineGagException("An error occurred while retrieving a section: the URL of the icon of the section could not be parsed.");
                        iconUrl = urlExtractionMatch.Groups["iconUrl"].Value;

                        // Adds the section to the list of parsed sections
                        sections.Add(new Section(name, description, iconUrl, new Url(NineGagClient.baseUrl, relativeUrl)));
                    }
                }

                // Returns the list of the parsed sections
                return sections;
            }
        }

        /// <summary>
        /// Gets the posts of the specified section.
        /// </summary>
        /// <param name="section">The section for which the posts are to be retrieved.</param>
        /// <param name="cancellationToken">The cancellation token, which can be used to cancel the retrieval of the posts.</param>
        /// <exception cref="NineGagException">If anything goes wrong during the retrieval of the posts, a <see cref="NineGagException"/> is thrown.</exception>
        /// <returns>Returns a list of all the posts of the specified section.</returns>
        public async Task<IEnumerable<string>> GetPostsAsync(Section section, CancellationToken cancellationToken = default(CancellationToken))
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
        /// Disposes of all the resources acquired by the <see cref="NineGagClient"/>. This method can be overridden by sub-classes to dispose of further resources.
        /// </summary>
        /// <param name="disposingManagedResources">Determines whether managed resources should be disposed of or only unmanaged resources.</param>
        protected virtual void Dispose(bool disposingManagedResources)
        {
            // Checks if the 9GAG client has already been disposed of
            if (this.IsDisposed)
                throw new ObjectDisposedException("The 9GAG client has already been disposed of.");
            this.IsDisposed = true;

            // Checks if managed resources should be disposed of
            if (disposingManagedResources)
            {
                // Disposes of the browsing context
                if (this.browsingContext != null)
                    this.browsingContext.Dispose();
            }
        }

        #endregion
    }
}
