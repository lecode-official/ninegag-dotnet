
#region Using Directives

using System.Collections.Generic;

#endregion

namespace NineGag
{
    /// <summary>
    /// Represents the result of retrieving sections.
    /// </summary>
    public class SectionResult
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="SectionResult"/> instance.
        /// </summary>
        /// <param name="sections">All sections (this does not include local sections).</param>
        /// <param name="featuredSections">The featured (popular) sections.</param>
        /// <param name="localSections">The local sections, which are country/region specific sections.</param>
        /// <param name="currentLocalSection">
        /// The current local section, which is the section corresponding to the country/region of the user.
        /// </param>
        public SectionResult(
            IEnumerable<Section> sections,
            IEnumerable<Section> featuredSections,
            IEnumerable<Section> localSections,
            Section currentLocalSection)
        {
            this.Sections = sections;
            this.FeaturedSections = featuredSections;
            this.LocalSections = localSections;
            this.CurrentLocalSection = currentLocalSection;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets all sections (this does not include local sections).
        /// </summary>
        public IEnumerable<Section> Sections { get; private set; }

        /// <summary>
        /// Gets the featured (popular) sections.
        /// </summary>
        public IEnumerable<Section> FeaturedSections { get; private set; }

        /// <summary>
        /// Gets the local sections, which are country/region specific sections.
        /// </summary>
        public IEnumerable<Section> LocalSections { get; private set; }

        /// <summary>
        /// Gets the current local section, which is the section corresponding to the country/region of the user.
        /// </summary>
        public Section CurrentLocalSection { get; private set; }

        #endregion
    }
}
