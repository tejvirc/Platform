namespace Aristocrat.Monaco.Application.Contracts.Localization
{
    using System.Globalization;

    /// <summary>
    ///     Notification that the operator culture has changed.
    /// </summary>
    public class OperatorCultureChangedEvent : CultureChangedEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OperatorCultureChangedEvent"/> class.
        /// </summary>
        /// <param name="oldCulture">The old culture.</param>
        /// <param name="newCulture">The new culture.</param>
        public OperatorCultureChangedEvent(CultureInfo oldCulture, CultureInfo newCulture)
            : base(oldCulture, newCulture)
        {
        }
    }
}
