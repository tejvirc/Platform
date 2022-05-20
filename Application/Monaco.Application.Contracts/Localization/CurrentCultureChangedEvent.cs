namespace Aristocrat.Monaco.Application.Contracts.Localization
{
    using System.Globalization;

    /// <summary>
    ///     Notification that the current culture has changed.
    /// </summary>
    public class CurrentCultureChangedEvent : CultureChangedEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CurrentCultureChangedEvent"/> class.
        /// </summary>
        /// <param name="oldCulture">The old culture.</param>
        /// <param name="newCulture">The new culture.</param>
        public CurrentCultureChangedEvent(CultureInfo oldCulture, CultureInfo newCulture)
            : base (oldCulture, newCulture)
        {
        }
    }
}
