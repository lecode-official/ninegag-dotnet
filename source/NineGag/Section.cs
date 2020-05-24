
#region Using Directives

using Newtonsoft.Json;

#endregion

namespace NineGag
{
    /// <summary>
    /// Represents a section of the 9GAG website. A section is like a category, which contains posts of a distinctive topic.
    /// </summary>
    public class Section
    {
        #region Public Properties

        /// <summary>
        /// Gets the name of the section.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the URL of the section.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; private set; }

        /// <summary>
        /// Gets a description of the section's content.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; private set; }

        /// <summary>
        /// Gets the URL to the JPEG version of the icon of the section.
        /// </summary>
        [JsonProperty("ogImageUrl")]
        public string ImageUrl { get; private set; }

        /// <summary>
        /// Gets the URL to the WebP version of the icon of the section.
        /// </summary>
        [JsonProperty("ogWebpUrl")]
        public string WebpImageUrl { get; private set; }

        /// <summary>
        /// Gets a value that determines whether users are allowed to upload content to this section.
        /// </summary>
        [JsonProperty("userUploadEnabled")]
        public bool IsUserUploadEnabled { get; private set; }

        /// <summary>
        /// Gets a value that determines whether the section contains sensitive content.
        /// </summary>
        [JsonProperty("isSensitive")]
        public bool IsSensitive { get; private set; }

        /// <summary>
        /// Gets the location code if the section is a local section. There are section on 9GAG for most countries/regions in the world,
        /// this is the code corresponding to that country/region. When this is not a local section, then the location is an empty string.
        /// </summary>
        [JsonProperty("location")]
        public string Location { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Converts the <see cref="Section"/> into a human-readable string representation.
        /// </summary>
        /// <returns>Returns a human-readable string representation of the <see cref="Section"/>.</returns>
        public override string ToString() => $"{this.Name} - {this.Description}";

        #endregion
    }
}
