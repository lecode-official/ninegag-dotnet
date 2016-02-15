
namespace NineGag
{
    /// <summary>
    /// Represents an enumeration for the different actuality levels of 9GAG posts.
    /// </summary>
    public enum PostActuality
    {
        /// <summary>
        /// The posts have gotten a lot of upvotes and are very popular among 9GAG users.
        /// </summary>
        Hot,

        /// <summary>
        /// The posts have gotten quite a few upvotes and are trending among 9GAG users. Trending can not be used in conjunction with sections.
        /// </summary>
        Trending,

        /// <summary>
        /// The posts have very recently uploaded to 9GAG and do not have a lot of upvotes.
        /// </summary>
        Fresh
    }
}