namespace Aristocrat.Monaco.Application.Contracts.Localization
{
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    ///     Notification that the culture has changed.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public abstract class CultureChangedEvent : LocalizationEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CurrentCultureChangedEvent"/> class.
        /// </summary>
        /// <param name="oldCulture">The old culture.</param>
        /// <param name="newCulture">The new culture.</param>
        protected CultureChangedEvent(CultureInfo oldCulture, CultureInfo newCulture)
        {
            OldCulture = oldCulture;
            NewCulture = newCulture;
        }

        /// <summary>
        ///     Gets the old culture.
        /// </summary>
        public CultureInfo OldCulture { get; }

        /// <summary>
        ///     Gets the new culture.
        /// </summary>
        public CultureInfo NewCulture { get; }
    }
}
