namespace Aristocrat.Monaco.Localization
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;

    /// <summary>
    ///     Provides localization services.
    /// </summary>
    [CLSCompliant(true)]
    public interface ILocalizationManager
    {
        /// <summary>
        ///     Gets or sets the current culture.
        /// </summary>
        CultureInfo CurrentCulture { get; set; }

        /// <summary>
        ///     Gets the available cultures.
        /// </summary>
        IReadOnlyList<CultureInfo> AvailableCultures { get; }

        /// <summary>
        ///     Starts the localization provider services.
        /// </summary>
        void Start();

        /// <summary>
        ///     Starts the localization provider services.
        /// </summary>
        /// <param name="resourceAssembly">The default resource assembly.</param>
        /// <param name="resourceDictionary">The default dictionary to look up.</param>
        void Start(Assembly resourceAssembly, string resourceDictionary);

        /// <summary>
        ///     Updates the list of available cultures using the given resource location.
        /// </summary>
        /// <param name="resourceAssembly">The resource assembly.</param>
        /// <param name="resourceDictionary">The dictionary to look up.</param>
        /// <returns>True, if the update was successful.</returns>
        void LoadResources(Assembly resourceAssembly, string resourceDictionary);

        /// <summary>
        ///     Adds supported cultures.
        /// </summary>
        /// <param name="cultures">Culture to add.</param>
        void AddCultures(params CultureInfo[] cultures);

        /// <summary>
        ///     Gets the localized resource for a specified key.
        /// </summary>
        /// <param name="key">Resource key.</param>
        /// <typeparam name="TResource">The type of the resource to retrieve.</typeparam>
        /// <returns>The resource form the specified key.</returns>
        TResource GetObject<TResource>(string key);

        /// <summary>
        ///     Gets the localized resource for a specified key.
        /// </summary>
        /// <param name="key">Resource key.</param>
        /// <param name="culture">The target culture.</param>
        /// <typeparam name="TResource">The type of the resource to retrieve.</typeparam>
        /// <returns>The resource form the specified key.</returns>
        TResource GetObject<TResource>(string key, CultureInfo culture);

        /// <summary>
        ///     Notification that a providers culture has changed.
        /// </summary>
        /// <param name="name">The provider name.</param>
        /// <param name="newCulture">The new culture.</param>
        void NotifyCultureChanged(string name, CultureInfo newCulture);
    }
}
