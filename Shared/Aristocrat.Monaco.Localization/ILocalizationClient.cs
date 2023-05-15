namespace Aristocrat.Monaco.Localization
{
    using System.Globalization;

    /// <summary>
    ///     Client for localization services.
    /// </summary>
    public interface ILocalizationClient
    {
        /// <summary>
        ///     Allows for the culture to be set prior to retrieving the localized resource.
        /// </summary>
        /// <param name="name">The culture provider name.</param>
        /// <returns>The new target culture</returns>
        CultureInfo GetCultureFor(string name);

        /// <summary>
        ///     Notification that a localization error has occurred.
        /// </summary>
        /// <param name="args">Error information</param>
        void LocalizationError(LocalizationErrorArgs args);
    }
}
