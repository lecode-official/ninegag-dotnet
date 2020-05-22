
#region Using Directives

using AngleSharp;

#endregion

namespace NineGag
{
    /// <summary>
    /// Represents a section of the 9GAG website. A section is like a category, which contains posts of a distinctive topic.
    /// </summary>
    public class Section
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="Section"/> instance. This constructor is internal, because only the <see cref="NineGagClient"/> should instantiate sections.
        /// </summary>
        /// <param name="name">The name of the section.</param>
        /// <param name="description">The description of the section's content.</param>
        /// <param name="iconUrl">The URL to the icon of the section.</param>
        /// <param name="url">The URL of the section.</param>
        internal Section(string name, string description, string iconUrl, Url url)
        {
            this.Name = name;
            this.Description = description;
            this.IconUrl = iconUrl;
            this.Url = url;
        }

        #endregion

        #region Internal Properties

        /// <summary>
        /// Gets the URL of the section.
        /// </summary>
        internal Url Url { get; private set; }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the name of the section.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets a description of the section's content.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets the URL to the icon of the section.
        /// </summary>
        public string IconUrl { get; private set; }

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
