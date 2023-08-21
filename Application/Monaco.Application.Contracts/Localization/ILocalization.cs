namespace Aristocrat.Monaco.Application.Contracts.Localization
{
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    ///     This is the contract for multi-language support services.
    /// </summary>
    public interface ILocalization : ILocalizerFactory
    {
        /// <summary>
        ///     Gets the current region info
        /// </summary>
        RegionInfo RegionInfo { get; }

        /// <summary>
        ///     Gets the default culture.
        /// </summary>
        CultureInfo DefaultCulture { get; }

        /// <summary>
        ///     Gets the current culture.
        /// </summary>
        /// <remarks>This property should only be set by <see cref="ICultureProvider" /> derived classes.</remarks>
        CultureInfo CurrentCulture { get; set; }

        /// <summary>
        ///     Gets the supported cultures.
        /// </summary>
        IReadOnlyList<CultureInfo> SupportedCultures { get; }

        /// <summary>
        ///     Get the provider with specified name.
        /// </summary>
        /// <param name="name">Provider name.</param>
        /// <returns>The culture provider with the specied name.</returns>
        ICultureProvider GetProvider(string name);

        /// <summary>
        ///     Get the provider with specified name.
        /// </summary>
        /// <param name="name">Provider name.</param>
        /// <param name="provider">The culture provider with the specied name.</param>
        /// <returns>True, if the provider with the specified name is found.</returns>
        bool TryGetProvider(string name, out ICultureProvider provider);

        /// <summary>
        ///     Determines if the specified culture is supported.
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <returns>True if culture is supported.</returns>
        bool IsCultureSupported(CultureInfo culture);
    }
}