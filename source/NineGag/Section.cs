
#region Using Directives

using System;
using Newtonsoft.Json;

#endregion

namespace NineGag
{
    /// <summary>
    /// Represents a section of the 9GAG website. A section is like a category, which contains posts of a distinctive topic.
    /// </summary>
    public struct Section : IEquatable<Section>
    {
        #region Public Properties

        /// <summary>
        /// Gets the name of the section.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the URL of the section.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; internal set; }

        /// <summary>
        /// Gets a description of the section's content.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; internal set; }

        /// <summary>
        /// Gets the URL to the JPEG version of the icon of the section.
        /// </summary>
        [JsonProperty("ogImageUrl")]
        public string ImageUrl { get; internal set; }

        /// <summary>
        /// Gets the URL to the WebP version of the icon of the section.
        /// </summary>
        [JsonProperty("ogWebpUrl")]
        public string WebpImageUrl { get; internal set; }

        /// <summary>
        /// Gets a value that determines whether users are allowed to upload content to this section.
        /// </summary>
        [JsonProperty("userUploadEnabled")]
        public bool IsUserUploadEnabled { get; internal set; }

        /// <summary>
        /// Gets a value that determines whether the section contains sensitive content.
        /// </summary>
        [JsonProperty("isSensitive")]
        public bool IsSensitive { get; internal set; }

        /// <summary>
        /// Gets the location code if the section is a local section. There are section on 9GAG for most countries/regions in the world,
        /// this is the code corresponding to that country/region. When this is not a local section, then the location is an empty string.
        /// </summary>
        [JsonProperty("location")]
        public string Location { get; internal set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Converts the <see cref="Section"/> into a human-readable string representation.
        /// </summary>
        /// <returns>Returns a human-readable string representation of the <see cref="Section"/>.</returns>
        public override string ToString() => $"{this.Name} - {this.Description}";

        /// <summary>
        /// Determines whether this section is equal to the other specified object.
        /// </summary>
        /// <param name="obj">The object, which is to be checked for equality to this section.</param>
        /// <returns>
        /// Returns <c>true</c> if the specified object is a <see cref="Section"/> and represents the same 9GAG section. Otherwise
        /// <c>false</c> is returned.
        /// </returns>
        public override bool Equals(object obj) => obj is Section ? this.Equals((Section)obj) : false;

        /// <summary>
        /// Gets a hash code, which uniquely identifies this section.
        /// </summary>
        /// <returns>Returns a hash code, which uniquely identifies this section.</returns>
        public override int GetHashCode() => this.Url.GetHashCode();

        #endregion

        #region Operators

        /// <summary>
        /// Determines whether the two sections represent the same 9GAG sections.
        /// </summary>
        /// <param name="firstSection">The left operand.</param>
        /// <param name="secondSection">The right operand.</param>
        /// <returns>
        /// Returns <c>true</c> if the two sections either represent the same section or are both <c>null</c>. Otherwise <c>false</c> is
        /// returned.
        /// </returns>
        public static bool operator ==(Section firstSection, Section secondSection)
            => firstSection == null ? secondSection == null : firstSection.Equals(secondSection);

        /// <summary>
        /// Determines whether the two sections represent different 9GAG sections.
        /// </summary>
        /// <param name="firstSection">The left operand.</param>
        /// <param name="secondSection">The right operand.</param>
        /// <returns>
        /// Returns <c>true</c> if the sections represent different 9GAG sections or one of them is <c>null</c> (but not both). Otherwise
        /// <c>null</c> is returned.
        /// </returns>
        public static bool operator !=(Section firstSection, Section secondSection) => !(firstSection == secondSection);

        #endregion

        #region IEquatable Implementation

        /// <summary>
        /// Determines whether the specified other section is equal to this section.
        /// </summary>
        /// <param name="other">The other section, which is to be checked for equality to this section.</param>
        /// <returns>
        /// Returns <c>true</c> if the other 9GAG section represents the same section as this section. Otherwise <c>false</c> is returned.
        /// </returns>
        public bool Equals(Section other) => this.Url == other.Url;

        #endregion
    }
}
