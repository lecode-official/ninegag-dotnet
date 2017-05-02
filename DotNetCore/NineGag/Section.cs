
#region Using Directives

using AngleSharp.Dom;
using System;

#endregion

namespace NineGag
{
    /// <summary>
    /// Represents a section of the 9GAG website. A section is like a category, which contains posts of a distintive kind.
    /// </summary>
    public class Section
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="Section"/> instance. The constructor is private, because <see cref="Section"/> implements a factory pattern.
        /// </summary>
        private Section() { }

        #endregion

        #region Private Static Fields

        /// <summary>
        /// Contains the 9GAG base URI.
        /// </summary>
        private static readonly Uri baseUri = new Uri("http://9gag.com", UriKind.Absolute);

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the original title of the section, as it is called on 9GAG:
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Gets the relative URI of the section.
        /// </summary>
        public Uri RelativeUri { get; private set; }

        /// <summary>
        /// Gets the kind of the section, which makes some of well-known sections computer-readable.
        /// </summary>
        public SectionKind Kind { get; private set; }

        #endregion

        #region Internal Static Methods

        /// <summary>
        /// Parses the specified DOM element and creates a section from it.
        /// </summary>
        /// <param name="sectionElement">The DOM element, which is to be parsed.</param>
        /// <returns>Returns the section, which was created from the DOM element.</returns>
        internal static Section FromHtml(IElement sectionElement)
        {
            // Parses the section kind, if the section kind could not be parsed, then the section kind is set to unknown
            SectionKind sectionKind;
            if (!Enum.TryParse(sectionElement.TextContent.Trim().Replace(" ", string.Empty), true, out sectionKind))
                sectionKind = SectionKind.Unknown;

            // Creates the new section and adds it to the list of sections
            return new Section
            {
                Title = sectionElement.TextContent.Trim(),
                Kind = sectionKind,
                RelativeUri = Section.baseUri.MakeRelativeUri(new Uri(sectionElement.GetAttribute("href"), UriKind.Absolute))
            };
        }

        #endregion
    }
}