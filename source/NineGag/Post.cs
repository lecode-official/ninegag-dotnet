
#region Using Directives

using System;
using Newtonsoft.Json;
using NineGag.Serialization;

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

        /// <summary>
        /// Gets the kind of the post.
        /// </summary>
        [JsonProperty("type")]
        public PostKind Kind { get; private set; }

        /// <summary>
        /// Gets a value that determines whether the post is not safe for work.
        /// </summary>
        [JsonProperty("nsfw")]
        public bool IsNotSafeForWork { get; private set; }

        /// <summary>
        /// Gets the number of up-votes that the post received.
        /// </summary>
        [JsonProperty("upVoteCount")]
        public int UpVoteCount { get; private set; }

        /// <summary>
        /// Gets the number of down-votes that the post received.
        /// </summary>
        [JsonProperty("downVoteCount")]
        public int DownVoteCount { get; private set; }

        /// <summary>
        /// Gets the date and time that the post was created in local time.
        /// </summary>
        [JsonProperty("creationTs")]
        [JsonConverter(typeof(TimestampToDateTimeConverter))]
        public DateTime CreationDateTime { get; private set; }

        /// <summary>
        /// Gets a value that determines whether the post is promoted.
        /// </summary>
        [JsonProperty("promoted")]
        public bool IsPromoted { get; private set; }

        /// <summary>
        /// Gets a value that determines whether the vote is masked.
        /// </summary>
        [JsonProperty("isVoteMasked")]
        public bool IsVoteMasked { get; private set; }

        /// <summary>
        /// Gets a value that determines whether the post has a long post cover.
        /// </summary>
        [JsonProperty("hasLongPostCover")]
        public bool HasLongPostCover { get; private set; }

        /// <summary>
        /// Gets the number of comments on the post.
        /// </summary>
        [JsonProperty("commentsCount")]
        public int CommentsCount { get; private set; }

        #endregion
    }
}
