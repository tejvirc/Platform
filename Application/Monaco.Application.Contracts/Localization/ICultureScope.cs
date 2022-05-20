namespace Aristocrat.Monaco.Application.Contracts.Localization
{
    using System;

    /// <summary>
    ///     This interface is used to retrieve a localized object for the current culture of the culture provider.
    /// </summary>
    public interface ICultureScope : IDisposable
    {
        /// <summary>
        ///     Gets the localized resource for a given key based on the current language in scope.
        /// </summary>
        /// <typeparam name="TResource">The type of the return value.</typeparam>
        /// <param name="key">The key used to lookup the resource.</param>
        /// <param name="options">Localization options.</param>
        /// <returns>Returns the localized resource for the specified key.</returns>
        TResource GetObject<TResource>(string key, LocalizeOptions options = LocalizeOptions.None);

        /// <summary>
        ///     Gets the localized string value for a given key based on the current language in scope.
        /// </summary>
        /// <param name="key">The key to lookup the text value.</param>
        /// <param name="options">Localization options.</param>
        /// <returns>Returns the localized string for the specified key.</returns>
        string GetString(string key, LocalizeOptions options = LocalizeOptions.None);

        /// <summary>
        ///     Gets the localized string value for a given key based on the current language in scope. Use this
        ///     method when the string returned needs to be formatted.
        /// </summary>
        /// <param name="key">The key used to lookup the resource.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns>Returns the localized string for the specified key.</returns>
        string FormatString(string key, params object[] args);

        /// <summary>
        ///     Gets the localized string value for a given key based on the current language in scope. Use this
        ///     method when the string returned needs to be formatted.
        /// </summary>
        /// <param name="key">The key used to lookup the resource.</param>
        /// <param name="options">Localization options.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns>Returns the localized string for the specified key.</returns>
        string FormatString(string key, LocalizeOptions options = LocalizeOptions.None, params object[] args);
    }
}
