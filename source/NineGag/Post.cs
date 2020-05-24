
#region Using Directives

using Newtonsoft.Json;

#endregion

namespace NineGag
{
    /// <summary>
    /// Represents 9GAG post.
    /// </summary>
    public class Post
    {
        #region Public Properties

        /// <summary>
        /// Gets the unique ID of the post.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; private set; }

        /// <summary>
        /// Gets the URL of the post.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; private set; }

        /// <summary>
        /// Gets the title of the post.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; private set; }

        /// <summary>
        /// Gets the description of the post.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; private set; }

        #endregion
    }
}
