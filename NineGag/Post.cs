
namespace NineGag
{
    /// <summary>
    /// Represents the base class for all kinds of posts offered by 9GAG.
    /// </summary>
    public class Post
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the title of the post.
        /// </summary>
        public string Title { get; internal set; }
        
        /// <summary>
        /// Gets the number of up-votes that the post received.
        /// </summary>
        public int NumberOfUpVotes { get; internal set; }

        /// <summary>
        /// Gets or sets the number of comments of the post.
        /// </summary>
        public int NumberOfComments { get; internal set; }

        #endregion
    }
}