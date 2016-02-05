
namespace NineGag
{
    /// <summary>
    /// Represents an enumeration for the different kinds of posts.
    /// </summary>
    internal enum PostKind
    {
        /// <summary>
        /// The kind of the post is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// The post is an image post.
        /// </summary>
        Image,

        /// <summary>
        /// The post is a video post.
        /// </summary>
        Video,

        /// <summary>
        /// The post is not safe for work (contains explicit content, the user has to sign in to view the content).
        /// </summary>
        NotSafeForWork
    }
}