namespace Aristocrat.Monaco.Application.Contracts.Localization
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using Monaco.Localization;

    /// <summary>
    ///     Culture provider interface.
    /// </summary>
    public interface ICultureProvider : ILocalizer, INotifyPropertyChanged, INotifyCollectionChanged
    {
        /// <summary>
        ///     Gets the name for the provider.
        /// </summary>
        /// <returns>The name of the service.</returns>
        string ProviderName { get; }

        /// <summary>
        ///     Gets a list of cultures available for the player.
        /// </summary>
        IReadOnlyCollection<CultureInfo> AvailableCultures { get; }

        /// <summary>
        ///     Add cultures.
        /// </summary>
        /// <param name="cultures">Array of cultures to add.</param>
        CultureInfo[] AddCultures(params CultureInfo[] cultures);

        /// <summary>
        ///     Remove cultures.
        /// </summary>
        /// <param name="cultures">Array of cultures to add.</param>
        CultureInfo[] RemoveCultures(params CultureInfo[] cultures);

        /// <summary>
        ///     Determines if the specified culture is supported.
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <returns></returns>
        bool IsCultureAvailable(CultureInfo culture);

        /// <summary>
        ///     Perform initialization logic.
        /// </summary>
        /// <param name="localizer"><see cref="ILocalizationManager"/> instance.</param>
        /// <param name="propertyProvider"><see cref="ILocalizationPropertyProvider"/> instance.</param>
        void Initialize(ILocalizationManager localizer, ILocalizationPropertyProvider propertyProvider);

        /// <summary>
        ///     Perform configuration logic.
        /// </summary>
        void Configure();

        /// <summary>
        ///     Switches the <see cref="ILocalization"/> service to the current culture of the provider.
        /// </summary>
        void SwitchTo();
    }
}
