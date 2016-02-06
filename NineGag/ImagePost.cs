
namespace NineGag
{
    /// <summary>
    /// Represents a post on 9GAG with an image content.
    /// </summary>
    public class ImagePost : Post
    {
        #region Public Properties

        /// <summary>
        /// Gets a value that determines whether this image post is a long post (this means, that the content of the image post is only the long post cover and not the full image).
        /// </summary>
        public bool IsLongPost { get; internal set; }

        #endregion
    }
}