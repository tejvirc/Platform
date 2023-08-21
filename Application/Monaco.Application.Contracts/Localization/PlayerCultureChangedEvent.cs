namespace Aristocrat.Monaco.Application.Contracts.Localization
{
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    ///     Notification that the player culture has changed.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class PlayerCultureChangedEvent : CultureChangedEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerCultureChangedEvent"/> class.
        /// </summary>
        /// <param name="oldCulture">The old culture.</param>
        /// <param name="newCulture">The new culture.</param>
        /// <param name="isPrimary">Indicates whether the new culture is the primary culture</param>
        public PlayerCultureChangedEvent(CultureInfo oldCulture, CultureInfo newCulture, bool isPrimary)
            : base(oldCulture, newCulture)
        {
            IsPrimary = isPrimary;
        }

        /// <summary>
        ///     Gets a value that indicates whether the new culture is the primary culture.
        /// </summary>
        public bool IsPrimary { get; }
    }
}
