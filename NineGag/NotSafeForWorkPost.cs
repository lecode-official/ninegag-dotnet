
#region Using Directives

using AngleSharp.Dom.Html;

#endregion

namespace NineGag
{
    /// <summary>
    /// Represents a post, which contains explicit content and can only be viewed if the user is signed in. Therefore the post contains only the base information.
    /// </summary>
    public class NotSafeForWorkPost : Post
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