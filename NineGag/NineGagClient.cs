
#region Using Directives

using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#endregion

namespace NineGag
{
    /// <summary>
    /// Represents the 9GAG client, which can be used to access 9GAG and get raw data from it.
    /// </summary>
    public class NineGagClient
    {
        #region Private Static Fields

        /// <summary>
        /// Contains the 9GAG base URI.
        /// </summary>
        private static readonly Uri baseUri = new Uri("http://9gag.com", UriKind.Absolute);

        /// <summary>
        /// Contains a regular expression, which detects non-numeric characters. This can be used to filter non-numeric characters from a string to retrieve an integer.
        /// </summary>
        private static readonly Regex numericFilterRegex = new Regex("[^0-9]");

        /// <summary>
        /// Contains a regular expression, which is used to parse the URI for the next page and extract the ID of the next page and the number of posts to retrieve.
        /// </summary>
        private static readonly Regex uriQueryRegex = new Regex(@"^/[^/]*/?\?id=(?<PageId>([^&]+))&c=(?<Count>([0-9]+))$");

        #endregion

        #region Private Fields

        /// <summary>
        /// Contains an HTTP client, which is used to call the 9GAG website.
        /// </summary>
        private readonly HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Contains an HTML parser, which is used to parse the HTML retrieved from the 9GAG website.
        /// </summary>
        private readonly HtmlParser htmlParser = new HtmlParser();

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets all the sections of 9GAG. Sections are like categories and contain the actual content.
        /// </summary>
        /// <returns>Returns a list of all the sections that are currently available on 9GAG.</returns>
        public async Task<IEnumerable<Section>> GetSectionsAsync()
        {
            // Creates a new list for the result set of sections
            List<Section> sections = new List<Section>();

            // Gets the main page of the 9GAG website
            HttpResponseMessage responseMessage = await this.httpClient.GetAsync(NineGagClient.baseUri);
            string responseMessageContent = await responseMessage.Content.ReadAsStringAsync();

            // Parses the HTML of the 9GAG main page
            IHtmlDocument htmlDocument = await this.htmlParser.ParseAsync(responseMessageContent);

            // Gets the main sections of 9GAG (hot, trending, fresh)
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

            // Gets the other, less known 9GAG sections
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

            // Returns the parsed sections
            return sections;
        }

        /// <summary>
        /// Gets the page of posts for the specified section and the next page of the specfied page.
        /// </summary>
        /// <param name="section">The section for which the posts are to be retrieved.</param>
        /// <param name="page">The page before the page that is to be retrieved.</param>
        /// <returns>Returns the page of posts from the page after the specified page of the specified section.</returns>
        public async Task<Page> GetPostsAsync(Section section, Page page)
        {
            // Builds the absolute URI for the section
            Uri absoluteUri;
            if (page == null)
                absoluteUri = new Uri(NineGagClient.baseUri, section.RelativeUri);
            else
                absoluteUri = new Uri(NineGagClient.baseUri, $"{section.RelativeUri.OriginalString}/?id={page.NextPageId}&c={page.NumberOfPostsToRetrieve}");

            // Gets the page of the 9GAG website for the specified section
            HttpResponseMessage responseMessage = await this.httpClient.GetAsync(absoluteUri);
            string responseMessageContent = await responseMessage.Content.ReadAsStringAsync();

            // Parses the HTML of the section page
            IHtmlDocument htmlDocument = await this.htmlParser.ParseAsync(responseMessageContent);

            // Gets all the posts, parses them and them to the result set
            List<Post> posts = new List<Post>();
            IHtmlCollection<IElement> postElements = htmlDocument.QuerySelectorAll("article");
            foreach (IElement postElement in postElements)
            {
                // Parses the number of comments and the number of up-votes
                int numberOfComments, numberOfUpVotes;
                if (!int.TryParse(NineGagClient.numericFilterRegex.Replace(postElement.QuerySelector(".comment").TextContent.Trim(), string.Empty), out numberOfComments))
                    numberOfComments = 0;
                if (!int.TryParse(NineGagClient.numericFilterRegex.Replace(postElement.QuerySelector(".badge-item-love-count").TextContent.Trim(), string.Empty), out numberOfUpVotes))
                    numberOfUpVotes = 0;

                // Checks if the post is a video or an image post
                Nullable<bool> isImagePost = null;
                IElement contentElement;
                if ((contentElement = postElement.QuerySelector("video")) != null)
                    isImagePost = false;
                else if ((contentElement = postElement.QuerySelector("img")) != null)
                    isImagePost = true;

                // Gets the content of the post
                Post post;
                if (isImagePost.HasValue && !isImagePost.Value)
                {
                    post = new VideoPost
                    {
                        VideoUri = new Uri(contentElement.Children.FirstOrDefault(child => child.GetAttribute("type") == "video/mp4").GetAttribute("src"), UriKind.Absolute),
                        ThumbnailUri = new Uri(contentElement.GetAttribute("poster"), UriKind.Absolute)
                    };
                }
                else if (isImagePost.HasValue && isImagePost.Value)
                {
                    post = new ImagePost { PictureUri = new Uri(contentElement.GetAttribute("src"), UriKind.Absolute) };
                }
                else
                {
                    post = new Post();
                }

                // Sets the general information of the post
                post.Title = postElement.QuerySelector("header").TextContent.Trim();
                post.NumberOfComments = numberOfComments;
                post.NumberOfUpVotes = numberOfUpVotes;

                // Adds the newly created post to the result set
                posts.Add(post);
            }

            // Parses the URI of the next page to retrieve the ID of the next page as well as the number of posts to retrieve
            string uriQuery = htmlDocument.QuerySelector("a.badge-load-more-post").GetAttribute("href").Trim();
            Match match = NineGagClient.uriQueryRegex.Match(uriQuery);
            string nextPageId = match.Groups["PageId"].Value;
            int numberOfPostsToRetrieve;
            if (!int.TryParse(match.Groups["Count"].Value, out numberOfPostsToRetrieve))
                numberOfPostsToRetrieve = 10;

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
        /// Gets the first page of posts for the specified section.
        /// </summary>
        /// <param name="section">The section for which the posts are to be retrieved.</param>
        /// <returns>Returns the fist page of posts of the specified section.</returns>
        public Task<Page> GetPostsAsync(Section section) => this.GetPostsAsync(section, null);

        #endregion
    }
}