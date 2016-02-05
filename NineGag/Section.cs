
#region Using Directives

using System;

#endregion

namespace NineGag
{
    /// <summary>
    /// Represents a section of the 9GAG website. A section is like a category, which contains posts of a distintive kind.
    /// </summary>
    public class Section
    {
        #region Public Properties

        /// <summary>
        /// Gets the original title of the section, as it is called on 9GAG:
        /// </summary>
        public string Title { get; internal set; }

        /// <summary>
        /// Gets the relative URI of the section.
        /// </summary>
        public Uri RelativeUri { get; internal set; }

        /// <summary>
        /// Gets the kind of the section, which makes some of well-known sections computer-readable.
        /// </summary>
        public SectionKind Kind { get; internal set; }

        #endregion
    }
}