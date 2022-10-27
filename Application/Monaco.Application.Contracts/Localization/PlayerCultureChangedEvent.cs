namespace Aristocrat.Monaco.Application.Contracts.Localization
{
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    ///     Notification that the player culture has changed.
    /// </summary>
    public class PlayerCultureChangedEvent : CultureChangedEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerCultureChangedEvent"/> class.
        /// </summary>
        /// <param name="oldCulture">The old culture.</param>
        /// <param name="newCulture">The new culture.</param>
        public PlayerCultureChangedEvent(CultureInfo oldCulture, CultureInfo newCulture)
            : base(oldCulture, newCulture) 
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerCultureChangedEvent"/> class.
        /// </summary>
        public PlayerCultureChangedEvent(string oldCulture, string newCulture)
            : base(new CultureInfo(oldCulture), new CultureInfo(newCulture))
        {
        }
    }
}
