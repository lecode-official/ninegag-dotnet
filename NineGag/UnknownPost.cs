
#region Using Directives

using AngleSharp.Dom.Html;

#endregion

namespace NineGag
{
    /// <summary>
    /// Represents a post whose type is unknown, it only contains the base information.
    /// </summary>
    public class UnknownPost : Post
    {
        #region Post Implementation

        /// <summary>
        /// Parses the detail information of the post.
        /// </summary>
        /// <param name="htmlDocument">The HTML document, which contains the details page of the post.</param>
        protected override void ParseDetailInformation(IHtmlDocument htmlDocument) { }

        #endregion
    }
}